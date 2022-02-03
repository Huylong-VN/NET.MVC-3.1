using Microsoft.AspNetCore.Http;

namespace Food_Market.ViewModel.User
{
    public class UserCreate
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FullName { set; get; }
        public string PhoneNumber { set; get; }
        public string Address { set; get; }
        public IFormFile Image { set; get; }
    }
}
