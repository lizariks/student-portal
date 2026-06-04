# configure-keycloak.ps1
# Цей скрипт виконується автоматично через Aspire AppHost після розгортання Pulumi

# --- 1. КОНСТАНТИ (Мають співпадати з Pulumi та AppHost) ---
$KeycloakBaseUrl = "http://localhost:8080"
$RealmName = "StudentPortal"
$AdminUser = "admin"
$AdminPassword = "admin"
$AdminCli = "admin-cli"

$RealmUrl = "$($KeycloakBaseUrl)/realms/$($RealmName)"
$AdminTokenUrl = "$($KeycloakBaseUrl)/realms/master/protocol/openid-connect/token"

Write-Host "--- Keycloak Configuration Script Started ---"

# --- 2. ФУНКЦІЯ ОТРИМАННЯ АДМІН ТОКЕНА ---
function Get-AdminAccessToken {
    Write-Host "Attempting to get Admin Access Token..."
    
    $Body = @{
        client_id = $AdminCli
        username = $AdminUser
        password = $AdminPassword
        grant_type = "password"
    }

    try {
        $Response = Invoke-RestMethod -Uri $AdminTokenUrl -Method Post -Body $Body -ContentType "application/x-www-form-urlencoded"
        return $Response.access_token
    }
    catch {
        Write-Error "Failed to get Admin Token. Keycloak may not be fully ready."
        return $null
    }
}

# --- 3. ФУНКЦІЯ ВИКОНАННЯ АДМІН ЗАПИТУ ---
function Invoke-KeycloakAdminApi {
    param(
        [Parameter(Mandatory=$true)]$AccessToken,
        [Parameter(Mandatory=$true)]$Endpoint,
        [Parameter(Mandatory=$true)]$Method,
        [Parameter(Mandatory=$false)]$Body
    )
    
    $Headers = @{
        "Authorization" = "Bearer $($AccessToken)"
        "Content-Type" = "application/json"
    }
    
    $ApiUrl = "$($KeycloakBaseUrl)/admin/realms/$($RealmName)/$($Endpoint)"
    
    try {
        if ($Body) {
            $BodyJson = $Body | ConvertTo-Json
            Invoke-RestMethod -Uri $ApiUrl -Method $Method -Headers $Headers -Body $BodyJson
        }
        else {
            Invoke-RestMethod -Uri $ApiUrl -Method $Method -Headers $Headers
        }
        return $true
    }
    catch {
        Write-Error "API call to $($Endpoint) failed: $($_.Exception.Message)"
        return $false
    }
}

# --- 4. ОСНОВНА ЛОГІКА СКРИПТА ---

$AccessToken = Get-AdminAccessToken

if (-not $AccessToken) {
    exit 1
}

Write-Host "Successfully obtained Admin Token."

# ПРИКЛАД: Створення або оновлення Client Scope Protocol Mappers 
# (Якщо Pulumi не підтримує якийсь специфічний мапер, тут ви можете його додати)

# ПРИКЛАД: Отримання ID Client-а
# $Clients = Invoke-KeycloakAdminApi -AccessToken $AccessToken -Endpoint "clients" -Method Get
# $ApiClientId = ($Clients | Where-Object { $_.clientId -eq "studentportal_api_service" }).id

# Write-Host "--- Keycloak Configuration Script Finished ---"
exit 0