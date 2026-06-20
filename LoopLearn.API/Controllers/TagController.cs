using LoopLearn.Entities.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoopLearn.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public TagController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<IActionResult> GetTags([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

				var tags = await _unitOfWork.Tags.GetAsync(selector: c => new
                {
                    Id = c.Id,
                    Name = c.Name
                });

				if (tags == null)
                    return NotFound(new
                    {
                        sucess = false,
                        message = "No Tags Found."
                    });

                var totalTags = tags.Count();

                var pagedTags = tags.Skip((page - 1) * pageSize).Take(pageSize).ToList();

				// Header metadata
                Response.Headers.Append("Total-Count", totalTags.ToString());
                Response.Headers.Append("Page-Number", page.ToString());
                Response.Headers.Append("Page-Size", pageSize.ToString());
                Response.Headers.Append("Access-Control-Expose-Headers", "Total-Count, Page-Number, Page-Size");

				return Ok(new
                {
                    success = true,
                    data = pagedTags
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
