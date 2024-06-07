using Microsoft.AspNetCore.Identity;
using UserGroup.Data.Interfaces;

namespace UserGroup.Data.Models;

public class Role : IdentityRole<int>, IEntityBase
{
    
}