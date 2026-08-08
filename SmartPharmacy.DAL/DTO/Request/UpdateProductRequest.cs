using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.DTO.Request
{
    public class UpdateProductRequest
    {
        public IFormFile? Image { get; set; }
        public decimal? Price { get; set; }
        public int? MinimumStock { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public bool? NeedsPrescription { get; set; }
        public int? StockQuantity { get; set; }

        public int? CategoryId { get; set; }

        public List<IFormFile>? ProductImages { get; set; } = new List<IFormFile>();

        public ICollection<ProductTranslationRequest>? ProductTranslations { get; set; } = new List<ProductTranslationRequest>();
    }
}
