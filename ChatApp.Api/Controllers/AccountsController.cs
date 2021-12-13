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
                return BadRequest(new UserRegistrationResponse { Success = false, Errors = new List<string>() { "Invalid payload" } });

            var userExist = await _userManager.FindByEmailAsync(registration.Email);
            if (userExist != null)
                return BadRequest(new UserRegistrationResponse { Success = false, Errors = new List<string>() { "Email already in use", "Invalid payload" } });

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

            // await userservice.Add(_user);
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

            return Ok(new UserLoginResponse() {
                Success = true,
                Token = jwtToken.JwtToken,
                RefreshToken = jwtToken.RefreshToken
            });
        }

        [HttpPost, Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new UserRegistrationResponse { Success = false, Errors = new List<string>() { "Invalid payload" }});

            var result = VerifyToken(request);

            if (result == null)
            {
                return BadRequest(new UserRegistrationResponse { Success = false, Errors = new List<string>() { "Token validation failed" }});
            }

            return Ok(result);
        }

        private async Task<AuthResult> VerifyToken(TokenRequest request)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(request.Token, _tokenValidationParameters, out var validateToken);

                // Check type
                if (validateToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (!result)
                        return null;
                }

                var expiryDateUtc = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expiryDate = UnixTimeStampToDateTime(expiryDateUtc);

                // Check expirydate
                if (expiryDate > UnixTimeStampToDateTime(expiryDateUtc))
                {
                    return new AuthResult() { Success = false, Errors = new List<string>() { "Jwt token has not expired" } };
                }

                // Check if token exists
                var refreshTokenExist = await _unitOfWork.RefreshTokens.GetByRefreshToken(request.RefreshToken);
                if (refreshTokenExist == null)
                {
                    return new AuthResult() { Success = false, Errors = new List<string>() { "Invalid Refresh token has not expired" } };
                }

                // Check refreshtoken expiry date
                if (refreshTokenExist.ExpiryDate > DateTime.UtcNow)
                {
                    return new AuthResult() { Success = false, Errors = new List<string>() { "Refresh token has expired, please login again" } };
                }

                // Check refreshtoken has been used
                if (refreshTokenExist.IsUsed)
                {
                    return new AuthResult() { Success = false, Errors = new List<string>() { "Refresh token has already been used" } };
                }

                // Check refreshtoken has been revoked
                if (refreshTokenExist.IsRevoked)
                {
                    return new AuthResult() { Success = false, Errors = new List<string>() { "Refresh token has been revoked" } };
                }

                var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (refreshTokenExist.JwtId != jti)
                {
                    return new AuthResult() { Success = false, Errors = new List<string>() { "Refresh token ref does not match jwt token" }};
                }

                // get new token
                refreshTokenExist.IsUsed = true;

                var updateResult = await _unitOfWork.RefreshTokens.MarkRefreshTokenAsUsed(refreshTokenExist);

                if (updateResult)
                {
                    await _unitOfWork.CompleteAsync();
                    var dbUser = await _userManager.FindByIdAsync(refreshTokenExist.UserId);

                    if (dbUser == null)
                    {
                        return new AuthResult() { Success = false, Errors = new List<string>() { "Error Processing Request" }};
                    }

                    var tokens = await GenerateJwtToken(dbUser);
                    return new AuthResult { Token = tokens.JwtToken, Success = true, RefreshToken = tokens.RefreshToken };
                }

                return new AuthResult() { Success = false, Errors = new List<string>() { "Error Processing request" }};
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                // TODO: Better error handling
                // TODO: Add logger
                return null;
            }

        }

        private DateTime UnixTimeStampToDateTime(long unixDate)
        {
            var dateTime = new DateTime(1970,1,1,0,0,0,0, DateTimeKind.Utc);
            return dateTime.AddSeconds(unixDate).ToUniversalTime();
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
            //const string chars = "(づ｡◕‿‿◕｡)づ😁😀😂😃😄😅😆😇😈😉😊😋😌😍😎😏😐😑😒";
            return new string (Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
