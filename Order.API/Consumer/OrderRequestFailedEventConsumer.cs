using MassTransit;
using Order.API.Models;
using Shared;
using Shared.Interfaces;

namespace Order.API.Consumer
{
    public class OrderRequestFailedEventConsumer : IConsumer<IOrderRequestFailedEvent>
    {
        private readonly AppDbContext appDbContext;
        private readonly ILogger<OrderRequestFailedEventConsumer> logger;

        public OrderRequestFailedEventConsumer(AppDbContext appDbContext, ILogger<OrderRequestFailedEventConsumer> logger)
        {
            this.appDbContext = appDbContext;
            this.logger = logger;
        }
        public async Task Consume(ConsumeContext<IOrderRequestFailedEvent> context)
        {
            var order = await appDbContext.Orders.FindAsync(context.Message.OrderId);

            if (order == null)
            {
                logger.LogError("");
                return;
            }

            order.Status = OrderStatus.Fail;
            order.FailMessage = context.Message.Reason;
            await appDbContext.SaveChangesAsync();
            logger.LogInformation("");
        }
    }
}
