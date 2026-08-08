using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.DTO.Response
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool NeedsPrescription { get; set; }
        public int CategoryId { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public List<string> SubImages { get; set; }
    }
}
