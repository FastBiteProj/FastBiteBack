namespace FastBite.Data.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string phoneNumber { get; set; }
    public bool IsEmailConfirmed { get; set; } = false;
    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetCodeExpiryTime { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }

    public ICollection<UserRole> UserRoles { get; set; }
    public ICollection<Order> Orders { get; set; }
    public ICollection<Reservation> Reservations { get; set; }
}
