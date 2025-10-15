using FluentValidation;
using StudentPortal.DiscussionService.Application.Services;
using StudentPortal.DiscussionService.Domain.Interfaces.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ICourseReviewService, CourseReviewService>();
builder.Services.AddScoped<IDiscussionThreadService, DiscussionThreadService>();