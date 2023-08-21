using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Models;
using Shared;
using Shared.Events;
using Shared.Interfaces;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IPublishEndpoint publishEndpoint;
        private readonly ISendEndpointProvider sendEndpointProvider;
        public OrdersController(AppDbContext context, IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpointProvider)
        {
            this.context = context;
            this.publishEndpoint = publishEndpoint;
            this.sendEndpointProvider = sendEndpointProvider;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderCreateDto)
        {
            var newOrder = new Models.Order
            {
                BuyerId = orderCreateDto.BuyerId,
                Status = OrderStatus.Suspend,
                Address = new Address { Line=orderCreateDto.address.Line, District = orderCreateDto.address.District, Province = orderCreateDto.address.Province},
                CreatedDate= DateTime.Now,
                FailMessage= String.Empty

            };

            orderCreateDto.orderItems.ForEach(item =>
            {
                newOrder.Items.Add(new OrderItem()
                {
                    Price=item.Price,
                    ProductId=item.ProductId,
                    Count=item.Count
                });
            });

            await context.AddAsync(newOrder);
            await context.SaveChangesAsync();
            var orderCreatedRequestEvent = new OrderCreatedRequestEvent //new OrderCreatedEvent()
            {
                BuyerId = orderCreateDto.BuyerId,
                OrderId = newOrder.Id,
                Payment = new PaymentMessage
                {
                    CardName = orderCreateDto.payment.CardName,
                    CardNumber = orderCreateDto.payment.CardNumber,
                    CVV = orderCreateDto.payment.CVV,
                    Expiration = orderCreateDto.payment.Expiration,
                    TotalPrice = orderCreateDto.orderItems.Sum(x => x.Price * x.Count)
                },
            };

            orderCreateDto.orderItems.ForEach(item =>
            {
                orderCreatedRequestEvent.OrderItems.Add(new OrderItemMessage { Count = item.Count, ProductId =item.ProductId });
            });

            // await publishEndpoint.Publish(OrderCreatedEvent);//subscribe olan kuyruk olmadığı için publish ile gönderdik ve exchangede tutulur havada tutulur. Direkt kuyruga göndereceksek send ile göndeririz ve kuyrukta kayıt altına alınır.Farklı servisler dinleyecekse publish, tek belirli bi consumer dinleyecekse send ile göndeririz.

            var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettingsConst.OrderSaga}"));// send ile yollayınca saga statemachine o anda ayakta olmasa bile mesaj boşa gitmnez sonradan okuyabilir.queue: standartı mass transit kütüphanesine ait.
            await sendEndpoint.Send<IOrderCreatedRequestEvent>(orderCreatedRequestEvent);

            return Ok();
        }
    }
}
