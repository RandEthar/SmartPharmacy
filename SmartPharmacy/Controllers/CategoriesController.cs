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
    public class CategoriesController : ControllerBase
    {

        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

          [HttpPost]
        [Authorize(Roles = Roles.AdminOrPharmacist)]
        public async Task<IActionResult> CreateCategory([FromForm] CategoryRequest categoryDto)
        {
          
                var categoryResponse = await _categoryService.CreateCategory(categoryDto);
                if (categoryResponse == null)
                {
                    return BadRequest("Failed to create category.");
                }
                return Ok(categoryResponse);
           
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            
                var categoryResponse = await _categoryService.GetCategory(c => c.Id == id);
                if (categoryResponse == null)
                {
                    return Problem(detail: $"Category with id {id} was not found.", statusCode: StatusCodes.Status404NotFound);
                }
                return Ok(categoryResponse);
          
        }
        [HttpGet()]
        public async Task<IActionResult> GetAllCategory()
        {

            var categoryResponse = await _categoryService.GetAllCategories();
            if (categoryResponse == null)
            {
                return NotFound();
            }
            return Ok(categoryResponse);

        }
        [HttpPatch("{id}")]
        [Authorize(Roles = Roles.AdminOrPharmacist)]
        public async Task<IActionResult> UpdateCategory(int id, [FromForm] CategoryUpdateRequest categoryDto)
        {

            var categoryResponse = await _categoryService.UpdateCategory(id, categoryDto);
            if (categoryResponse == null)
            {
                return Problem(detail: $"Category with id {id} was not found.", statusCode: StatusCodes.Status404NotFound);
            }
            return Ok(categoryResponse);

        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.AdminOrPharmacist)]
        public async Task<IActionResult> DeleteCategory(int id)
        {

             bool Deleted= await _categoryService.DeleteCategory(id);
            if (!Deleted)
            {
                return Problem(detail: $"Category with id {id} was not found.", statusCode: StatusCodes.Status404NotFound);
            }
            return Ok();

        }



    }
    }

