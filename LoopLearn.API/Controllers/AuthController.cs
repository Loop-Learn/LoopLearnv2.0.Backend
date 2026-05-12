using LoopLearn.Application.DTOs.Auth;
using LoopLearn.Application.Services.Implementations;
using LoopLearn.Entities.Helpers.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LoopLearn.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        [Consumes("application/json")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.LoginAsync(request);
                if (!result.IsAuthenticated)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (ArgumentNullException e)
            {
                return BadRequest(new {Message = e.Message});
            }
            catch (ValidationException e)
            {
                return BadRequest(new { Message = e.Message });
            }
            catch (UnauthorizedException e)
            {
                return Unauthorized(new { Message = e.Message });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,new { Message = e.Message });
            }
        }
        [HttpPost("register")]
        [Consumes("application/json")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.RegisterAsync(request);
                if (!result.IsAuthenticated)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (ArgumentNullException e)
            {
                return BadRequest(new { Message = e.Message });
            }
            catch (ValidationException e)
            {
                return BadRequest(new { Message = e.Message });
            }
            catch (UnauthorizedException e)
            {
                return Unauthorized(new { Message = e.Message });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = e.Message });
            }
        }
    }
}
