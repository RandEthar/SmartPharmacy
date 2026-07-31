using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPharmacy.DAL.Models;

namespace SmartPharmacy.DAL.Data.Configurations
{
    public class ProductTranslationConfiguration : IEntityTypeConfiguration<ProductTranslation>
    {
        public void Configure(EntityTypeBuilder<ProductTranslation> builder)
        {
            builder.ToTable("ProductTranslations");

            builder.HasKey(pt => pt.Id);

            builder.Property(pt => pt.Name).IsRequired().HasMaxLength(200);
            builder.Property(pt => pt.Language).IsRequired().HasMaxLength(10);

            builder.HasOne(pt => pt.Product)
                .WithMany(p => p.ProductTranslations)
                .HasForeignKey(pt => pt.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(pt => new { pt.ProductId, pt.Language }).IsUnique();
        }
    }
}
