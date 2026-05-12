using LoopLearn.Application.DTOs.Auth;
using LoopLearn.Entities.Helpers.CustomValidations;
using LoopLearn.Entities.Helpers.Exceptions;
using LoopLearn.Entities.Helpers.Models;
using LoopLearn.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoopLearn.Application.Services.Implementations
{
    public class AuthService
    {
        private readonly Jwt _jwt;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthService(IOptions<Jwt> jwt, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _jwt = jwt?.Value ?? throw new ArgumentNullException(nameof(jwt));
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<AuthModel> LoginAsync(LoginRequestDTO request)
        {

            if (request is null) throw new ArgumentNullException(nameof(request));

            var isEmail = new EmailAddressAttribute().IsValid(request.EmailOrUsername);
            var isUsername = new UsernameAttribute().IsValid(request.EmailOrUsername);

            var user = new ApplicationUser();
            if (isEmail) user = await _userManager.FindByEmailAsync(request.EmailOrUsername);
            else if (isUsername) user = await _userManager.FindByNameAsync(request.EmailOrUsername);
            else throw new ValidationException("Invalid Username or Email.");

            if (user is null)
            {
                throw new UnauthorizedException("Invalid credentials.");
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);

            if (!signInResult.Succeeded)
            {
                if (signInResult.IsLockedOut)
                {
                    return new AuthModel
                    {
                        IsAuthenticated = false,
                        Message = "User account is locked."
                    };
                }

                throw new UnauthorizedException("Invalid credentials.");
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Count == 0)
            {
                return new AuthModel
                {
                    IsAuthenticated = false,
                    Message = "User has no assigned roles."
                };
            }

            var token = await GenerateTokenAsync(user, userRoles.First());
            user.LastLoginAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
            return new AuthModel
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresOn = token.ValidTo,
                IsAuthenticated = true,
                Message = "Login successful"
            };

        }

        public async Task<AuthModel> RegisterAsync(RegisterRequestDTO request)
        {
            if (request is null)
                throw new ArgumentNullException("The request can not be empty!");

            var emailExists = await _userManager.FindByEmailAsync(request.Email);

            if (emailExists is not null)
            {
                return new AuthModel
                {
                    IsAuthenticated = false,
                    Message = $"Email '{request.Email}' is already registered."
                };
            }
            var newUser = new ApplicationUser
            {
                FirstName = request.FName,
                LastName = request.LName,
                UserName = request.Username,
                Email = request.Email,
                PhoneNumber = request.Phone,
                BirthDate = request.BirthDate,
                Gender = request.Gender,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(newUser, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthModel
                {
                    IsAuthenticated = false,
                    Message = errors
                };
            }

            await _userManager.AddToRoleAsync(newUser, "Student");

            var token = await GenerateTokenAsync(newUser, "Student");

            await _userManager.UpdateAsync(newUser);


            return new AuthModel
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresOn = token.ValidTo,
                IsAuthenticated = true,
                Message = "Registration successful"
            };

        }
        private async Task<JwtSecurityToken> GenerateTokenAsync(ApplicationUser user, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role),
            };

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.Duration),
                signingCredentials: credentials);

            return token;
        }
    }
}
