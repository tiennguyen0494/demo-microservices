using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MassTransit;
using Models;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DemoMicroservices.Controllers
{
    [Route("order")]
    [ApiController]
    public class OrderController : Controller
    {
        private IPublishEndpoint _bus;

        public OrderController(IPublishEndpoint bus)
        {
            _bus = bus;
        }

        [HttpPost]
        [Route("create-order")]
        public async Task<IActionResult> CreateOrder(CreateOrderModel model, CancellationToken cancellationToken)
        {
            await _bus.Publish(model, cancellationToken);
            return Accepted();
        }
    }
}

