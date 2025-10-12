using System.Collections.Generic;

namespace Trivare.Domain.Entities;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
