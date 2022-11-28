using System;
using MassTransit;
using Models;

namespace DemoMicroservices.Consumers
{
    public class OrderHasBeenCreatedConsumer : IConsumer<OrderHasBeenCreatedModel>
    {
        private IPublishEndpoint _publishEndpoint;

        public OrderHasBeenCreatedConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderHasBeenCreatedModel> context)
        {
            await Task.FromResult(0);
        }
    }
}

