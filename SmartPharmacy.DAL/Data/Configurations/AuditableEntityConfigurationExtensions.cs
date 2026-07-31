using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPharmacy.DAL.Models;

namespace SmartPharmacy.DAL.Data.Configurations
{
    public static class AuditableEntityConfigurationExtensions
    {
        public static void ConfigureAuditableEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : AuditableEntity
        {
            builder.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
