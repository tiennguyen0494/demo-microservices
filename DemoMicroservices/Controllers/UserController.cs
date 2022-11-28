using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace DemoMicroservices.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IPublishEndpoint _bus;

        public UserController(IPublishEndpoint bus)
        {
            _bus = bus;
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(CreateUserModel model, CancellationToken cancellationToken)
        {
            await _bus.Publish(model, cancellationToken);
            return Accepted();
        }

    }
}

