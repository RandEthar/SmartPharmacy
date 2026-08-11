using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Models;
using SmartPharmacy.PLL.services;
using System.Text;

namespace SmartPharmacy.PLL.Jobs
{
    public class InventoryAlertJob : IInventoryAlertJob
    {
        private readonly IInventoryAlertService _inventoryAlertService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<InventoryAlertJob> _logger;

        public InventoryAlertJob(
            IInventoryAlertService inventoryAlertService,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<InventoryAlertJob> logger)
        {
            _inventoryAlertService = inventoryAlertService;
            _notificationService = notificationService;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        //نقص المخزون / قرب الانتهاء / منتهي
        public async Task Run()
        {
            var lowStock = await _inventoryAlertService.GetLowStockProducts();
            await _notificationService.NotifyPharmacists(
                NotificationTypeEnum.LowStock,
                lowStock.Select(p => p.ProductId).ToList());

            var nearExpiry = await _inventoryAlertService.GetNearExpiryProducts();
            await _notificationService.NotifyPharmacists(
                NotificationTypeEnum.NearExpiry,
                nearExpiry.Select(p => p.ProductId).ToList());

            var expired = await _inventoryAlertService.GetExpiredProducts();
            await _notificationService.NotifyPharmacists(
                NotificationTypeEnum.Expired,
                expired.Select(p => p.ProductId).ToList());

            // The in-app notification is the record; the email is what reaches a pharmacist who
            // has not opened the system today. One digest per run - never one mail per product.
            await SendDigestEmail(lowStock, nearExpiry, expired);
        }

        private async Task SendDigestEmail(
            List<InventoryAlertResponse> lowStock,
            List<InventoryAlertResponse> nearExpiry,
            List<InventoryAlertResponse> expired)
        {
            if (!lowStock.Any() && !nearExpiry.Any() && !expired.Any())
                return;

            var body = BuildDigest(lowStock, nearExpiry, expired);
            var subject = $"Inventory alerts - {DateTime.UtcNow:yyyy-MM-dd} " +
                          $"({expired.Count} expired, {nearExpiry.Count} near expiry, {lowStock.Count} low stock)";

            var pharmacists = await _userManager.GetUsersInRoleAsync(Roles.Pharmacist);

            foreach (var pharmacist in pharmacists)
            {
                if (string.IsNullOrWhiteSpace(pharmacist.Email))
                    continue;

                // One failing address must not stop the rest of the pharmacy from being told.
                try
                {
                    await _emailSender.SendEmailAsync(pharmacist.Email, subject, body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not send the inventory digest to {Email}.", pharmacist.Email);
                }
            }
        }

        private static string BuildDigest(
            List<InventoryAlertResponse> lowStock,
            List<InventoryAlertResponse> nearExpiry,
            List<InventoryAlertResponse> expired)
        {
            var body = new StringBuilder();
            body.Append("<h2>SmartPharmacy - Daily inventory report</h2>");
            body.Append($"<p>Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC.</p>");

            AppendSection(body, "❌ Expired - remove from the shelves", expired,
                p => $"expired on {p.ExpiryDate:yyyy-MM-dd} ({Math.Abs(p.DaysUntilExpiry)} day(s) ago)");

            AppendSection(body, "⚠️ Near expiry", nearExpiry,
                p => $"expires on {p.ExpiryDate:yyyy-MM-dd} (in {p.DaysUntilExpiry} day(s))");

            AppendSection(body, "📦 Low stock - needs restocking", lowStock,
                p => $"{p.StockQuantity} left, minimum is {p.MinimumStock}");

            body.Append("<hr><p style='color:#888;font-size:12px'>" +
                        "Automated message from SmartPharmacy. The same alerts are waiting in your notifications." +
                        "</p>");

            return body.ToString();
        }

        private static void AppendSection(
            StringBuilder body,
            string title,
            List<InventoryAlertResponse> products,
            Func<InventoryAlertResponse, string> describe)
        {
            if (!products.Any())
                return;

            body.Append($"<h3>{title} ({products.Count})</h3><ul>");
            foreach (var product in products)
            {
                body.Append($"<li><strong>{product.Name}</strong> - {describe(product)}</li>");
            }
            body.Append("</ul>");
        }
    }
}
