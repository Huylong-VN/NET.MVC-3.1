using Food_Market.Data;
using Food_Market.Data.Entity;
using Food_Market.Data.Services;
using Food_Market.ViewModel;
using Food_Market.ViewModel.Product;
using Food_Market.ViewModel.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Food_Market.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IStorageService _storageService;
        private readonly MarketDbContext _context;

        public AdminController(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager, IStorageService storageService, MarketDbContext context, IHttpContextAccessor httpContext)
        {
            _roleManager = roleManager;
            _context = context;
            _storageService = storageService;
            _userManager = userManager;
        }
        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            return fileName;
        }
        public async Task<IActionResult> Index(string keyword, int p = 1, int s = 10)
        {
            ViewData["FullName"] = HttpContext.Session.GetString("fullname");
            var query = _userManager.Users.Include(x=>x.AppUserRoles).ThenInclude(x=>x.AppRole).AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query=query.Where(x => x.UserName.Contains(keyword) || x.Email.Contains(keyword));
            }
            var result = await query.OrderByDescending(x => x.CreatedAt).Skip((p - 1) * s).Take(s)
                .ToListAsync();
            ViewBag.ListUser = result;
            var returnvalue = new PagedResultBase()
            {
                PageIndex = p,
                Keyword = keyword,
                PageSize = s,
                TotalRecords = query.Count()
            };
            return View(returnvalue);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(UserCreate user)
        {
            var check = await _userManager.FindByNameAsync(user.UserName);
            if (check != null) {
                TempData["error"] = "Username existed !!";
                return View();
            }
            var appuser = new AppUser()
            {
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                CreatedAt= DateTime.Now,
                UserName = user.UserName,
            };
            if(user.Image != null)
            {
                appuser.Image = await SaveFile(user.Image);
            }
            var result=await _userManager.CreateAsync(appuser,user.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("index");
            }
            TempData["error"] = result.Errors;
            return View();
        }
        [HttpGet("admin/update/{Id}")]
        public async Task<IActionResult> Update(Guid Id)
        {
            var user = await _userManager.FindByIdAsync(Id.ToString());
            if (user != null)
            {
                var result = new UserUpdate()
                {
                    Address = user.Address,
                    Email = user.Email,
                    FullName = user.FullName,
                    Id = Id,
                    PhoneNumber = user.PhoneNumber,
                };
                return View(result);
            }
            return RedirectToAction("Error", "Home");

        }
        [HttpPost()]
        public async Task<IActionResult> Update(UserUpdate request)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
            {
                return View(request);
            }
            if(request.Email != null)
            {
                user.Email = request.Email;
            }
            if(request.FullName != null)
            {
                user.FullName = request.FullName;

            }
            if(request.Address != null)
            {
                user.Address = request.Address;

            }

            if (request.Image != null)
            {
                if(user.Image != null)
                {
                    await _storageService.DeleteFileAsync(user.Image);
                }
                user.Image = await SaveFile(request.Image);
            }
            await _userManager.UpdateAsync(user);
            return RedirectToAction("index");
        }
        [HttpGet("admin/delete/{Id}")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var user = await _userManager.FindByIdAsync(Id.ToString());
            if(user != null)
            {
                await _userManager.DeleteAsync(user);
                return RedirectToAction("index");
            }
            TempData["error"] = "Can't find user";
            return RedirectToAction("index");
        }


        public async Task<IActionResult> Product(string keyword, int p = 1, int s = 10)
        {
            var query = _context.Products.Include(x => x.Supplier).Include(x => x.Category).AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword) || x.Description.Contains(keyword));
            }
            var result = await query.OrderByDescending(x => x.CreatedAt).Skip((p - 1) * s).Take(s)
                .ToListAsync();
            ViewBag.List = result;
            var returnvalue = new PagedResultBase()
            {
                PageIndex = p,
                Keyword = keyword,
                PageSize = s,
                TotalRecords = query.Count()
            };
            return View(returnvalue);
        }


        //Create product
        [HttpGet]
        public IActionResult CreateProduct()
        {
            ViewBag.listCategory = _context.Categories.ToList();
            ViewBag.listSupplier = _context.Suppliers.ToList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductCreate request)
        {
            var product = new Product()
            {
                CreatedAt = DateTime.Now,
                CategoryId = request.CategoryId,
                Description = request.Description,
                SupplierId = request.SupplierId,
                Name = request.Name,
                Price = request.Price,
                SalePrice = request.SalePrice,
            };
            if (request.Image != null)
            {
                product.Image = await SaveFile(request.Image);
            }
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Product));
        }
        public async Task<IActionResult> UpdateProduct(ProductUpdate request)
        {
            var product = await _context.Products.FindAsync(request.Id);
            if (product != null)
            {
                product.Name = request.Name;
                product.Price = request.Price;
                product.SalePrice = request.SalePrice;
                product.Description = request.Description;
                product.SupplierId = request.SupplierId;
                product.CategoryId = request.CategoryId;
                if(request.Image != null)
                {
                    if(product.Image != null)
                    {
                        await _storageService.DeleteFileAsync(product.Image);
                    }
                    product.Image = await SaveFile(request.Image);
                }
                 _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Product));
            }
            return RedirectToAction("Error", "Home");

        }
        [HttpGet("admin/updateproduct/{Id}")]
        public IActionResult UpdateProduct(Guid Id)
        {
            ViewBag.listCategory = _context.Categories.ToList();
            ViewBag.listSupplier = _context.Suppliers.ToList();
            var result = _context.Products.Find(Id);
            var product = new ProductUpdate()
            {
                CategoryId = Id,
                Description = result.Description,
                SupplierId = result.SupplierId,
                SalePrice = result.SalePrice,
                Id = Id,
                Name = result.Name,
                Price = result.Price,
            };
            return View(product);
        }

        [HttpGet("admin/deleteproduct/{Id}")]
        public async Task<IActionResult> DeleteProduct(Guid Id)
        {
            var product = await _context.Products.FindAsync(Id);
            if (product != null)
            {
                if (product.Image != null)
                {
                    await _storageService.DeleteFileAsync(product.Image);
                }
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("index");
            }
            TempData["error"] = "Can't find Product";
            return RedirectToAction("index");
        }

        [HttpGet]
        public async Task<IActionResult> RoleAssign(Guid Id)
        {
            var user = await _userManager.FindByIdAsync(Id.ToString());
            var status = await _userManager.GetRolesAsync(user);
            ViewBag.id = Id;
            if (status.Count>0)
            {
                ViewBag.status = true;
            }
            else
            {
                ViewBag.status = false;
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RoleAssign()
        {
            var user = await _userManager.FindByIdAsync(Request.Form["Id"]);
            if(user != null)
            {
                if (Request.Form["status"] != "on")
                {
                    
                    var roles = await _userManager.GetRolesAsync(user);
                    var tt = await _userManager.IsInRoleAsync(user,"admin");
                    var tt2 = await _userManager.IsInRoleAsync(user,"Admin");
                    var tt3 = await _userManager.IsInRoleAsync(user,"ADMIN");
                    var result=await _userManager.RemoveFromRolesAsync(user, roles);
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "admin");
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Category(string keyword, int p = 1, int s = 10)
        {
            var query = _context.Categories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword) || x.Description.Contains(keyword));
            }
            var result = await query.OrderByDescending(x => x.CreatedAt).Skip((p - 1) * s).Take(s)
                .ToListAsync();
            ViewBag.List = result;
            var returnvalue = new PagedResultBase()
            {
                PageIndex = p,
                Keyword = keyword,
                PageSize = s,
                TotalRecords = query.Count()
            };
            return View(returnvalue);
        }
        public IActionResult CreateCategory()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if(category == null)
            {
                return View();
            }
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Category));
        }
        public async Task<IActionResult> UpdateCategory(Guid Id)
        {
            var category = await _context.Categories.FindAsync(Id);
            if(category == null) return RedirectToAction("index");
            return View(category);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateCategory(Category category)
        {
            var up = await _context.Categories.FindAsync(category.Id);
            up.Name = category.Name;
            up.Description = category.Description;
            _context.Categories.Update(up);
            await _context.SaveChangesAsync();
            return RedirectToAction("category");
        }
        public async Task<IActionResult> DeleteCategory(Guid Id)
        {
            var category = await _context.Categories.FindAsync(Id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Category));
        }
        public async Task<IActionResult> Supplier(string keyword, int p = 1, int s = 10)
        {
            var query = _context.Suppliers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword) || x.Phone.Contains(keyword));
            }
            var result = await query.OrderByDescending(x => x.CreatedAt).Skip((p - 1) * s).Take(s)
                .ToListAsync();
            ViewBag.List = result;
            var returnvalue = new PagedResultBase()
            {
                PageIndex = p,
                Keyword = keyword,
                PageSize = s,
                TotalRecords = query.Count()
            };
            return View(returnvalue);
        }
        public IActionResult CreateSupplier()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateSupplier(Supplier supplier)
        {
            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Supplier));
        }
        public async Task<IActionResult> UpdateSupplier(Guid Id)
        {
            var supplier = await _context.Suppliers.FindAsync(Id);
            if (supplier == null) return RedirectToAction("index");
            return View(supplier);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateSupplier(Supplier supplier)
        {
            var up = await _context.Suppliers.FindAsync(supplier.Id);
            up.Name = supplier.Name;
            up.Address = supplier.Address;
            up.Phone = supplier.Phone;
            _context.Suppliers.Update(up);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Supplier));
        }
        public async Task<IActionResult> DeleteSupplier(Guid Id)
        {
            var supplier = await _context.Suppliers.FindAsync(Id);
            if (supplier != null)
            {
                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Supplier));
        }
        public async Task<IActionResult> Order(string keyword, int p = 1, int s = 10)
        {
            var query = _context.Orders.Include(x => x.ProductOrders).ThenInclude(x=>x.Product).Include(x => x.AppUser).AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.LastName.Contains(keyword) || x.FirstName.Contains(keyword));
            }
            var result = await query.OrderByDescending(x => x.CreateAt).Skip((p - 1) * s).Take(s)
                .ToListAsync();
            ViewBag.List = result;
            var returnvalue = new PagedResultBase()
            {
                PageIndex = p,
                Keyword = keyword,
                PageSize = s,
                TotalRecords = query.Count()
            };
            return View(returnvalue);
        }
        public async Task<IActionResult> DeleteOrder(Guid Id)
        {
            var order = await _context.Orders.FindAsync(Id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Order));
        }
        public async Task<IActionResult> RemoveOrder(Guid Id)
        {
            var order = await _context.Orders.FindAsync(Id);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Order));
        }
        public async Task<IActionResult> TickOrder(Guid Id)
        {
            var order = await _context.Orders.FindAsync(Id);
            order.Status = !order.Status;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Order));
        }

    }
}