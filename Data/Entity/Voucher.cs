using System;

namespace Food_Market.Data.Entity
{
    public class Voucher
    {
        public Guid Id { set; get; }
        public string Code { set; get; }
        public int ReducePrice { set; get; }
        public DateTime CreateAt { set; get; }
    }
}
