using Models;
using System;
using Constants;
using MassTransit;
using Newtonsoft.Json;
using OrderService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace OrderService.Consumers
{
    public class ProductsAreNotReadyForSaleConsumer : IConsumer<ProductsAreNotReadyForSaleModel>
    {
        private IBus _bus;
        private OrderDbContext _dbContext;
        private IDistributedCache _distributedCache;

        public ProductsAreNotReadyForSaleConsumer(
            IBus bus,
            OrderDbContext dbContext,
            IDistributedCache distributedCache
        )
        {
            _bus = bus;
            _dbContext = dbContext;
            _distributedCache = distributedCache;
        }

        public async Task Consume(ConsumeContext<ProductsAreNotReadyForSaleModel> context)
        {
            var message = context.Message;
            Order? order = null;
            string redisKey = $"{RedisEntityKeys.Order_PREFIX}-{message.OrderId}";

            string? redisOrderString = await _distributedCache.GetStringAsync(redisKey);
            if (!string.IsNullOrWhiteSpace(redisOrderString))
            {
                order = JsonConvert.DeserializeObject<Order>(redisOrderString);
            }
            else
            {
                order = await _dbContext
                    .Orders
                    .Include(order => order.OrderItems)
                    .SingleOrDefaultAsync(o => o.Id == message.OrderId);
            }

            if (order != null)
            {
                _dbContext.Remove(order);
                await _dbContext.SaveChangesAsync();
            }

            await _distributedCache.RemoveAsync(redisKey);
        }
    }
}

