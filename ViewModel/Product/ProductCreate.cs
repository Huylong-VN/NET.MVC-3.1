using Microsoft.AspNetCore.Http;
using System;

namespace Food_Market.ViewModel.Product
{
    public class ProductCreate
    {
        public string Name { get; set; }

        public Guid SupplierId { get; set; }

        public Guid CategoryId { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public IFormFile Image { set; get; }
        public string Price { get; set; }
        public string SalePrice { set; get; }
    }
}
