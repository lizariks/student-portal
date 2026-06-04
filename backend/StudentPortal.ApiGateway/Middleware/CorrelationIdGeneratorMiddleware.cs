namespace StudentPortal.ApiGateway.Middleware;

using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;
using System.Threading.Tasks;

    /// <summary>
    /// Відповідає за генерацію та встановлення X-Correlation-Id.
    /// Працює як єдина точка входу (Entry Point) для трасування.
    /// </summary>
    public class CorrelationIdGeneratorMiddleware
    {
        private const string CorrelationHeader = "X-Correlation-Id";
        private readonly RequestDelegate _next;

        public CorrelationIdGeneratorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. ПЕРЕВІРКА: Спробувати отримати ID з вхідних HTTP-заголовків
            var correlationId = context.Request.Headers.TryGetValue(CorrelationHeader, out var existingId)
                ? existingId.ToString()
                : Guid.NewGuid().ToString(); // 2. ГЕНЕРАЦІЯ: Створити новий GUID, якщо заголовок відсутній

            // 3. ЗБЕРІГАННЯ: Зберегти ID у HttpContext.Items.
            // YARP і Transform'и можуть автоматично використовувати цей ID для вихідних запитів.
            context.Items[CorrelationHeader] = correlationId;

            // 4. RESPONSE HEADER: Додати Correlation ID до відповіді, щоб клієнт міг його отримати.
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationHeader] = correlationId;
                return Task.CompletedTask;
            });

            // 5. SERILOG: Додати ID до LogContext.PushProperty.
            // Це гарантує, що всі наступні лог-записи (від YarpProxyLoggingMiddleware та інших)
            // автоматично включатимуть цей ID.
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                // Логування початку запиту для аудиту
                Log.Information("CorrelationId established: {CorrelationId}", correlationId);

                await _next(context);
            }
        }
    }