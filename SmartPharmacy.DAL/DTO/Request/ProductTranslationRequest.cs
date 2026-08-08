using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.DTO.Request
{
   public class ProductTranslationRequest
    {
        public String Name { get; set; }
        public String Description { get; set; }
        public String Language { get; set; }
    }
}
