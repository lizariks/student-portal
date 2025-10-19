using FluentValidation;
using MediatR;
using StudentPortal.DiscussionService.Domain.Interfaces.Repositories;
using StudentPortal.DiscussionService.Infrastructure.Repositories;
using StudentPortal.DiscussionService.Application.Services;
using StudentPortal.DiscussionService.Application.Behaviors;
using StudentPortal.DiscussionService.Application.Interfaces.Queries;
using StudentPortal.DiscussionService.Infrastructure;
using StudentPortal.DiscussionService.Infrastructure.Indexes;

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



builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<IIndexCreation, MongoIndexCreation>();

builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICourseReviewRepository, CourseReviewRepository>();
builder.Services.AddScoped<IDiscussionThreadRepository, DiscussionThreadRepository>();

builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ICourseReviewService, CourseReviewService>();
builder.Services.AddScoped<IDiscussionThreadService, DiscussionThreadService>();