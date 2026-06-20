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
        public async Task<IActionResult> GetCategories([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Page and pageSize must be greater than zero."
                    });
				}

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

                var totalCategories = categories.Count();

                var pagedCategories = categories.Skip((page - 1) * pageSize).Take(pageSize).ToList();

				// Header metadata
                Response.Headers.Append("Total-Count", totalCategories.ToString());
                Response.Headers.Append("Page-Number", page.ToString());
                Response.Headers.Append("Page-Size", pageSize.ToString());
                Response.Headers.Append("Access-Control-Expose-Headers", "Total-Count, Page-Number, Page-Size");

				return Ok(new
                {
                    success = true,
                    data = pagedCategories
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
