using System;
using Models;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using OrderService.Entities;
using Enums;
using Newtonsoft.Json;
using Constants;

namespace OrderService.Consumers
{
    public class CreateOrderConsumer : IConsumer<CreateOrderModel>
    {
        private IPublishEndpoint _bus;
        private OrderDbContext _dbContext;
        private IDistributedCache _distributedCache;

        public CreateOrderConsumer(
            IPublishEndpoint bus,
            OrderDbContext dbContext,
            IDistributedCache distributedCache
        )
        {
            _bus = bus;
            _dbContext = dbContext;
            _distributedCache = distributedCache;
        }

        public async Task Consume(ConsumeContext<CreateOrderModel> context)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                var drafData = context.Message;
                Order order = new Order();
                order.Id = drafData.OrderId;
                order.UserId = drafData.UserId;
                order.Status = OrderStatus.Draf;
                order.CreatedDate = DateTime.UtcNow;

                // Save data to the database.
                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                List<OrderItem> orderItems = new List<OrderItem>();
                Parallel.ForEach(drafData.ProductList, productId =>
                {
                    OrderItem anItem = new OrderItem();
                    anItem.Id = Guid.NewGuid();
                    anItem.OrderId = order.Id;
                    anItem.CreatedDate = DateTime.UtcNow;
                    anItem.Quantity = productId.Quantity;
                    anItem.ProductId = productId.ProductId;
                    orderItems.Add(anItem);
                });
                _dbContext.OrderItems.AddRange(orderItems);
                await _dbContext.SaveChangesAsync();
                
                // Save data to the Redis.
                try
                {
                    var jsonSetting = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };
                    string jsonData = JsonConvert.SerializeObject(order, jsonSetting);
                    string orderIdString = order?.Id.ToString();
                    string redisKey = $"{RedisEntityKeys.Order_PREFIX}-{orderIdString}";
                    await _distributedCache.SetStringAsync(redisKey, jsonData);
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                    await transaction.RollbackAsync();
                }
            }
        }
    }
}

