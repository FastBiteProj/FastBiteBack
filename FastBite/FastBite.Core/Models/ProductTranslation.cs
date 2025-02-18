using System.ComponentModel.DataAnnotations;

namespace FastBite.Core.Models
{
    public class ProductTranslation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProductId { get; set; }

        public Product Product { get; set; }

        public string LanguageCode { get; set; }

        public string Name { get; set; }
        
        public string Description { get; set; }
    }
}
