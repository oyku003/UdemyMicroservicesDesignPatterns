using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    public class OrderCreatedSagaEvent : IOrderCreatedSagaEvent
    {
        public OrderCreatedSagaEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public List<OrderItemMessage> orderItems { get; set; }// = new List<OrderItemMessage>();

        public Guid CorrelationId { get; }
    }
}
