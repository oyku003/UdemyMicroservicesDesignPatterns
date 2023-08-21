using MassTransit;
using Order.API.Models;
using Shared;
using Shared.Events;
using Shared.Interfaces;

namespace Order.API.Consumer
{
    public class OrderRequestCompletedEventConsumer : IConsumer<IOrderRequestCompletedEvent>
    {
        private readonly AppDbContext appDbContext;
        private readonly ILogger<OrderRequestCompletedEventConsumer> logger;

        public OrderRequestCompletedEventConsumer(AppDbContext appDbContext, ILogger<OrderRequestCompletedEventConsumer> logger)
        {
            this.appDbContext = appDbContext;
            this.logger = logger;
        }
        public async Task Consume(ConsumeContext<IOrderRequestCompletedEvent> context)
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
