using Enums;
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
    public class ProductsAreReadyForSaleConsumer : IConsumer<ProductsAreReadyForSaleModel>
    {
        private IBus _bus;
        private OrderDbContext _dbContext;
        private IDistributedCache _distributedCache;

        public ProductsAreReadyForSaleConsumer(
            IBus bus,
            OrderDbContext dbContext,
            IDistributedCache distributedCache
        )
        {
            _bus = bus;
            _dbContext = dbContext;
            _distributedCache = distributedCache;
        }

        public async Task Consume(ConsumeContext<ProductsAreReadyForSaleModel> context)
        {
            var message = context.Message;

            string redisKey = $"{RedisEntityKeys.Order_PREFIX}-{message.OrderId}";
            string? orderJsonString = await _distributedCache.GetStringAsync(redisKey);
            Order? order = null;
            OrderHasBeenCreatedModel orderEvent = new ();
            orderEvent.OrderId = message.OrderId;

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderJsonString))
                    {
                        order = await _dbContext
                            .Orders
                            .Include(o=> o.OrderItems)
                            .FirstOrDefaultAsync(o => o.Id == message.OrderId);
                    }
                    else
                    {
                        order = JsonConvert.DeserializeObject<Order>(orderJsonString);
                    }

                    orderEvent.ProductList = order
                        .OrderItems
                        .Select(oi => new OrderItemModel
                        {
                            ProductId = oi.ProductId,
                            Quantity = oi.Quantity
                        })
                        .ToList();

                    var dataToUpdate = new Order();
                    dataToUpdate.Id = message.OrderId;

                    _dbContext.Orders.Attach(dataToUpdate);
                    dataToUpdate.Status = OrderStatus.New;

                    await _dbContext.SaveChangesAsync();
                    await _dbContext.Database.CommitTransactionAsync();

                    order.Status = OrderStatus.New;
                    orderJsonString = JsonConvert.SerializeObject(order);
                    await _distributedCache.SetStringAsync(redisKey, orderJsonString);
                    orderEvent.IsCompleted = true;
                }
                catch (Exception ex)
                {
                    orderEvent.Message = ex.Message;
                    await _dbContext.Database.RollbackTransactionAsync();
                }
            }

            await _bus.Publish(orderEvent);
        }
    }
}

