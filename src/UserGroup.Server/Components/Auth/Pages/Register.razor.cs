using System.Text;
using System.Text.Encodings.Web;
using Blazored.FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using UserGroup.Data.Models;
using UserGroup.Server.Components.Emails;
using UserGroup.Shared.DTOs;
using SharedConstants = UserGroup.Shared.Models.Constants;

namespace UserGroup.Server.Components.Auth.Pages;

public partial class Register : ComponentBase
{
    private List<IdentityError>? _identityErrors = new();
    private FluentValidationValidator _fluentValidationValidator;

    [SupplyParameterFromForm] private RegisterDto Dto { get; set; } = new();
    
    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }
    
    [Inject]
    protected UserManager<User>? UserManager { get; set; }
    
    [Inject]
    protected SignInManager<User>? SignInManager { get; set; }
    
    [Inject]
    protected ILogger<Register> Logger { get; set; }
    
    [Inject]
    protected NavigationManager NavigationManager { get; set; }
    
    [Inject]
    protected IEnhancedEmailSender<User> EmailSender { get; set; }
    
    [Inject]
    internal IdentityRedirectManager RedirectManager { get; set; }

    private string? Message => _identityErrors is null
        ? null
        : $"Error: {string.Join(", ", _identityErrors.Select(error => error.Description))}";

    private async Task RegisterUser()
    {
        _identityErrors = new();
        if (await _fluentValidationValidator.ValidateAsync())
        {
            var user = new User()
            {
                FirstName = Dto.FirstName,
                LastName = Dto.LastName,
                Email = Dto.Email,
                UserName = Dto.Email,
                MemberSince = DateTime.UtcNow
            };

            var result = await UserManager.CreateAsync(user, Dto.Password);

            if (!result.Succeeded)
            {
                _identityErrors = result.Errors.ToList();
                return;
            }

            Logger.LogInformation("User created a new account with password.");
            
            await UserManager.AddToRoleAsync(user, SharedConstants.User);

            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = NavigationManager.GetUriWithQueryParameters(
                NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                new Dictionary<string, object?> { ["userId"] = user.Id, ["code"] = code, ["returnUrl"] = ReturnUrl });

            await EmailSender.SendConfirmationLinkAsync(user, user.Email, HtmlEncoder.Default.Encode(callbackUrl));

            if (UserManager.Options.SignIn.RequireConfirmedEmail)
            {
                RedirectManager.RedirectTo("Account/RegistrationConfirmation",
                    new() { ["email"] = Dto.Email, ["returnUrl"] = ReturnUrl });
            }

            await SignInManager.SignInAsync(user, isPersistent: false);
            RedirectManager.RedirectTo(ReturnUrl);
        }
    }
}