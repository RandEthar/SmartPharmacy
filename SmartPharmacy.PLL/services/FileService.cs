using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.PLL.services
{
    public class FileService : IFileService
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public void DeleteFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public async Task<string?> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"نوع الملف غير مسموح. الأنواع المسموحة: {string.Join(", ", AllowedExtensions)}");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                throw new ArgumentException("حجم الملف أكبر من الحد المسموح (5 ميجابايت).");
            }

            var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");
            Directory.CreateDirectory(imagesFolder);

            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(imagesFolder, fileName);
            using (var stream = File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }
    }
}
