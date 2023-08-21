using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly AppDbContext appDbContext;
        private readonly ILogger<PaymentFailedEvent> logger;

        public PaymentFailedEventConsumer(AppDbContext appDbContext, ILogger<PaymentFailedEvent> logger)
        {
            this.appDbContext = appDbContext;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            foreach (var item in context.Message.orderItemMessages)
            {
                var stock = await appDbContext.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);

                if (stock != null)
                {
                    stock.Count += item.Count;
                    await appDbContext.SaveChangesAsync();
                }
            }

            logger.LogInformation($"Stock was released for Order Id ={context.Message.OrderId}");
        }
    }
}
