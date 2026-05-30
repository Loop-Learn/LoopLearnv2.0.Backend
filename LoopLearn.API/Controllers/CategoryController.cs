using LoopLearn.Entities.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoopLearn.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAsync(selector: c => new
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                });

                if (categories == null)
                    return NotFound(new
                    {
                        success = false,
                        message = "No Categories Found."
                    });

                return Ok(new
                {
                    success = true,
                    data = categories
                });
            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError,
                new
                {
                    success = false,
                    message = "An unexpected error occurred while retrieving courses."
                });
            }
        }
    }
}
