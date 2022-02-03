using System;
using System.Collections.Generic;

namespace Food_Market.Data.Entity
{
    public class Supplier
    {
        public Guid Id { set; get; }
        public string Name { set; get; }
        public string Address { set; get; }
        public string Phone { set; get; }
        public DateTime CreatedAt { get; set; }=DateTime.Now;
        public List<Product> Products { set; get; }
    }
}
