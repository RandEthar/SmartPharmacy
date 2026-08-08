using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace SmartPharmacy.DAL.Models
{
    public class Product : AuditableEntity
    {
     
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
