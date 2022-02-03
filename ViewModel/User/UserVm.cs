using System;

namespace Food_Market.ViewModel.User
{
    public class UserVm
    {
       public string FullName { get; set; } 
       public string Email { get; set; } 
       public string Address { get; set; } 
       public string Image { get; set; } 
       public Guid Id { get; set; } 
       public DateTime CreatedAt { get; set; } 
       public string PhoneNumber { get; set; } 
       public string UserName { get; set; } 
    }
}
