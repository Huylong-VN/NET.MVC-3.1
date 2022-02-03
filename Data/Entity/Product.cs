using System;
using System.Collections.Generic;

namespace Food_Market.Data.Entity
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Supplier Supplier { get; set; }
        public Guid SupplierId { get; set; }

        public Category Category { get; set; }
        public Guid CategoryId { get; set; }


        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Image { set; get; }
        public string Price { get; set; }
        public string SalePrice { set; get; }
        public List<ProductOrder> ProductOrders { get; set; }
    }
}
