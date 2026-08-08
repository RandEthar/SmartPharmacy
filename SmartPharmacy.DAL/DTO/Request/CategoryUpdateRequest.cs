using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace SmartPharmacy.DAL.DTO.Request
{
    public class CategoryUpdateRequest
    {
        public List<CategoryTranslationRequest>? CategoryTranslations { get; set; }
        public IFormFile? Image { get; set; }
    }
}
