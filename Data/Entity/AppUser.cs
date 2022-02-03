using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Food_Market.Data.Entity
{
    public class AppUser : IdentityUser<Guid>
    {
        public string FullName { get; set; }
        public string Image { set; get; }
        public string Address { set; get; }
        public DateTime CreatedAt { get; set; }= DateTime.Now;

        public List<AppUserRole> AppUserRoles { get; set; }
        public List<Order> Orders { get; set; }

    }
}