using System;
using System.Collections.Generic;

namespace Food_Market.Data.Entity
{
    public class Category
    {
        public Guid Id { set; get; }
        public List<Product> Products { get; set; }
        public string Name { set; get; }
        public string Description { set; get; }
        public DateTime CreatedAt { get; set; }=DateTime.Now;
    }
}
