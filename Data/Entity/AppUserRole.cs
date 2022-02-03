
using Microsoft.AspNetCore.Identity;
using System;

namespace Food_Market.Data.Entity
{
    public class AppUserRole:IdentityUserRole<Guid>
    {
        public AppUser AppUser { set; get; }
        public AppRole AppRole { set; get; }
        public DateTime CreatedAt { get; set; }=DateTime.Now;
    }
}
