using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Messages;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class StockRollBackMessageConsumer : IConsumer<IStockRollBackMessage>
    {
        private readonly AppDbContext appDbContext;
        private readonly ILogger<StockRollBackMessageConsumer> logger;

        public StockRollBackMessageConsumer(AppDbContext appDbContext, ILogger<StockRollBackMessageConsumer> logger)
        {
            this.appDbContext = appDbContext;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<IStockRollBackMessage> context)
        {
            foreach (var item in context.Message.OrderItems)
            {
                var stock = await appDbContext.Stocks.FirstOrDefaultAsync(x=>x.ProductId == item.ProductId);

                if (stock == null)
                {
                    stock.Count += item.Count;
                    await appDbContext.SaveChangesAsync();
                    return;
                }

                logger.LogInformation("");
            }           
        }
    }
}
