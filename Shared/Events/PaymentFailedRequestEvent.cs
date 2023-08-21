using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    public class PaymentFailedRequestEvent : IPaymentFailedEvent
    {
        public PaymentFailedRequestEvent(Guid correlationId)
        {
            CorrelationId= correlationId;
        }
        public string Reason { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }

        public Guid CorrelationId {get;}
    }
}
