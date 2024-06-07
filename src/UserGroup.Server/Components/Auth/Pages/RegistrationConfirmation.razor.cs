using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using UserGroup.Data.Models;
using UserGroup.Server.Components.Emails;

namespace UserGroup.Server.Components.Auth.Pages;

public partial class RegistrationConfirmation : ComponentBase
{
    private string? emailConfirmationLink;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromQuery]
    private string? Email { get; set; }

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }
    
    [Inject] private IdentityRedirectManager RedirectManager { get; set; }
    [Inject] private UserManager<User> UserManager { get; set; }
    [Inject] private ILogger<RegistrationConfirmation> Logger { get; set; }
    [Inject] private IEnhancedEmailSender<User> EmailSender { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (Email is null)
        {
            RedirectManager.RedirectTo("");
        }

        var user = await UserManager.FindByEmailAsync(Email);
        if (user is null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            Logger.LogError("Unable to find {Email} to send registration confirmation to.", Email);
            // statusMessage = "Error finding user for unspecified email";
        }
        else if (EmailSender is IdentityNoOpEmailSender)
        {
            // Once you add a real email sender, you should remove this code that lets you confirm the account
            var userId = await UserManager.GetUserIdAsync(user);
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            emailConfirmationLink = NavigationManager.GetUriWithQueryParameters(
                NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                new Dictionary<string, object?> { ["email"] = Email, ["code"] = code, ["returnUrl"] = ReturnUrl });
        }
        else
        {
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var confirmationLink = NavigationManager.GetUriWithQueryParameters(
                NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                new Dictionary<string, object?> { ["email"] = Email, ["code"] = code, ["returnUrl"] = ReturnUrl });
            Logger.LogInformation("Sending registration confirmation to {Email}", Email);
            await EmailSender.SendConfirmationLinkAsync(user, Email, confirmationLink);
        }
    }
}