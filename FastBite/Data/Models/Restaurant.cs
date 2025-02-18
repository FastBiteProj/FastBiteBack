using FastBite.Data.Models;

namespace FastBite.Data.Models;
public class Restaurant {
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; }
    
    public ICollection<Table> Tables { get; set; }
}