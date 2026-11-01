using TwitchUniConnect.Social.Api;
using TwitchUniConnect.Social.Application.Services;
using TwitchUniConnect.Social.DAL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSocialPersistence(builder.Configuration);

builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<InteractionService>();
builder.Services.AddScoped<FriendshipService>();
builder.Services.AddScoped<MessagingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SocialDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseExceptionHandler();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TwitchUniConnect Social API v1");
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();