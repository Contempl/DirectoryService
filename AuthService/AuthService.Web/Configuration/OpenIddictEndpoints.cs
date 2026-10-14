using System.Collections.Immutable;
using System.Security.Claims;
using AuthService.Domain.Entities;
using Core.Auth;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthService.Configuration;

public static class OpenIddictEndpoints
{
    public static IEndpointRouteBuilder MapOpenIddictEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMethods(
            "/connect/authorize",
            [HttpMethods.Get, HttpMethods.Post],
            AuthorizeAsync);

        endpoints.MapPost("/connect/token", ExchangeAsync)
            .DisableAntiforgery();

        endpoints.MapGet("/connect/userinfo", UserInfo)
            .RequireAuthorization(policy => policy
                .AddAuthenticationSchemes(
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser());

        return endpoints;
    }

    private static async Task<IResult> AuthorizeAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        IOptions<AuthorizationServerOptions> options)
    {
        var request = context.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException(
                "The OpenID Connect request cannot be retrieved.");

        var authentication = await context.AuthenticateAsync(
            IdentityConstants.ApplicationScheme);

        if (!authentication.Succeeded)
        {
            if (request.HasPromptValue(PromptValues.None))
                return ProtocolForbid(
                    Errors.LoginRequired,
                    "The user is not logged in.");

            var returnUrl = context.Request.PathBase +
                            context.Request.Path +
                            context.Request.QueryString;

            return Results.Redirect(
                QueryHelpers.AddQueryString(
                    options.Value.LoginPageUrl,
                    "returnUrl",
                    returnUrl));
        }

        var user = await userManager.GetUserAsync(authentication.Principal!);
        if (user is null || !user.IsActive || !await userManager.IsEmailConfirmedAsync(user))
        {
            await context.SignOutAsync(IdentityConstants.ApplicationScheme);
            return ProtocolForbid(
                Errors.LoginRequired,
                "The provider session is no longer valid.");
        }

        var principal = await CreateUserPrincipalAsync(
            userManager,
            user,
            request.GetScopes(),
            options.Value.ApiScope);

        return Results.SignIn(
            principal,
            authenticationScheme:
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IResult> ExchangeAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        IOptions<AuthorizationServerOptions> options,
        IOptions<OpenIddictClientsOptions> clientOptions)
    {
        var request = context.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException(
                "The OpenID Connect request cannot be retrieved.");

        if (request.IsClientCredentialsGrantType())
        {
            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType,
                Claims.Name,
                Claims.Role);

            var configuredClient = clientOptions.Value.DirectoryService;
            var subject = string.Equals(
                    configuredClient.ClientId,
                    request.ClientId,
                    StringComparison.Ordinal)
                ? configuredClient.SubjectId?.ToString()
                : null;

            identity.SetClaim(
                Claims.Subject,
                subject ?? request.ClientId!);
            identity.SetClaim(Claims.Name, request.ClientId!);
            identity.SetClaim(Claims.Role, SystemRoles.ServiceAccount);

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());
            principal.SetResources(options.Value.ApiScope);
            principal.SetDestinations(GetDestinations);

            return Results.SignIn(
                principal,
                authenticationScheme:
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsAuthorizationCodeGrantType() ||
            request.IsRefreshTokenGrantType())
        {
            var authentication = await context.AuthenticateAsync(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var subject = authentication.Principal?.GetClaim(Claims.Subject);

            if (string.IsNullOrWhiteSpace(subject))
                return ProtocolForbid(
                    Errors.InvalidGrant,
                    "The token no longer identifies a user.");

            var user = await userManager.FindByIdAsync(subject);
            if (user is null || !user.IsActive ||
                !await userManager.IsEmailConfirmedAsync(user))
            {
                return ProtocolForbid(
                    Errors.InvalidGrant,
                    "The user is no longer allowed to sign in.");
            }

            var principal = await CreateUserPrincipalAsync(
                userManager,
                user,
                authentication.Principal!.GetScopes(),
                options.Value.ApiScope);

            return Results.SignIn(
                principal,
                authenticationScheme:
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException(
            "The specified grant type is not supported.");
    }

    private static IResult UserInfo(ClaimsPrincipal user)
    {
        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [Claims.Subject] = user.GetClaim(Claims.Subject)
                ?? throw new InvalidOperationException(
                    "The subject claim is missing.")
        };

        if (user.HasScope(Scopes.Profile))
            claims[Claims.Name] = user.GetClaim(Claims.Name) ?? string.Empty;

        if (user.HasScope(Scopes.Email))
            claims[Claims.Email] = user.GetClaim(Claims.Email) ?? string.Empty;

        if (user.HasScope(Scopes.Roles))
            claims[Claims.Role] = user.GetClaims(Claims.Role).ToArray();

        return Results.Ok(claims);
    }

    private static async Task<ClaimsPrincipal> CreateUserPrincipalAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        IEnumerable<string> scopes,
        string apiResource)
    {
        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            Claims.Name,
            Claims.Role);

        identity.SetClaim(Claims.Subject, user.Id.ToString());
        identity.SetClaim(Claims.Email, user.Email!);
        identity.SetClaim(
            Claims.Name,
            string.Join(' ', user.FirstName, user.LastName));
        identity.SetClaims(
            Claims.Role,
            (await userManager.GetRolesAsync(user)).ToImmutableArray());

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(scopes);
        principal.SetResources(apiResource);
        principal.SetDestinations(GetDestinations);

        return principal;
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        switch (claim.Type)
        {
            case Claims.Subject:
                yield return Destinations.AccessToken;
                yield return Destinations.IdentityToken;
                yield break;

            case Claims.Name:
                yield return Destinations.AccessToken;
                if (claim.Subject?.HasScope(Scopes.Profile) is true)
                    yield return Destinations.IdentityToken;
                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;
                if (claim.Subject?.HasScope(Scopes.Email) is true)
                    yield return Destinations.IdentityToken;
                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;
                if (claim.Subject?.HasScope(Scopes.Roles) is true)
                    yield return Destinations.IdentityToken;
                yield break;
        }
    }

    private static IResult ProtocolForbid(
        string error,
        string description)
    {
        return Results.Forbid(
            new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                    description
            }),
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }
}
