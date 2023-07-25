using System.Security.Claims;
using System.Text.Encodings.Web;
using eQuantic.GoogleIdentityPlatform.Auth.Options;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace eQuantic.GoogleIdentityPlatform.Auth.Handlers;

public class FirebaseAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string AuthorizationKey = "Authorization";
    private const string BearerPrefix = "Bearer ";
    private readonly GoogleIdentityPlatformAuthOptions _authOptions;

    public FirebaseAuthenticationHandler(
        GoogleIdentityPlatformAuthOptions authOptions,
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
        _authOptions = authOptions;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Context.Request.Headers.ContainsKey(AuthorizationKey))
        {
            return AuthenticateResult.NoResult();
        }
        
        var bearerToken = Context.Request.Headers[AuthorizationKey].ToString();
        if (string.IsNullOrEmpty(bearerToken) || !bearerToken.StartsWith(BearerPrefix))
        {
            return AuthenticateResult.Fail("Invalid scheme");
        }

        var token = bearerToken[BearerPrefix.Length..];
        var firebaseApp = FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
        {
            ProjectId = _authOptions.ProjectId,
            Credential = GoogleCredential.FromAccessToken(token)
        });
        var result = await FirebaseAuth.GetAuth(firebaseApp).VerifyIdTokenAsync(token);

        return AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(new List<ClaimsIdentity>
                {
                    new(ToClaims(result.Claims), nameof(FirebaseAuthenticationHandler))
                }),
                JwtBearerDefaults.AuthenticationScheme));
    }

    private static IEnumerable<Claim> ToClaims(IReadOnlyDictionary<string, object> resultClaims)
    {
        foreach (var (key, value) in resultClaims)
        {
            var newKey = key switch
            {
                "user_id" => "id",
                "name" => "username",
                _ => key
            };
            yield return new Claim(newKey,
                resultClaims.GetValueOrDefault(key, string.Empty).ToString() ?? string.Empty);
        }
    }
}