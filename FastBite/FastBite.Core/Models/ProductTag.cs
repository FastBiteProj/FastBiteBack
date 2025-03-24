namespace FastBite.Core.Models;

public class ProductTag
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public List<Product> Products { get; set; }
}