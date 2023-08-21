using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachineWorkerService.Models
{
    public class OrderStateDbContext : SagaDbContext//db contexte denk geliyor
    {
        public OrderStateDbContext(DbContextOptions<OrderStateDbContext> options) : base(options)
        {

        }
        protected override IEnumerable<ISagaClassMap> Configurations {get{yield return new OrderStateMap(); } }
    }
}
