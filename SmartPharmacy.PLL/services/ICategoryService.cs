using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.PLL.services
{
    public interface ICategoryService
    {
     Task<CategoryResponse> CreateCategory(CategoryRequest request);   
        Task<CategoryResponse> UpdateCategory(int Id,CategoryUpdateRequest request);
        Task<bool> DeleteCategory(int Id);
        Task<List<CategoryResponse>> GetAllCategories();
        Task<CategoryResponse> GetCategory(Expression<Func<Category, bool>> filter);


    }
}
