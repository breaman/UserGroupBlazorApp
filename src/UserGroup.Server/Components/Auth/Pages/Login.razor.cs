using System.Security.Claims;
using Blazored.FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using UserGroup.Data.Models;
using UserGroup.Shared.DTOs;

namespace UserGroup.Server.Components.Auth.Pages;

public partial class Login : ComponentBase
{
    private string? errorMessage;
    private FluentValidationValidator _fluentValidationValidator;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromForm]
    private LoginDto Dto { get; set; } = new();

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }
    
    [Inject] private ILogger<Login> Logger { get; set; }
    [Inject] private UserManager<User> UserManager { get; set; }
    [Inject] private SignInManager<User> SignInManager { get; set; }
    [Inject] private ApplicationDbContext DbContext { get; set; }
    [Inject] private IdentityRedirectManager RedirectManager { get; set; }

    private async Task LoginUser()
    {
        if (await _fluentValidationValidator.ValidateAsync())
        {
            Logger.LogInformation("Attempting to sign in {User}", Dto.Email);
            var user = await UserManager.FindByEmailAsync(Dto.Email);

            if (user != null)
            {
                if (!(await UserManager.IsEmailConfirmedAsync(user)))
                {
                    errorMessage = "Your account has not been activated yet. Please activate your account first.";
                }
                else
                {
                    // this logic was borrowed from https://github.com/dotnet/aspnetcore/issues/46558
                    var isValid = await UserManager.CheckPasswordAsync(user, Dto.Password);
                    if (isValid)
                    {
                        var customClaims = new[]
                        {
                            new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
                            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty)
                        };

                        await SignInManager.SignInWithClaimsAsync(user, false, customClaims);
                        Logger.LogInformation("{User} logged in.", Dto.Email);
                        RedirectManager.RedirectTo(ReturnUrl);
                        // if (managerId.HasValue)
                        // {
                        //     var customClaims = new[]
                        //     {
                        //         new Claim(SharedConstants.ManagerIdClaimKey, managerId.ToString() ?? string.Empty),
                        //         new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
                        //         new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty)
                        //     };
                        //     await SignInManager.SignInWithClaimsAsync(user, false, customClaims);
                        //
                        //     Logger.LogInformation("{User} logged in.", Input.Email);
                        //     RedirectManager.RedirectTo(ReturnUrl);
                        // }
                        // else
                        // {
                        //     errorMessage = "Unable to find a Manager with this email address.";
                        //     Logger.LogError("Unable to find a manager object associated with {User}", Input.Email);
                        // }
                    }
                    else
                    {
                        errorMessage = "Invalid Username or Password.";
                    }
                }
            }
            else
            {
                errorMessage = "Invalid Username or Password";
            }
        }
    }
}