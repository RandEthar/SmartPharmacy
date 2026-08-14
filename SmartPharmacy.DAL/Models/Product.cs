using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartPharmacy.DAL.Models
{
    public class Product : AuditableEntity
    {
        /// <summary>
        /// True once the expiry date has passed. A pharmacy must never sell an expired medicine,
        /// so this is checked when adding to the cart and again at checkout.
        /// </summary>
        [NotMapped]
        public bool IsExpired => ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow);

        /// <summary>
        /// Whether the product may be sold at all, ignoring the quantity being asked for.
        /// </summary>
        [NotMapped]
        public bool IsSellable => entitystate == Entitystate.Active && !IsExpired;

     
        public int Id { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public int MinimumStock { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public bool NeedsPrescription { get; set; }
        public int StockQuantity { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

        public ICollection<ProductTranslation> ProductTranslations { get; set; } = new List<ProductTranslation>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<NotificationProduct> NotificationProducts { get; set; } = new List<NotificationProduct>();
    }
}
