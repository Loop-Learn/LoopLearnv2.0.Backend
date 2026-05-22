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
        public async Task<IActionResult> GetTags()
        {
            try
            {
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

                return Ok(new
                {
                    success = true,
                    data = tags
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
