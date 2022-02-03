using Food_Market.Data.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Food_Market.Data
{
    public class MarketDbContext : IdentityDbContext<AppUser, AppRole, Guid, IdentityUserClaim<Guid>, AppUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public MarketDbContext(DbContextOptions options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Voucher>(x =>
            {
                x.ToTable("Vouchers");
                x.HasKey(x => x.Id);
                x.HasData(new Voucher()
                {
                    CreateAt = DateTime.Now,
                    Code = "GIAM10",
                    ReducePrice = 10,
                    Id = Guid.NewGuid()
                });
            });
            builder.Entity<Order>(x =>
            {
                x.ToTable("Orders");
                x.HasKey(x => x.Id);
                x.HasOne(x => x.AppUser).WithMany(x => x.Orders).HasForeignKey(x => x.UserId);
            });
            builder.Entity<ProductOrder>(x =>
            {
                x.ToTable("ProductOrders");
                x.HasKey(x=>new {x.OrderId, x.ProductId});
                x.HasOne(x => x.Order).WithMany(x => x.ProductOrders).HasForeignKey(x => x.OrderId);
                x.HasOne(x => x.Product).WithMany(x => x.ProductOrders).HasForeignKey(x => x.ProductId);
            });

            builder.Entity<Product>(x =>
            {
                x.ToTable("Products");
                x.HasKey(x => x.Id);
                x.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId);
                x.HasOne(x => x.Supplier).WithMany(x => x.Products).HasForeignKey(x => x.SupplierId);
            });
         
            builder.Entity<Category>(x =>
            {
                x.ToTable("Categories");
                x.HasKey(x => x.Id);
                x.HasData(new Category()
                {
                    CreatedAt = DateTime.Now,
                    Description = "Gồm các loại thịt như thịt lợn, thịt gà , thịt bò và một số loại khác",
                    Name = "Thịt",
                    Id = Guid.NewGuid(),
                });
                x.HasData(new Category()
                {
                    CreatedAt = DateTime.Now,
                    Description = "Rau gồm các loại như là rau cải, súp lơ, cải ngọt ...",
                    Name = "Rau",
                    Id = Guid.NewGuid(),
                });
                x.HasData(new Category()
                {
                    CreatedAt = DateTime.Now,
                    Description = "Những chú cá to tươi sống trong ngày",
                    Name = "Cá",
                    Id = Guid.NewGuid(),
                });
                x.HasData(new Category()
                {
                    CreatedAt = DateTime.Now,
                    Description = "Gồm các loại hoa quả nhập khẩu từ các nước, đa số là hoa quả tươi trong ngày",
                    Name = "Hoa Quả",
                    Id = Guid.NewGuid(),
                });
                x.HasData(new Category()
                {
                    CreatedAt = DateTime.Now,
                    Description = "Sản phẩm là đồ ăn sản có thể sử dụng luôn đã qua sơ chế cơ bản",
                    Name = "Đồ đóng hộp",
                    Id = Guid.NewGuid(),
                });
            });
            builder.Entity<Supplier>(x =>
            {
                x.ToTable("Suppliers");
                x.HasKey(x => x.Id);

                x.HasData(new Supplier()
                {
                    Address = "Hải phòng, Việt nam",
                    CreatedAt = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Name = "Công ty Rau củ Hải phòng",
                    Phone = "0102010221",
                });
                x.HasData(new Supplier()
                {
                    Address = "Hà nội, Việt nam",
                    CreatedAt = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Name = "Công ty chuyên sản xuất thịt lợn",
                    Phone = "0392933232",
                });

                x.HasData(new Supplier()
                {
                    Address = "Nghệ An, Việt nam",
                    CreatedAt = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Name = "Công ty chuyên cung cấp rau sạch",
                    Phone = "032032322",
                });
            });


            var hasher = new PasswordHasher<AppUser>();
            builder.Entity<AppUser>().HasData(new AppUser
            {
                Id = new Guid("0027068e-4c5d-4ecb-a157-b9cc063cd672"),
                UserName = "admin",
                NormalizedUserName = "admin",
                Email = "admin@gmail.com",
                NormalizedEmail = "admin@gmail.com",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "1"),
                SecurityStamp = string.Empty,
                FullName = "Nguyen Admin",
                PhoneNumber = "02002012",
            });
            builder.Entity<AppRole>().HasData(new AppRole()
            {
                Id = new Guid("cc88ab6f-5d66-4c30-a60e-8f5254f1e112"),
                Name = "admin",
                NormalizedName = "admin",
            });
            builder.Entity<AppUserRole>().HasData(new AppUserRole()
            {
                RoleId = new Guid("cc88ab6f-5d66-4c30-a60e-8f5254f1e112"),
                UserId = new Guid("0027068e-4c5d-4ecb-a157-b9cc063cd672")
            });


            builder.Entity<AppUserRole>(x =>
            {
                x.HasKey(x => new { x.RoleId, x.UserId });
                x.HasOne(x => x.AppUser).WithMany(x => x.AppUserRoles).HasForeignKey(x => x.UserId);
                x.HasOne(x => x.AppRole).WithMany(x => x.AppUserRoles).HasForeignKey(x => x.RoleId);
            });

            //IdentityUserLogin
            builder.Entity<IdentityUserLogin<Guid>>().HasKey(x => x.UserId);
            //IdentityUserToken
            builder.Entity<IdentityUserToken<Guid>>().HasKey(x => x.UserId);      

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<ProductOrder> ProductOrders { get; set; }

    }
}