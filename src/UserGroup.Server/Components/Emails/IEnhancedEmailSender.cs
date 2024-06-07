using Microsoft.AspNetCore.Identity;

namespace UserGroup.Server.Components.Emails;

public interface IEnhancedEmailSender<TUser> : IEmailSender<TUser> where TUser : class
{
    Task SendCongratulationsEmail(TUser user, string email);
}