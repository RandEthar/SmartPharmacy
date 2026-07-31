using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPharmacy.DAL.Models;

namespace SmartPharmacy.DAL.Data.Configurations
{
    public class CategoryTranslationConfiguration : IEntityTypeConfiguration<CategoryTranslation>
    {
        public void Configure(EntityTypeBuilder<CategoryTranslation> builder)
        {
            builder.ToTable("CategoryTranslations");

            builder.HasKey(ct => ct.Id);

            builder.Property(ct => ct.Language).IsRequired().HasMaxLength(10);
            builder.Property(ct => ct.Name).IsRequired().HasMaxLength(200);

            builder.HasOne(ct => ct.Category)
                .WithMany(c => c.CategoryTranslations)
                .HasForeignKey(ct => ct.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(ct => new { ct.CategoryId, ct.Language }).IsUnique();
        }
    }
}
