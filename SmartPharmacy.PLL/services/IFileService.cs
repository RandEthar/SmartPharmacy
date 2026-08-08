using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.PLL.services
{
   public interface IFileService
    {
      Task<string?> UploadFileAsync( IFormFile file);
        void DeleteFile(string fileName);
    }
}
