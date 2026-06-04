namespace StudentPortal.CourseCatalogService.Apii.MiddleWare;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Net;
using System.Text.Json;
public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var problemDetails = new ProblemDetails
            {
                Instance = context.Request.Path,
                Title = "An unexpected error occurred.",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = exception.Message
            };

            switch (exception)
            {
                case KeyNotFoundException:
                    problemDetails.Title = "Resource not found.";
                    problemDetails.Status = (int)HttpStatusCode.NotFound;
                    break;

                case ValidationException validationEx:
                    problemDetails.Title = "Validation error.";
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Extensions["errors"] = validationEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );
                    break;

                case InvalidOperationException:
                    problemDetails.Title = "Conflict occurred.";
                    problemDetails.Status = (int)HttpStatusCode.Conflict;
                    break;

                case DbUpdateException dbEx when dbEx.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation:
                    problemDetails.Title = "Duplicate key violation.";
                    problemDetails.Detail = "Such relation already exists";
                    problemDetails.Status = (int)HttpStatusCode.Conflict; 
                    break;

                default:
                    problemDetails.Title = "Server error.";
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

            var json = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(json);
        }
    }