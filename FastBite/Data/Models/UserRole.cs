using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FastBite.Data.Models;

public class UserRole
{
    public Guid Id { get; set; } = Guid.NewGuid();
  
    public Guid RoleId { get; set; }

    public Guid UserId { get; set; }

    public AppRole AppRole { get; set; }
    
    public User User { get; set; }
}