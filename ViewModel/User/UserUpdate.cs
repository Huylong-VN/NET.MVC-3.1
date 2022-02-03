using Microsoft.AspNetCore.Http;
using System;

namespace Food_Market.ViewModel.User
{
    public class UserUpdate
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public IFormFile Image { set; get; }
    }
}
