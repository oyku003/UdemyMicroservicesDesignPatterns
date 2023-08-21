using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public  interface IOrderCreatedSagaEvent :CorrelatedBy<Guid>
    {
        public List<OrderItemMessage> orderItems { get; set; }// = new List<OrderItemMessage>();
    }
}
