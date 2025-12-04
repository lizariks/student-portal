namespace StudentPortal.AppHost.Extensions;

using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

public static class KeycloakConfigurationExtensions
{
    private const string KeycloakBaseUrl = "http://localhost:8080";
    private const string KeycloakAdminUsername = "admin";
    private const string KeycloakAdminPassword = "admin";
    private const string KeycloakAdminClient = "admin-cli";
    
    private const string RealmName = "StudentPortal"; 

    private const string PulumiStackName = "dev";
    private const string PulumiCliPath = @"C:\Program Files (x86)\Pulumi\pulumi.exe";
    private const string PulumiProjectDirName = "StudentPortal.Pulumi";
    private const string PulumiConfigFileName = "Pulumi.dev.yaml";

    private const string ConfigureScriptName = "configure-keycloak.ps1";

    private const int PostDeploymentDelayMs = 3000;
    private const int MaxParentDirectoryLevels = 5;

    private const string LoggerCategoryName = "KeycloakAutoConfig";

    /// <summary>
    /// Automatically configures Keycloak realm using Pulumi when the container starts.
    /// </summary>
    public static IResourceBuilder<T> WithAutoConfiguration<T>(
        this IResourceBuilder<T> builder) where T : ContainerResource
    {
        var solutionRoot = FindSolutionRoot(builder.ApplicationBuilder.Environment.ContentRootPath);

        if (solutionRoot == null)
        {
            LogWarning(builder, $"Could not find '{PulumiProjectDirName}' folder - skipping auto-configuration");
            return builder;
        }

        // Підписка на подію готовності ресурсу (Keycloak контейнера)
        builder.ApplicationBuilder.Eventing.Subscribe<ResourceReadyEvent>(builder.Resource,
            async (@event, ct) => await ConfigureKeycloakAsync(builder, solutionRoot, ct));

        return builder;
    }

    private static async Task ConfigureKeycloakAsync<T>(
        IResourceBuilder<T> builder,
        string solutionRoot,
        CancellationToken ct) where T : ContainerResource
    {
        ILogger? logger = null;
        try
        {
            logger = CreateLogger(builder);

            // Перевірка наявності Pulumi
            if (!File.Exists(PulumiCliPath))
            {
                logger.LogWarning("Pulumi CLI not found at: {PulumiPath}", PulumiCliPath);
                logger.LogWarning("Please verify Pulumi installation path. Skipping auto-configuration.");
                return;
            }

            logger.LogInformation("Waiting for Keycloak to become available...");

            var ready = await WaitForKeycloakAsync(logger, ct);
            if (!ready)
            {
                logger.LogWarning("Keycloak did not become ready in time. Skipping auto configuration.");
                return;
            }

            // Перевірка, чи Realm вже існує (чи конфігурація вже застосована)
            if (await IsKeycloakAlreadyConfiguredAsync(logger, ct))
            {
                logger.LogInformation("Keycloak is already configured - skipping auto-configuration");
                return;
            }

            logger.LogInformation("Keycloak not configured yet - starting auto-configuration...");

            var infrastructurePath = Path.Combine(solutionRoot, PulumiProjectDirName);

            if (!Directory.Exists(infrastructurePath))
            {
                logger.LogWarning("Pulumi project directory not found: {Path}", infrastructurePath);
                return;
            }

            await EnsurePulumiStackExistsAsync(infrastructurePath, logger, ct);

            logger.LogInformation("Deploying Keycloak configuration via Pulumi...");
            
            // Запуск Pulumi Up
            var success = await RunPulumiCommandAsync("up --yes --skip-preview",
                infrastructurePath, logger, ct);

            if (!success)
            {
                logger.LogWarning("Pulumi deployment failed — check logs above.");
                return;
            }

            await Task.Delay(PostDeploymentDelayMs, ct);

            // Запуск PowerShell скрипту (якщо існує)
            var scriptPath = Path.Combine(solutionRoot, ConfigureScriptName);
            if (File.Exists(scriptPath))
            {
                logger.LogInformation("Running configure-keycloak.ps1 for role-to-scope mappings...");
                var scriptArgs = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"";

                var scriptSuccess = await RunProcessAsync("powershell", scriptArgs, solutionRoot, logger, ct);

                if (!scriptSuccess)
                {
                    logger.LogWarning("configure-keycloak.ps1 execution failed.");
                    return;
                }
            }
            else
            {
                logger.LogInformation("PowerShell script not found, skipping: {ScriptPath}", scriptPath);
            }

            logger.LogInformation("Keycloak auto-configuration completed successfully!");
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Keycloak auto-config failed.");
        }
    }

