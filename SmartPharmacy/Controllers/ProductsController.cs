using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.Models;
using SmartPharmacy.PLL.services;

namespace SmartPharmacy.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        [Authorize(Roles = Roles.AdminOrPharmacist)]
        public async Task<IActionResult> CreateProduct([FromForm] ProductRequest productDto)
        {

            var productResponse = await _productService.CreateProduct(productDto);
            if (productResponse == null)
            {
                return BadRequest("Failed to create product.");
            }
            return Ok(productResponse   );

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct         (int id)
        {

            var productResponse = await _productService.GetProduct(p => p.Id == id);
            if (productResponse == null)
            {
                return Problem(detail: $"Product with id {id} was not found.", statusCode: StatusCodes.Status404NotFound);
            }
            return Ok(productResponse);

        }
        [HttpGet()]
        public async Task<IActionResult> GetAllProducts ([FromQuery] ProductFilterRequest request)
        {

            var productResponse = await _productService.GetAllProducts(request);
            if (productResponse == null)
            {
                return NotFound();
            }
            return Ok(productResponse);

        }
        [HttpPatch("{id}")]
        [Authorize(Roles = Roles.AdminOrPharmacist)]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductRequest productDto)
        {

            var productResponse = await _productService.UpdateProduct(id, productDto);
            if (productResponse == null)
            {
                return Problem(detail: $"Product with id {id} was not found.", statusCode: StatusCodes.Status404NotFound);
            }
            return Ok(productResponse);

        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.AdminOrPharmacist)]
        public async Task<IActionResult> DeleteProduct      (int id)
        {

            bool Deleted = await _productService.DeleteProduct (id);
            if (!Deleted)
            {
                return Problem(detail: $"Product with id {id} was not found.", statusCode: StatusCodes.Status404NotFound);
            }
            return Ok();

        }

    }
}
