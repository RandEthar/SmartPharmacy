namespace SmartPharmacy.DAL.Models
{
    public class ProductTranslation
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public String Language { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
