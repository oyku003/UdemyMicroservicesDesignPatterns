using MassTransit;
using Shared;

namespace Payment.API.Consumer
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        private readonly ILogger<StockReservedEvent> logger;
        private readonly IPublishEndpoint publishEndpoint;

        public StockReservedEventConsumer(ILogger<StockReservedEvent> logger, IPublishEndpoint publishEndpoint)
        {
            this.logger = logger;
            this.publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            var balance = 3000m;

            if (balance>context.Message.Payment.TotalPrice)
            {
                logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was  withdrawn from credit card for user id ={context.Message.BuyerId}");

                await publishEndpoint.Publish(new PaymentCompletedEvent { BuyerId=context.Message.BuyerId, OrderId =context.Message.OrderId });
            }
            else
            {
                logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was not withdrawn from credit card for user id={context.Message.BuyerId}");

                await publishEndpoint.Publish(new PaymentFailedEvent { BuyerId = context.Message.BuyerId, OrderId = context.Message.OrderId, Message = "not enough balance"  });
            }
        }
    }
}
