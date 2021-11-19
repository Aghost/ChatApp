using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ChatApp.DataService.Data;
using ChatApp.DataService.IConfiguration;
using ChatApp.Domain.DbSet;
using ChatApp.Domain.DTOs.Incoming;

namespace ChatApp.Api.Controllers.v1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : BaseController
    {
        public UsersController(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await _unitOfWork.Users.All());
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(UserDto user)
        {
            // Add Automapper
            var _user = new User();

            _user.FirstName = user.FirstName;
            _user.LastName = user.LastName;
            _user.Email = user.Email;

            await _unitOfWork.Users.Add(_user);
            await _unitOfWork.CompleteAsync();

            return CreatedAtRoute("GetUser", new { id = _user.Id }, user);
        }

        [HttpGet, Route("GetUser", Name = "GetUser")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _unitOfWork.Users.GetById(id);
            return Ok(user);
        }
    }
}
