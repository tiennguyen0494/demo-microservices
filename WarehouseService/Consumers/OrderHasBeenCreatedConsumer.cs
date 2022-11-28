using Models;
using System;
using Constants;
using MassTransit;
using Newtonsoft.Json;
using UserService.Entities;
using WarehouseService.Entities;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;

namespace WarehouseService.Consumers
{
    public class OrderHasBeenCreatedConsumer : IConsumer<OrderHasBeenCreatedModel>
    {
        private IPublishEndpoint _bus;
        private WarehouseDbContext _dbContext;
        private IDistributedCache _distributedCache;

        public OrderHasBeenCreatedConsumer(
            IPublishEndpoint bus,
            WarehouseDbContext dbContext,
            IDistributedCache distributedCache
        )
        {
            _bus = bus;
            _dbContext = dbContext;
            _distributedCache = distributedCache;
        }

        public async Task Consume(ConsumeContext<OrderHasBeenCreatedModel> context)
        {
            var message = context.Message;
            string redisKey = $"{RedisEntityKeys.WAREHOUSE_PREFIX}-{message.OrderId}";
            string? productListJsonString = await _distributedCache.GetStringAsync(redisKey);
            List<Warehouse>? productList = JsonConvert.DeserializeObject<List<Warehouse>>(productListJsonString);

            if (!message.IsCompleted)
            {
                var dataListToUpdate = productList.Select(w => new Warehouse { Id = w.Id });
                _dbContext.Warehouses.AttachRange(dataListToUpdate);

                Parallel.ForEach(dataListToUpdate, item =>
                {
                    var product = message.ProductList.FirstOrDefault(x => x.ProductId == item.ProductId);
                    item.Quantity += product.Quantity;
                });

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}

