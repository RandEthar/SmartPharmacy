namespace SmartPharmacy.DAL.Models
{
    public enum NotificationTypeEnum
    {
        // Pharmacy-facing: raised by the daily inventory job.
        LowStock = 1,
        NearExpiry = 2,
        Expired = 3,

        // Patient-facing: raised as an order moves through its lifecycle.
        OrderPaid = 4,
        OrderShipped = 5,
        OrderDelivered = 6,
        OrderCancelled = 7,
        PrescriptionApproved = 8,
        PrescriptionRejected = 9,
    }
}
