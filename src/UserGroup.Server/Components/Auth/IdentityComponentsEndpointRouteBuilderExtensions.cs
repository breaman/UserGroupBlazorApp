using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using UserGroup.Data.Models;

namespace UserGroup.Server.Components.Auth;

public static class IdentityComponentsEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        
        var accountGroup = endpoints.MapGroup("/Account");

        accountGroup.MapPost("/logout", async (ClaimsPrincipal User, SignInManager<User> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return TypedResults.LocalRedirect("/");
        });

        return accountGroup;
    }
}