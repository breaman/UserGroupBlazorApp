using Microsoft.AspNetCore.Identity;
using UserGroup.Data.Interfaces;

namespace UserGroup.Data.Models;

public class User : IdentityUser<int>, IEntityBase
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime MemberSince { get; set; }
}