using System.ComponentModel.DataAnnotations;

namespace FastBite.Core.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int Price { get; set; }

        public Guid CategoryId { get; set; }

        public Category Category { get; set; }

        public string ImageUrl { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
        
        public ICollection<ProductTranslation> Translations { get; set; }
        
        public ICollection<ProductTag> ProductTags { get; set; }
    }
}
