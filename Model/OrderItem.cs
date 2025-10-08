using System.ComponentModel.DataAnnotations.Schema;

namespace PRM_BE.Model
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int FlowerId { get; set; }
        public Flower Flower { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
        //line  total = unit price * quantity
        public decimal LineTotal { get; set; }
    }
}
