using MassTransit;
using Order.API.Models;
using Shared;

namespace Order.API.Consumer
{
    public class StockNotReservedEventConsumer : IConsumer<StockNotReservedEvent>
    {
        private readonly AppDbContext appDbContext;
        private readonly ILogger<StockNotReservedEvent> logger;

        public StockNotReservedEventConsumer(AppDbContext appDbContext, ILogger<StockNotReservedEvent> logger)
        {
            this.appDbContext = appDbContext;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<StockNotReservedEvent> context)
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
