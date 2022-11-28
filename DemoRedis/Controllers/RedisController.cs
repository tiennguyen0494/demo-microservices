using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoRedis.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DemoRedis.Controllers
{
    [ApiController]
    [Route("api/redis")]
    public class RedisController : Controller
    {
        private readonly IDistributedCache _distributedCache;

        public RedisController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public async ValueTask<IActionResult> Get(string key, CancellationToken cancellationToken)
        {
            var data = await _distributedCache.GetStringAsync(key, cancellationToken);
            return Json(data);
        }

        [HttpPost]
        public async ValueTask<IActionResult> Post(AddRedisModel model, CancellationToken cancellationToken)
        {
            await _distributedCache.SetStringAsync(model.Key, model.Value, cancellationToken);
            return NoContent();
        }
    }
}

