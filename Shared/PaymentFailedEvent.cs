using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class PaymentFailedEvent
    {
        public string Message { get; set; }
        public int OrderId { get; set; }
        public string BuyerId { get; set; }
        public List<OrderItemMessage>  orderItemMessages { get; set; }
    }
}
