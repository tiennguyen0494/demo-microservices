using System;
using Models;
using Constants;
using MassTransit;
using Newtonsoft.Json;
using UserService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace WarehouseService.Consumers
{
    public class InventoryConsumer : IConsumer<CreateOrderModel>
    {
        private IPublishEndpoint _bus;
        private WarehouseDbContext _dbContext;
        private IDistributedCache _distributedCache;

        public InventoryConsumer(
            IPublishEndpoint bus,
            WarehouseDbContext dbContext,
            IDistributedCache distributedCache
        )
        {
            _bus = bus;
            _dbContext = dbContext;
            _distributedCache = distributedCache;
        }

        public async Task Consume(ConsumeContext<CreateOrderModel> context)
        {
            var drafData = context.Message;
            var failureMessage = new ProductsAreNotReadyForSaleModel { OrderId = drafData.OrderId };
            using (var transation = _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var productsInWarehouse = await _dbContext
                        .Warehouses
                        .Where(w => drafData.ProductList.Any(p => p.ProductId == w.ProductId))
                        .ToListAsync();
                    
                    if (productsInWarehouse
                        .Any(p => drafData
                                .ProductList
                                .Any(pt => pt.ProductId == p.ProductId && pt.Quantity > p.Quantity)
                        )
                    )
                    {
                        failureMessage.Message = "Some products are not enough in the warehouse.";
                        await _bus.Publish(failureMessage);
                    }
                    else
                    {
                        Parallel.ForEach(productsInWarehouse, item =>
                        {
                            var productFromMessage = drafData.ProductList.FirstOrDefault(p => p.ProductId == item.ProductId);
                            item.Quantity -= productFromMessage.Quantity;
                        });
                        await _dbContext.SaveChangesAsync();
                        await _dbContext.Database.CommitTransactionAsync();

                        string productListJsonString = JsonConvert.SerializeObject(productsInWarehouse);
                        await _distributedCache
                            .SetStringAsync($"{RedisEntityKeys.WAREHOUSE_PREFIX}-{drafData.OrderId}", productListJsonString);

                        var message = new ProductsAreReadyForSaleModel { OrderId = drafData.OrderId };
                        await _bus.Publish(message);
                    }
                }
                catch (Exception ex)
                {
                    failureMessage.Message = ex.Message;
                    await _bus.Publish(failureMessage);
                    await _dbContext.Database.RollbackTransactionAsync();
                }
            }
        }
    }
}

