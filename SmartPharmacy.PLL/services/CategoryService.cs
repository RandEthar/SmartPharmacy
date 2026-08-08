using Mapster;
using Microsoft.AspNetCore.Http;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Models;
using SmartPharmacy.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.PLL.services
{
    public class CategoryService: ICategoryService
    {

        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileService _fileService;
        public CategoryService(ICategoryRepository categoryRepository, IFileService fileService)
        {
            _categoryRepository = categoryRepository;
            _fileService = fileService;
        }

        public async Task<CategoryResponse> CreateCategory(CategoryRequest request)
        {
           var category= request.Adapt<Category>();

            if (request.Image!=null)
            {
                category.Image = await _fileService.UploadFileAsync(request.Image);
            }
            await _categoryRepository.CreateAsync(category);

            return category.Adapt<CategoryResponse>();
        }

        public async Task<bool> DeleteCategory(int Id)
        {
            var category =await _categoryRepository.GetOne(c => c.Id == Id);
            if (category == null) return false;
        
            if (!string.IsNullOrEmpty(category.Image))
            {
                _fileService.DeleteFile(category.Image);
            }
            await _categoryRepository.DeleteAsync(category);
            return true;
        }

        public async Task<List<CategoryResponse>> GetAllCategories()
        {
            var category = await _categoryRepository.GetAllAsync(null, new[]
            {
                nameof(Category.CategoryTranslations)
            });
            return category.Adapt<List<CategoryResponse>>();
        }

        public async Task<CategoryResponse> GetCategory(Expression<Func<Category, bool>> filter)
        {
            var category = await _categoryRepository.GetOne(filter, new[]
            {
                nameof(Category.CategoryTranslations)
            });
            if (category == null) return null;
            return category.Adapt<CategoryResponse>();
        }

        public async Task<CategoryResponse> UpdateCategory(int Id,  CategoryUpdateRequest request)
        {
            var category = await _categoryRepository.GetOne(c => c.Id == Id , new[]
             {
                nameof(Category.CategoryTranslations)
            });
            if (category == null) return null;
            if (request.CategoryTranslations!=null)
            {
                foreach (var item in request.CategoryTranslations)
                {
                    var existingTranslation = category.CategoryTranslations.FirstOrDefault(t => t.Language == item.Language);
                    if (existingTranslation != null)
                    {
                       if(!string.IsNullOrEmpty(item.Name))     existingTranslation.Name = item.Name;

                    }

                }
            }

            if (request.Image != null)
            {
                var oldImage = category.Image;
                category.Image = await _fileService.UploadFileAsync(request.Image);
                if (!string.IsNullOrEmpty(oldImage))
                {
                    _fileService.DeleteFile(oldImage);
                }
            }

            await _categoryRepository.UpdateAsync(category);
            return category.Adapt<CategoryResponse>();
        }
    }
}
