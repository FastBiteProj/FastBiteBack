namespace FastBite.Data.Models;
public class Category {
    public Guid Id { get; set; } = Guid.NewGuid();

    public string  Name { get; set; }
    
    public ICollection<Product> Products { get; set; }
}