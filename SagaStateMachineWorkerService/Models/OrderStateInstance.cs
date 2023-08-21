using Automatonymous;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachineWorkerService.Models
{
    public class OrderStateInstance : SagaStateMachineInstance  //dbde tabloya karşılık geliyor, product gibi düşünebilirsin.her bir sipariş için bir instance oluşacak dbde ya da in memoryde nerde tutuluyorsa
    { 
        public Guid CorrelationId { get; set; }//her bir state machine instance ı için random id üretilir.Eventler geldikce hangi event hangi dbdeki satırla ilişkili onu anlayabilmemk için id üretir.
        public string CurrentState { get; set; }
        public string BuyerId { get; set; }
        public int OrderId { get; set; }
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string CVV { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; }
        public override string ToString()
        {
            var properties = GetType().GetProperties();
            var stringBuilder=new StringBuilder();

            properties.ToList().ForEach(p =>
            {
                var value=p.GetValue(this, null);
                stringBuilder.AppendLine($"{p.Name}:{value}");
            });

            stringBuilder.Append("----------------------");
            return stringBuilder.ToString();
        }
    }
}