    private static async Task EnsurePulumiStackExistsAsync(
        string infrastructurePath,
        ILogger logger,
        CancellationToken ct)
    {
        // Налаштування локального backend (без необхідності логіну)
        logger.LogInformation("Configuring Pulumi to use local backend...");
        await RunPulumiCommandAsync("login --local",
            infrastructurePath, logger, ct);

        if (await CheckPulumiStackExistsAsync(infrastructurePath, logger, ct))
        {
            logger.LogInformation("Pulumi stack '{StackName}' already exists", PulumiStackName);
            return;
        }

        logger.LogInformation("Initializing Pulumi stack '{StackName}'...", PulumiStackName);

        await RunPulumiCommandAsync($"stack init {PulumiStackName}",
            infrastructurePath, logger, ct);

        var configPath = Path.Combine(infrastructurePath, PulumiConfigFileName);
        var configContent = $@"config:
  keycloak:url: {KeycloakBaseUrl}
  keycloak:username: {KeycloakAdminUsername}
  keycloak:password: {KeycloakAdminPassword}
  keycloak:clientId: {KeycloakAdminClient}
  studentportal:realmName: {RealmName}
";
        await File.WriteAllTextAsync(configPath, configContent, ct);
        logger.LogInformation("Pulumi config created at: {ConfigPath}", configPath);
    }

    private static async Task<bool> CheckPulumiStackExistsAsync(
        string infrastructurePath,
        ILogger logger,
        CancellationToken ct)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = PulumiCliPath,
                    Arguments = "stack ls --json",
                    WorkingDirectory = infrastructurePath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);

            return output.Contains($"\"name\":\"{PulumiStackName}\"");
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to check Pulumi stack existence");
            return false;
        }
    }

    private static async Task<bool> IsKeycloakAlreadyConfiguredAsync(ILogger logger, CancellationToken ct)
    {
        try
        {
            using var httpClient = new HttpClient();
            var realmUrl = $"{KeycloakBaseUrl}/realms/{RealmName}";

            var response = await httpClient.GetAsync(realmUrl, ct);

            if (response.IsSuccessStatusCode)
            {
                logger.LogDebug("{RealmName} realm exists - configuration present", RealmName);
                return true;
            }

            logger.LogDebug("{RealmName} realm not found - needs configuration", RealmName);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Could not check realm existence - assuming not configured");
            return false;
        }
    }

    private static string? FindSolutionRoot(string startPath)
    {
        var currentDir = new DirectoryInfo(startPath);

        for (int i = 0; i < MaxParentDirectoryLevels && currentDir != null; i++)
        {
            var infrastructurePath = Path.Combine(currentDir.FullName, PulumiProjectDirName);
            if (Directory.Exists(infrastructurePath))
            {
                return currentDir.FullName;
            }
            currentDir = currentDir.Parent;
        }

        return null;
    }

    private static async Task<bool> RunPulumiCommandAsync(
        string arguments,
        string workingDirectory,
        ILogger logger,
        CancellationToken ct)
    {
        // Встановлюємо змінну оточення для локального backend
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = PulumiCliPath,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        // Налаштування локального backend
        process.StartInfo.Environment["PULUMI_BACKEND_URL"] = "file://~";

        return await ExecuteProcessAsync(process, logger, ct);
    }

    private static async Task<bool> ExecuteProcessAsync(
        Process process,
        ILogger logger,
        CancellationToken ct)
    {
        try
        {
            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    logger.LogDebug(e.Data);
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    logger.LogWarning(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(ct);

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute process");
            return false;
        }
    }

    private static async Task<bool> RunProcessAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        ILogger logger,
        CancellationToken ct)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            return await ExecuteProcessAsync(process, logger, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to run process: {FileName} {Arguments}", fileName, arguments);
            return false;
        }
    }

    private static ILogger CreateLogger<T>(IResourceBuilder<T> builder) where T : ContainerResource
    {
        var loggerFactory = builder.ApplicationBuilder.Services
            .BuildServiceProvider()
            .GetRequiredService<ILoggerFactory>();

        return loggerFactory.CreateLogger(LoggerCategoryName);
    }

    private static void LogWarning<T>(IResourceBuilder<T> builder, string message) where T : ContainerResource
    {
        var logger = CreateLogger(builder);
        logger.LogWarning(message);
    }

    private static async Task<bool> WaitForKeycloakAsync(ILogger logger, CancellationToken ct)
    {
        using var httpClient = new HttpClient();
        var healthUrl = $"{KeycloakBaseUrl}/realms/master/.well-known/openid-configuration";

        const int maxAttempts = 30;
        for (int i = 1; i <= maxAttempts; i++)
        {
            try
            {
                var response = await httpClient.GetAsync(healthUrl, ct);
                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Keycloak is ready");
                    return true;
                }
            }
            catch
            {
                // Ignore exceptions during wait
            }

            logger.LogInformation("Waiting for Keycloak... attempt {Attempt}/{MaxAttempts}", i, maxAttempts);
            await Task.Delay(2000, ct);
        }

        return false;
    }
}