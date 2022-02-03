using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Food_Market.Data.Entity
{
    public class AppRole:IdentityRole<Guid>
    {
        public List<AppUserRole> AppUserRoles { get; set; }
    }
}
