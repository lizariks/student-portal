using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Text.Json;
using StudentPortal.DiscussionService.Domain.Exceptions;

namespace StudentPortal.DiscussionService.Api.Middleware;
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                _logger.LogError(ex, "Unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Title = "An error occurred",
                Status = StatusCodes.Status500InternalServerError,
                Detail = exception.Message,
                Instance = context.Request.Path
            };

            switch (exception)
            {
                case ValidationException ve:
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "Validation Error";
                    problemDetails.Extensions["errors"] =
                        ve.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;

                case AutoMapperMappingException amex when amex.InnerException is DomainException de:
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "Domain Validation Error";
                    problemDetails.Detail = de.Message;
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;

                case InvalidContentException:
                case InvalidRatingException:
                case ThreadClosedException:
                case UnauthorizedActionException:
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "Domain Validation Error";
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;

                case NotFoundException:
                    problemDetails.Status = StatusCodes.Status404NotFound;
                    problemDetails.Title = "Not Found";
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    break;

                case MongoConnectionException:
                    problemDetails.Status = StatusCodes.Status503ServiceUnavailable;
                    problemDetails.Title = "Database Connection Error";
                    context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    break;

                case MongoWriteException writeEx:
                    problemDetails.Status = writeEx.WriteError.Category == ServerErrorCategory.DuplicateKey
                        ? StatusCodes.Status409Conflict
                        : StatusCodes.Status400BadRequest;
                    problemDetails.Title = "Database Write Error";
                    context.Response.StatusCode = problemDetails.Status.Value;
                    break;

                default:
                    problemDetails.Status = StatusCodes.Status500InternalServerError;
                    problemDetails.Title = "Internal Server Error";
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            var json = JsonSerializer.Serialize(problemDetails);
            return context.Response.WriteAsync(json);
        }
    }
