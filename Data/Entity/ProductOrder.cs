using System;

namespace Food_Market.Data.Entity
{
    public class ProductOrder
    {
        public Product Product { get; set; }
        public Guid ProductId { set; get; }
        public Order Order { get; set; }
        public Guid OrderId { set; get; }
        public int Quantity { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
