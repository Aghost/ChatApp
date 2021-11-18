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

        public async Task<IActionResult> Get()
        {
            Guid id = new("a67b4b21-51c7-46f6-84ee-25cd47cfea86");
            var testuser = await _unitOfWork.Users.GetById(id);


            var newUser = new User();

            newUser.Id = id;
            newUser.UserName = "newUserName";
            newUser.FirstName = "newName";
            newUser.LastName = "newLastName";
            newUser.Email = "new@email.com";
            newUser.Status = 0;

            await _unitOfWork.Users.Upsert(newUser);
            await _unitOfWork.CompleteAsync();

            Console.WriteLine($"User {newUser.Id}:{newUser.UserName} edited");

            return Ok(newUser);
        }

    }
}
