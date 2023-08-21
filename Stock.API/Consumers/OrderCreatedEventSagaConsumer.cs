using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Shared.Interfaces;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventSagaConsumer : IConsumer<IOrderCreatedSagaEvent>
    {
        private readonly AppDbContext _context;
        private ILogger<OrderCreatedEventConsumer> logger;
        private readonly ISendEndpointProvider sendEndpointProvider;
        private readonly IPublishEndpoint publishEndpoint;

        public OrderCreatedEventSagaConsumer(AppDbContext context, ILogger<OrderCreatedEventConsumer> logger, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            this.logger = logger;
            this.sendEndpointProvider = sendEndpointProvider;
            this.publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<IOrderCreatedSagaEvent> context)
        {
            var stockResult = new List<bool>();
            foreach (var item in context.Message.orderItems)
            {
                stockResult.Add(await this._context.Stocks.AnyAsync(x => x.ProductId == item.ProductId && x.Count > item.Count));
            }

            if (stockResult.Any() && stockResult.All(x => x.Equals(true)))
            {
                foreach (var item in context.Message.orderItems)
                {
                    var stock = await this._context.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);

                    if (stock != null)
                    {
                        stock.Count -= item.Count;
                    }
                    await _context.SaveChangesAsync();
                }

                StockReservedSagaEvent stockReservedEvent = new StockReservedSagaEvent(context.Message.CorrelationId)
                {
                    OrderItems = context.Message.orderItems
                };

                await publishEndpoint.Publish(stockReservedEvent);
            }
            else
            {
                await publishEndpoint.Publish(new StockNotReservedSagaEvent(context.Message.CorrelationId)
                {
                    Reason = "Not enough stock"
                });

                logger.LogInformation($"Not enough stock for buyer ıd :{context.Message.CorrelationId}");
            }
        }
    }
}
