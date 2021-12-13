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
    [ApiController, Route("api/v{version:apiVersion}/[controller]"), ApiVersion("1.0")]
    public class BaseController : ControllerBase
    {
        public IUnitOfWork _unitOfWork;

        public BaseController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

    }
}
