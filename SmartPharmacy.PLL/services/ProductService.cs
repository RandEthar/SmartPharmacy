using Mapster;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Extentions;
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
    public class ProductService : IProductService
    {
        private readonly IFileService _fileService;
        private readonly IProductRepository _productRepository;
        public ProductService(IFileService fileService, IProductRepository productRepository)
        {
            _fileService = fileService;
           _productRepository = productRepository;
        }
        public async Task<ProductResponse> CreateProduct(ProductRequest request)
        {
            var product = request.Adapt<Product>();
            product.ProductImages.Clear();

            if (request.Image != null)
            {
                product.Image = await _fileService.UploadFileAsync(request.Image);
            }
            if (request.ProductImages != null && request.ProductImages.Any())
            {
                foreach (var image in request.ProductImages)
                {
                    var productImage = new ProductImage
                    {
                        ImageUrl = await _fileService.UploadFileAsync(image),
                        ProductId = product.Id
                    };
                    product.ProductImages.Add(productImage);
                }
            }
            await _productRepository.CreateAsync(product);

            return product.Adapt<ProductResponse>();
        }

        public async Task<bool> DeleteProduct(int Id)
        {
            var product = await _productRepository.GetOne(c => c.Id == Id, new[]
            {
                nameof(Product.ProductImages),
                nameof(Product.ProductTranslations)
            });
            if (product == null) return false;
          
            if (!string.IsNullOrEmpty(product.Image))
            {
                _fileService.DeleteFile(product.Image);
            }
            if (product.ProductImages != null && product.ProductImages.Any())
            {
                foreach (var image in product.ProductImages)
                {
                    _fileService.DeleteFile(image.ImageUrl);
                }
            }
            await _productRepository.DeleteAsync(product);
            return true;
        }

        public async Task<PagenationResponse<ProductResponse>> GetAllProducts(ProductFilterRequest request)
        {
            var quere =  _productRepository.GetQueryableAsync(null, new[]
            {
                nameof(Product.ProductImages),nameof(Product.ProductTranslations)
            });
            if (request.Search!=null)
            {
                quere = quere.Where(p => p.ProductTranslations.Any(t => t.Name.Contains(request.Search)));
            }
            if (request.CategoryId.HasValue)
            {
                quere = quere.Where(p => p.CategoryId == request.CategoryId.Value);
            }
            if (request.MinPrice.HasValue)
            {
                quere = quere.Where(p => p.Price >= request.MinPrice.Value);
            }
            if (request.MaxPrice.HasValue)
            {
                quere = quere.Where(p => p.Price <= request.MaxPrice.Value);
            }   
            var product=await quere
                .OrderByDescending(p => p.Id)
                .ApplyPagenation(request.Page, request.Limit);

           return new PagenationResponse<ProductResponse>
            {
                Data = product.Data.Adapt<List<ProductResponse>>(),
                TotalCount = product.TotalCount,
                Page = product.Page,
                Limit = product.Limit
            };
        }

      

        public async Task<ProductResponse> GetProduct(Expression<Func<Product, bool>> filter)
        {
            var product = await _productRepository.GetOne(filter, new[]
            {
                nameof(Product.ProductImages), nameof(Product.ProductTranslations)
            });
            return product.Adapt<ProductResponse>();
        }

      

        public async Task<ProductResponse> UpdateProduct(int Id, UpdateProductRequest request)
        {
            var product = await _productRepository.GetOne(c => c.Id == Id, new[]
            {
                nameof(Product.ProductImages), nameof(Product.ProductTranslations)
            });
            if (product == null) return null;

            if (request.Price.HasValue) product.Price = request.Price.Value;
            if (request.MinimumStock.HasValue) product.MinimumStock = request.MinimumStock.Value;
            if (request.ExpiryDate.HasValue) product.ExpiryDate = request.ExpiryDate.Value;
            if (request.NeedsPrescription.HasValue) product.NeedsPrescription = request.NeedsPrescription.Value;
            if (request.StockQuantity.HasValue) product.StockQuantity = request.StockQuantity.Value;
            if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId.Value;

            if (request.ProductTranslations != null)
            {
                foreach (var item in request.ProductTranslations)
                {
                    var existingTranslation = product.ProductTranslations.FirstOrDefault(t => t.Language == item.Language);
                    if (existingTranslation != null)
                    {
                        if (!string.IsNullOrEmpty(item.Name)) existingTranslation.Name = item.Name;
                        if (!string.IsNullOrEmpty(item.Description)) existingTranslation.Description = item.Description;

                    }

                }
            }

            if (request.Image != null)
            {
                var oldImage = product.Image;
                product.Image = await _fileService.UploadFileAsync(request.Image);
                if (!string.IsNullOrEmpty(oldImage))
                {
                    _fileService.DeleteFile(oldImage);
                }
            }
            if (request.ProductImages != null && request.ProductImages.Any())
            {
                foreach (var image in product.ProductImages)
                {
                    var oldImage =image .ImageUrl;
                    _fileService.DeleteFile(oldImage);
                }
                product.ProductImages.Clear();
                foreach (var image in request.ProductImages)
                {
                    var productImage = new ProductImage
                    {
                        ImageUrl = await _fileService.UploadFileAsync(image),
                        ProductId = product.Id
                    };
                    product.ProductImages.Add(productImage);
                }
            }

            await _productRepository.UpdateAsync(product);
            return product.Adapt<ProductResponse>();
        }
    }
}
