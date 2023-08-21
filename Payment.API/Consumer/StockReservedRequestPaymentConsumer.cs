using MassTransit;
using Shared;
using Shared.Events;
using Shared.Interfaces;

namespace Payment.API.Consumer
{
    public class StockReservedRequestPaymentConsumer : IConsumer<IStockReservedRequestPayment>
    {
        private readonly ILogger<StockReservedRequestPaymentConsumer> logger;
        private readonly IPublishEndpoint publishEndpoint;

        public StockReservedRequestPaymentConsumer(ILogger<StockReservedRequestPaymentConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            this.logger = logger;
            this.publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<IStockReservedRequestPayment> context)
        {
            var balance = 3000m;

            if (balance > context.Message.payment.TotalPrice)
            {
                logger.LogInformation($"{context.Message.payment.TotalPrice} TL was  withdrawn from credit card for user id ={context.Message.BuyerId}");

                await publishEndpoint.Publish(new PaymentCompletedRequestEvent(context.Message.CorrelationId));
            }
            else
            {
                logger.LogInformation($"{context.Message.payment.TotalPrice} TL was not withdrawn from credit card for user id={context.Message.BuyerId}");

                await publishEndpoint.Publish(new PaymentFailedRequestEvent(context.Message.CorrelationId) { Reason ="not enough balance", OrderItems=context.Message.OrderItems});
            }
        }
    }
}
