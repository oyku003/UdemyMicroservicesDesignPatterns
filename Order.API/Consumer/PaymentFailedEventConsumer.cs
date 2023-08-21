using MassTransit;
using Order.API.Models;
using Shared;

namespace Order.API.Consumer
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly AppDbContext appDbContext;
        private readonly ILogger<PaymentFailedEventConsumer> logger;

        public PaymentFailedEventConsumer(AppDbContext appDbContext, ILogger<PaymentFailedEventConsumer> logger)
        {
            this.appDbContext = appDbContext;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var order = await appDbContext.Orders.FindAsync(context.Message.OrderId);

            if (order == null)
            {
                logger.LogError("");
                return;
            }

            order.Status = OrderStatus.Fail;
            order.FailMessage = context.Message.Message;
            await appDbContext.SaveChangesAsync();
            logger.LogInformation("");
        }
    }
}
