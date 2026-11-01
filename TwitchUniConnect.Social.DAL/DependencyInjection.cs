using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TwitchUniConnect.Social.Application.Interfaces;

namespace TwitchUniConnect.Social.DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddSocialPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SocialDatabase")
            ?? throw new InvalidOperationException("Connection string 'SocialDatabase' is not configured.");

        services.AddDbContext<SocialDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SocialDbContext>());
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IDomainEventDispatcher, LoggingDomainEventDispatcher>();

        return services;
    }
}
