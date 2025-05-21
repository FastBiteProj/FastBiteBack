namespace FastBite.Core.Models;

public class ProductTag
{
    public Guid Id { get; set; }    
    public List<Product> Products { get; set; } = new();
    public List<ProductTagTranslation> Translations { get; set; } = new();
}