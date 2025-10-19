using FluentValidation;
using MediatR;
using StudentPortal.DiscussionService.Application.Services;
using StudentPortal.DiscussionService.Application.Behaviors;
using StudentPortal.DiscussionService.Application.Interfaces.Queries;

using StudentPortal.DiscussionService.Domain.Interfaces.Services;

var builder = WebApplication.CreateBuilder(args);

//mediatr
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IQuery<>).Assembly);
    cfg.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
    cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
});

builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ICourseReviewService, CourseReviewService>();
builder.Services.AddScoped<IDiscussionThreadService, DiscussionThreadService>();