using Microsoft.AspNetCore.Http;
using System;

namespace Food_Market.ViewModel.Product
{
    public class ProductUpdate
    {
        public Guid Id { set; get; }
        public string Name { set; get; }
        public string Description { set; get; }
        public IFormFile Image { set; get; }
        public string Price { get; set; }
        public string SalePrice { set; get; }
        public Guid SupplierId { get; set; }

        public Guid CategoryId { get; set; }

    }
}
