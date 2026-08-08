using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.DTO.Request
{
    public class CategoryRequest
    {
    public List<CategoryTranslationRequest> CategoryTranslations { get; set; }
        public IFormFile Image { get; set; }

    }
}
