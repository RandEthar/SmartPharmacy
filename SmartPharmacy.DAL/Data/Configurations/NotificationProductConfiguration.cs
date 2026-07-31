using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartPharmacy.DAL.Models;

namespace SmartPharmacy.DAL.Data.Configurations
{
    public class NotificationProductConfiguration : IEntityTypeConfiguration<NotificationProduct>
    {
        public void Configure(EntityTypeBuilder<NotificationProduct> builder)
        {
            builder.ToTable("NotificationsProduct");

            builder.HasOne(np => np.Notification)
                .WithMany(n => n.NotificationProducts)
                .HasForeignKey(np => np.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(np => np.Product)
                .WithMany(p => p.NotificationProducts)
                .HasForeignKey(np => np.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
