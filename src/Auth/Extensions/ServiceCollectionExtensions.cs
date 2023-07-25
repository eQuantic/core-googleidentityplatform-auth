using eQuantic.GoogleIdentityPlatform.Auth.Handlers;
using eQuantic.GoogleIdentityPlatform.Auth.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.GoogleIdentityPlatform.Auth.Extensions;

/// <summary>
/// Service Registry Extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Google Identity Platform Authentication and Authorization
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="options">The options</param>
    /// <returns>The registry</returns>
    public static IServiceCollection AddGoogleIdentityPlatformAuth(this IServiceCollection services,
        Action<GoogleIdentityPlatformAuthOptions>? options = null)
    {
        var authOptions = new GoogleIdentityPlatformAuthOptions();
        options?.Invoke(authOptions);
        services.AddSingleton(authOptions);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, FirebaseAuthenticationHandler>(JwtBearerDefaults
                .AuthenticationScheme, opt => {});
        return services;
    }
}