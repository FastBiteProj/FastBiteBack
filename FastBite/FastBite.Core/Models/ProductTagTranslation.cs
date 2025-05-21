using FastBite.Core.Models;

namespace FastBite.Core.Models
{
    public class ProductTagTranslation
    {
        public Guid Id { get; set; }
        public string LanguageCode { get; set; }
        public string Name { get; set; }
        
        public Guid ProductTagId { get; set; }
        public ProductTag ProductTag { get; set; }
    }
}