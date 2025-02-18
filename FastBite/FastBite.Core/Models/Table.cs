namespace FastBite.Core.Models;

public class Table
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int Number {get; set;}

    public int Capacity { get; set; }
    
    public ICollection<Reservation> Reservations { get; set; }
}