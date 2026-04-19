using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Minio;
using SyncChat.API.Infrastructure.AuthProviders.Google;
using SyncChat.API.Infrastructure.Notification;
using SyncChat.API.Infrastructure.Outbox;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Infrastructure.Socket;
using SyncChat.API.Infrastructure.Storage;
using SyncChat.API.Shared.AuthProviders.Contracts;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Events;
using SyncChat.API.Shared.Notification.Contracts;
using SyncChat.API.Shared.Security.Contracts;
using SyncChat.API.Shared.Socket.Contracts;
using SyncChat.API.Shared.Storage.Contracts;
using SyncChat.API.Infrastructure.Services;
using System.Text;
using static SyncChat.API.Shared.Constants.EndpointConstants;
using static SyncChat.API.Shared.Constants.StorageConstants;

namespace SyncChat.API.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IOptions<JWTSettings> jwtOptions) =>
            services.AddAuthenticationInternal(jwtOptions.Value)
                    .AddPersistence(configuration)
                    .AddOpenApiInternal()
                    .AddIdentityServicesInternal()
                    .AddSocketServicesInternal()
                    .AddNotificationServices()
                    .AddSecurity()
                    .AddAuthProviders()
                    .AddStorage()
                    .AddMediaServices()
                    .AddOutboxServices();


    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services, JWTSettings jwtSettings)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.AccessTokenSecret)),
                    ClockSkew = TimeSpan.Zero
                };

                // This is for SignalR
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments(HubRoute.NotificationHub))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    //private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    //{
    //    services.AddAuthorization();

    //    services.AddScoped<PermissionProvider>();

    //    services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

    //    services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

    //    return services;
    //}

    private static IServiceCollection AddPersistence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MapEnum<UserStatus>("user_status");
                }));
        return services;
    }

    private static IServiceCollection AddOpenApiInternal(
        this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        return services;
    }

    private static IServiceCollection AddIdentityServicesInternal(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IIdentityService, IdentityService>();

        return services;
    }

    private static IServiceCollection AddSocketServicesInternal(this IServiceCollection services)
    {
        services.AddSingleton<IUserConnectionManager, UserConnectionManager>();
        services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
        return services;
    }

    private static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        services.AddScoped<IMessageNotificationService, SignalRMessageNotificationService>();
        services.AddSingleton<IEmailService, SmtpEmailService>();
        services.AddSingleton<IPasswordResetTokenService, PasswordResetTokenService>();

        return services;
    }

    private static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();
        services.AddSingleton<ICookieOptionsProvider, CookieOptionsProvider>();
        services.AddScoped<IRefreshTokenCookieManager, RefreshTokenCookieManager>();
        services.AddHostedService<RefreshTokenCleanupService>();

        return services;
    }

    private static IServiceCollection AddAuthProviders(this IServiceCollection services)
    {
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();

        return services;
    }

    private static IServiceCollection AddStorage(this IServiceCollection services)
    {
        // Internal client: reaches MinIO via Docker hostname for all real I/O operations.
        services.AddKeyedSingleton<IMinioClient>(MinioClientKeys.Internal, (serviceProvider, _) =>
        {
            var s = serviceProvider.GetRequiredService<IOptions<StorageSettings>>().Value;

            return new MinioClient()
                .WithEndpoint(s.Endpoint, s.Port)
                .WithCredentials(s.AccessKey, s.SecretKey)
                .WithSSL(s.UseSSL)
                .Build();
        });

        // Presign client: configured with the browser-facing public URL so that
        // generated presigned URLs embed the correct host in their HMAC signature.
        // Presigned URL generation is pure local computation — no network call is made.
        services.AddKeyedSingleton<IMinioClient>(MinioClientKeys.Presign, (serviceProvider, _) =>
        {
            var s = serviceProvider.GetRequiredService<IOptions<StorageSettings>>().Value;

            if (!string.IsNullOrEmpty(s.PublicUrl))
            {
                var publicUri = new Uri(s.PublicUrl);
                int port = publicUri.IsDefaultPort
                    ? (publicUri.Scheme == "https" ? 443 : 80)
                    : publicUri.Port;

                return new MinioClient()
                    .WithEndpoint(publicUri.Host, port)
                    .WithCredentials(s.AccessKey, s.SecretKey)
                    .WithSSL(publicUri.Scheme == "https")
                    .Build();
            }

            // Fallback: no public URL configured, use same endpoint as internal client.
            return new MinioClient()
                .WithEndpoint(s.Endpoint, s.Port)
                .WithCredentials(s.AccessKey, s.SecretKey)
                .WithSSL(s.UseSSL)
                .Build();
        });

        services.AddScoped<IBlobStorage, MinioBlobStorage>();
        services.AddHostedService<StorageInitializationService>();

        return services;
    }

    private static IServiceCollection AddMediaServices(this IServiceCollection services)
    {
        services.AddHostedService<StaleMediaCleanupService>();
        services.AddHostedService<OrphanBlobCleanupService>();

        return services;
    }

    private static IServiceCollection AddOutboxServices(this IServiceCollection services)
    {
        services.AddSingleton<DomainEventChannel>();

        services.AddScoped<IDomainEventPublisher, OutboxImmediateEventPublisher>();

        services.AddHostedService<OutboxDispatcher>();
        services.AddHostedService<ImmediateEventDispatcher>();

        return services;
    }
}
