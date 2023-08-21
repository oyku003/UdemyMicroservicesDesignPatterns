using Automatonymous;
using Shared;
using Shared.Events;
using Shared.Interfaces;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachineWorkerService.Models
{
    public class OrderStateMachine :MassTransitStateMachine<OrderStateInstance>//dist. transactionı yöneten class.hangi instance veritabanında tutulacaksa o verilir.
    {
        public Event<IOrderCreatedRequestEvent> OrderCreatedRequestEvent { get; set; }//statede oluşacak event interface ini aldık.
        public Event<IStockReservedSagaEvent> StockReservedSagaEvent { get; set; }//statede oluşacak event interface ini aldık.
        public Event<IPaymentCompletedEvent> PaymentCompletedEvent { get; set; }//statede oluşacak event interface ini aldık.
        public Event<IPaymentFailedEvent> PaymentFailedEvent { get; set; }//statede oluşacak event interface ini aldık.
        public Event<IStockNotReservedSagaEvent> StockNotReservedSagaEvent { get; set; }//statede oluşacak event interface ini aldık.
        public State OrderCreated { get;private set; }//bu event ile gelindiğinde state, orderCreateda set edilir.
        public State StockReserved { get;private set; }//bu event ile gelindiğinde state, StockReserved set edilir.
        public State StockNotReserved { get;private set; }//bu event ile gelindiğinde state, StockReserved set edilir.
        public State PaymentCompleted { get; private set; }
        public State PaymentFailed { get; private set; }
        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);//initail state i set ederek başladık

            Event(() => OrderCreatedRequestEvent, y => y.CorrelateBy<int>(x => x.OrderId, z => z.Message.OrderId).SelectId(context=>Guid.NewGuid()));//eventten gelen id ile dbdekjini karşılaştır eger aynısı degilse yeni id oluşturup atamasını yap

            Event(() => StockReservedSagaEvent, x => x.CorrelateById(y => y.Message.CorrelationId));
            Event(() => StockNotReservedSagaEvent, x => x.CorrelateById(y => y.Message.CorrelationId));

            Event(() => PaymentCompletedEvent, x => x.CorrelateById(y => y.Message.CorrelationId));

            Initially(
                When(OrderCreatedRequestEvent)//event geldiyse.. then kısmı business kurallarının oldğu kısım
                .Then(context =>
            {
                context.Instance.BuyerId = context.Data.BuyerId;
                context.Instance.OrderId = context.Data.OrderId;
                context.Instance.CreatedDate = DateTime.Now;
                context.Instance.CardName = context.Data.Payment.CardName;
                context.Instance.CardNumber = context.Data.Payment.CardNumber;
                context.Instance.CardName = context.Data.Payment.CardName;
                context.Instance.CVV = context.Data.Payment.CVV;
                context.Instance.Expiration = context.Data.Payment.Expiration;
                context.Instance.TotalPrice = context.Data.Payment.TotalPrice;
            })
                .Then(context => { Console.WriteLine($"OrderCreatedRequestEvent before:{context.Instance}"); })
                .Publish(context=>new OrderCreatedSagaEvent(context.Instance.CorrelationId) { orderItems = context.Data.OrderItems})//state değişim işleminden önce stock mikroservisine publish yolladık
                .TransitionTo(OrderCreated)                
                .Then(context => { Console.WriteLine($"OrderCreatedRequestEvent after:{context.Instance}"); }));//OrderCreatedRequestEvent geldiyse, then business kodlarının çalıştıgı yer.
            //Instance , veritabanına kaydedilecek olan satırı ifade eder.


            During(OrderCreated,
                When(StockReservedSagaEvent)//orderCreatedda iken  StockReservedSagaEvent gelirse..
                .TransitionTo(StockReserved)//state changed
                .Send(new Uri($"queue:{RabbitMQSettingsConst.PaymentStockReservedRequestQueueName}"), context =>new StockReservedRequestPayment(context.Instance.CorrelationId)
                {
                    OrderItems=context.Data.OrderItems,
                    payment =new PaymentMessage()
                    {
                        CardName = context.Instance.CardName,
                        CardNumber = context.Instance.CardNumber,
                        CVV = context.Instance.CVV,
                        Expiration = context.Instance.Expiration,
                         TotalPrice = context.Instance.TotalPrice
                    },
                    BuyerId=context.Instance.BuyerId
                })
               .Then(context => { Console.WriteLine($"StockReservedEvent after:{context.Instance}"); }),
                When(StockNotReservedSagaEvent)
                .TransitionTo(StockNotReserved)
                .Publish(context=>new OrderRequestFailedEvent() { OrderId=context.Instance.OrderId, Reason= context.Data.Reason })             
                );

            During(StockReserved,
                When(PaymentCompletedEvent)
                .TransitionTo(PaymentCompleted)
                .Publish(PaymentCompleted)
                .Publish(context => new OrderRequestCompletedEvent() { OrderId = context.Instance.OrderId })
                .Then(context => { Console.WriteLine($"PaymentCompletedEvent after:{context.Instance}"); }).Finalize(),
                When(PaymentFailedEvent)                
                .Publish(context => new OrderRequestFailedEvent() { OrderId = context.Instance.OrderId, Reason = context.Data.Reason })
                .Send(new Uri($"queue:{RabbitMQSettingsConst.StockRollBackMessageQueueName}"), context=>new StockRollBackMessage { OrderItems = context.Data.OrderItems })
                .TransitionTo(PaymentFailed));

            SetCompletedWhenFinalized();
        }
    }
}
