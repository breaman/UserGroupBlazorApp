using UserGroup.Data.Interfaces;

namespace UserGroup.Data.Models;

public abstract class EntityBase : IEntityBase
{
    public int Id { get; set; }
}