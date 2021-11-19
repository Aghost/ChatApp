using System;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ChatApp.Domain.DbSet;
using ChatApp.DataService.Data;
using ChatApp.DataService.IConfiguration;
using ChatApp.Authentication.Configuration;
using ChatApp.Authentication.Configuration.Models.DTOs.Incoming;
using ChatApp.Authentication.Configuration.Models.DTOs.Outgoing;
using ChatApp.Authentication.Configuration.Models.DTOs.Generic;

namespace ChatApp.Api.Controllers.v1
{
    public class AccountsController : BaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly JwtConfiguration _jwtConfiguration;

        public AccountsController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager,
                TokenValidationParameters tokenValidationParameters,
                IOptionsMonitor<JwtConfiguration> optionMonitor)
            : base(unitOfWork)
        {
            _userManager = userManager;
            _jwtConfiguration =  optionMonitor.CurrentValue;
            _tokenValidationParameters = tokenValidationParameters;
        }

        [HttpPost, Route("Register")]
        public async Task<IActionResult> Register([FromBody]UserRegistrationRequest registration)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new UserRegistrationResponse {
                    Success = false,
                    Errors = new List<string>() { "Invalid payload" }
                });
            }

            var userExist = await _userManager.FindByEmailAsync(registration.Email);

            if (userExist != null)
            {
                return BadRequest(new UserRegistrationResponse {
                    Success = false,
                    Errors = new List<string>() { "Email already in use", "Invalid payload" }
                });
            }

            var newUser = new IdentityUser()
            {
                Email = registration.Email,
                UserName = registration.Email,
                EmailConfirmed = true // TODO: build emailfunctionality
            };

            var isCreated = await _userManager.CreateAsync(newUser, registration.Password);

            if (!isCreated.Succeeded)
            {
                return BadRequest(new UserRegistrationResponse {
                    Success = isCreated.Succeeded,
                    Errors = isCreated.Errors.Select(x => x.Description).ToList()
                });
            }
            var _user = new User();

            _user.IdentityId = new Guid(newUser.Id);
            _user.FirstName = registration.FirstName;
            _user.LastName = registration.LastName;
            _user.Email = registration.Email;

            await _unitOfWork.Users.Add(_user);
            await _unitOfWork.CompleteAsync();

            var token = await GenerateJwtToken(newUser);

            return Ok(new UserRegistrationResponse {
                Success = true,
                Token = token.JwtToken,
                RefreshToken = token.RefreshToken
            });
        }

        [HttpPost, Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest login)
        {

            if (!ModelState.IsValid)
                return BadRequest(new UserRegistrationResponse { Success = false, Errors = new List<string>() { "Invalid payload" }});

            var userExist = await _userManager.FindByEmailAsync(login.Email);

            if (userExist == null)
                return BadRequest(new UserLoginResponse { Success = false, Errors = new List<string>() { "Invalid authentication request" }});

            var isCorrect = await _userManager.CheckPasswordAsync(userExist, login.Password);

            if (!isCorrect)
                return BadRequest(new UserLoginResponse { Success = false, Errors = new List<string>() { "Invalid authentication request" }});

            var jwtToken = await GenerateJwtToken(userExist);

            return Ok(new UserLoginResponse() { Success = true, Token = jwtToken.JwtToken, RefreshToken = jwtToken.RefreshToken });
        }

        private async Task<TokenData> GenerateJwtToken(IdentityUser user)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtConfiguration.Secret);

            var tokenDesciptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new []
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.Add(_jwtConfiguration.ExpiryTimeFrame),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) //Review algo
            };

            var token = jwtHandler.CreateToken(tokenDesciptor);
            var jwtToken = jwtHandler.WriteToken(token);

            var refreshToken = new RefreshToken
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Token = $"{GenerateRandomString(25)}_{Guid.NewGuid()}",
                UserId = user.Id,
                IsRevoked = false,
                IsUsed = false,
                JwtId = token.Id,
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            };

            await _unitOfWork.RefreshTokens.Add(refreshToken);
            await _unitOfWork.CompleteAsync();

            return new TokenData
            {
                JwtToken = jwtToken,
                RefreshToken = refreshToken.Token
            };
        }

        private string GenerateRandomString(int length)
        {
            var random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string (Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
