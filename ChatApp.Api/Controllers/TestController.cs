using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ChatApp.DataService.Data;
using ChatApp.DataService.IConfiguration;
using ChatApp.Domain.DbSet;
using ChatApp.Domain.DTOs.Incoming;

namespace ChatApp.Api.Controllers.v1
{
    public class TestController : BaseController
    {
        public TestController(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var users = await _unitOfWork.Users.All();

            return Ok(users);
            //return Ok(_unitOfWork.Users.All());
        }

    }
}
