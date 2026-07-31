using System.Collections.Generic;

namespace SmartPharmacy.DAL.Models
{
    public class Category : AuditableEntity
    {
        public int Id { get; set; }

        public ICollection<CategoryTranslation> CategoryTranslations { get; set; } = new List<CategoryTranslation>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
