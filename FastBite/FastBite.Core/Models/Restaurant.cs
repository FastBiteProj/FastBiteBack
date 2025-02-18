namespace FastBite.Core.Models;
public class Restaurant {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
}