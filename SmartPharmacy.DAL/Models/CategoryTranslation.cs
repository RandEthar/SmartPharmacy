namespace SmartPharmacy.DAL.Models
{
    public class CategoryTranslation
    {
        public int Id { get; set; }
        public String Language { get; set; }
        public String Name { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
