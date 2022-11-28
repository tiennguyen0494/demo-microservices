using MassTransit;
using Models;

namespace OrderService.Consumers
{
    public class CreateUserConsumer : IConsumer<CreateUserModel>
    {
        public IPublishEndpoint _publishEndpoint;

        public CreateUserConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<CreateUserModel> context)
        {
            var data = context.Message;
            await Task.FromResult(data);
        }
    }
}
