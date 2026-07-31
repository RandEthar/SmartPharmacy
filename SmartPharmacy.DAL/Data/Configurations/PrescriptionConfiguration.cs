using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPharmacy.DAL.Models;

namespace SmartPharmacy.DAL.Data.Configurations
{
    public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            builder.ToTable("Prescriptions");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.ImageUrl).IsRequired();
            builder.Property(p => p.Status).IsRequired().HasMaxLength(50);

            builder.HasOne(p => p.Order)
                .WithMany(o => o.Prescriptions)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ConfigureAuditableEntity();
        }
    }
}
