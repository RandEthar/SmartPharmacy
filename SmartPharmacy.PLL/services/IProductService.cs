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
    public interface IProductService
    {
        Task<ProductResponse> CreateProduct(ProductRequest request);
        Task<ProductResponse> UpdateProduct(int Id, UpdateProductRequest request);
        Task<bool> DeleteProduct(int Id);
        Task<PagenationResponse<ProductResponse>> GetAllProducts(ProductFilterRequest request);
        Task<ProductResponse> GetProduct(Expression<Func<Product, bool>> filter);
    }
}
