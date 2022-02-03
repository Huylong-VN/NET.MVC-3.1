using System;
using System.Collections.Generic;

namespace Food_Market.Data.Entity
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTime CreateAt { get; set; }= DateTime.Now;
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public string Country { set; get; }
        public string City { set; get; }
        public string Address { set; get; }
        public string Appartment { set; get; }
        public string Phone { set; get; }
        public string Email { set; get; }
        public string Method { set; get; }

        public bool Status { set; get; } = false;

        public string Total { set; get; }

        public List<ProductOrder> ProductOrders { get; set; }
        public AppUser AppUser { get; set; }
        public Guid UserId { get; set; }
    }
}
