using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using SyncChat.API.Infrastructure.Persistence;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Configuration;
using SyncChat.API.Shared.Entities;
using SyncChat.API.Shared.Security.Contracts;
using System.Text;

namespace SyncChat.API.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IOptions<JWTSettings> jwtOptions) =>
            services.AddAuthenticationInternal(jwtOptions.Value)
                    .AddPersistence(configuration)
                    .AddOpenApiInternal();


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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        //services.AddHttpContextAccessor();
        //services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();

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
}
