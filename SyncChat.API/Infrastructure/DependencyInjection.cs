using Microsoft.AspNetCore.Authorization;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Security.Contracts;

namespace SyncChat.API.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
            services.AddAuthenticationInternal(configuration);


    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //    .AddJwtBearer(o =>
        //    {
        //        o.RequireHttpsMetadata = false;
        //        o.TokenValidationParameters = new TokenValidationParameters
        //        {
        //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
        //            ValidIssuer = configuration["Jwt:Issuer"],
        //            ValidAudience = configuration["Jwt:Audience"],
        //            ClockSkew = TimeSpan.Zero
        //        };
        //    });

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
}
