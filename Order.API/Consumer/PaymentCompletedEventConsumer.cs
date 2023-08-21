using MassTransit;
using Order.API.Models;
using Shared;

namespace Order.API.Consumer
{
    public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
    {
        private readonly AppDbContext appDbContext;
        private readonly ILogger<PaymentCompletedEvent> logger;

        public PaymentCompletedEventConsumer(AppDbContext appDbContext, ILogger<PaymentCompletedEvent> logger)
        {
            this.appDbContext = appDbContext;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            var order = await appDbContext.Orders.FindAsync(context.Message.OrderId);

            if (order != null)
            {
                order.Status = OrderStatus.Completed;
                await appDbContext.SaveChangesAsync();
                logger.LogInformation($"Order (Id={context.Message.OrderId}) status changed:{order.Status}");
            }
            else
            {
                logger.LogError($"Order (Id={context.Message.OrderId}) not found");
            }

        }
    }
}
