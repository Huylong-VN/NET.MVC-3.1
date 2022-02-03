using Food_Market.Data;
using Food_Market.Data.Entity;
using Food_Market.Data.Services;
using Food_Market.Models;
using Food_Market.ViewModel;
using Food_Market.ViewModel.User;
using Food_Market.ViewModel.Voucher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Food_Market.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MarketDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IStorageService _storageService;

        public HomeController(ILogger<HomeController> logger, MarketDbContext context, UserManager<AppUser> userManager, IStorageService storageService)
        {
            _storageService=storageService;
            _userManager = userManager;
            _context=context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string keyword, int p = 1, int s = 10)
        {
            TempData["Voucher"]=0;
            var cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            if (cart == null) ViewBag.totalCart = 0;
            else ViewBag.totalCart = cart.Count();

            var query = _context.Products.Include(x => x.Supplier).Include(x => x.Category).AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword) || x.Description.Contains(keyword) || x.Category.Name.Contains(keyword));
            }
            var result = await query.OrderBy(x => x.CreatedAt).Skip((p - 1) * s).Take(s)
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

        public IActionResult Cart()
        {
            var cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            if (cart == null) {
                return View();
            }
            var total= cart.Sum(item => int.Parse(item.Product.SalePrice) * item.Quantity);
            ViewBag.cart = cart;
            ViewBag.total = total;
            ViewBag.totalCart = cart.Count();
            return View();
        }
        public async Task<IActionResult> AddVoucher()
        {
            var cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            if (cart == null)
            {
                TempData["error"] = "You don't have any products yet";
                RedirectToAction(nameof(Cart));
            }
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(x=>x.Code.Equals(Request.Form["code"]));
            if (voucher == null) {
                TempData["error"] = "can't find voucher";
                return RedirectToAction(nameof(Cart));
            }
            TempData["error"] = "You get reduced:: " + voucher.ReducePrice+"$";
            var subTotal = cart.Sum(item => int.Parse(item.Product.SalePrice) * item.Quantity);
            TempData["Voucher"] = voucher.ReducePrice;
            return RedirectToAction(nameof(Cart));

        }

        public IActionResult AddToCart(Guid Id)
        {
            if (SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart") == null)
            {
                List<Item> cart = new List<Item>();
                cart.Add(new Item { Product = _context.Products.Find(Id), Quantity = 1 });
                SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            }
            else
            {
                List<Item> cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
                int index = isExist(Id);
                if (index != -1)
                {
                    cart[index].Quantity++;
                }
                else
                {
                    cart.Add(new Item { Product = _context.Products.Find(Id), Quantity = 1 });
                }
                SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            }
            return RedirectToAction(nameof(Cart));
        }

        public IActionResult Remove(Guid id)
        {
            List<Item> cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            int index = isExist(id);
            cart.RemoveAt(index);
            SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            return RedirectToAction(nameof(Cart));
        }

        private int isExist(Guid id)
        {
            List<Item> cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            for (int i = 0; i < cart.Count; i++)
            {
                if (cart[i].Product.Id.Equals(id))
                {
                    return i;
                }
            }
            return -1;
        }




        public async Task<IActionResult> Shopping(string keyword, int p = 1, int s = 10)
        {
            var cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            if (cart == null) ViewBag.totalCart = 0;
            else ViewBag.totalCart = cart.Count();
            var query = _context.Products.Include(x => x.Supplier).Include(x => x.Category).AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword) || x.Description.Contains(keyword) || x.Category.Name.Contains(keyword));
            }
            var result = await query.OrderByDescending(x => x.CreatedAt).Skip((p - 1) * s).Take(s)
                .ToListAsync();
            ViewBag.List = result;
            ViewBag.listCategory = _context.Categories.ToList();
            var returnvalue = new PagedResultBase()
            {
                PageIndex = p,
                Keyword = keyword,
                PageSize = s,
                TotalRecords = query.Count()
            };
            return View(returnvalue);
        }
        public async Task<IActionResult> Detail(Guid Id)
        {
            var product = await _context.Products.FindAsync(Id);
            ViewBag.RelativeProduct=await _context.Products.Where(x=>x.Id != Id).Take(4).ToListAsync();
            return View(product);
        }
        [Authorize]
        public IActionResult CheckOut(string Discount, string SubTotal, string Total)
        {
            if (Discount == null || SubTotal==null || Total == null ) return RedirectToAction(nameof(Shopping));
            var checkout = new Checkout()
            {
                Discount = int.Parse(Discount),
                Subtotal = int.Parse(SubTotal),
                Total = int.Parse(Total)
            };
            ViewBag.Checkout = checkout;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            order.UserId = (await _userManager.GetUserAsync(User)).Id;
            var addorder = await _context.Orders.AddAsync(order);
            foreach (var item in cart)
            {
                await _context.ProductOrders.AddAsync(new ProductOrder()
                {
                    CreateAt = DateTime.Now,
                    OrderId = order.Id,
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity
                });
            }

            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("cart");
            return RedirectToAction(nameof(BuySuccess));
        }
        public IActionResult BuySuccess()
        {
            return View();
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Redirect("/account/login");
            return View(user);
        }
        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            return fileName;
        }
        [Authorize]
        public async Task<IActionResult> UpdateProfile()
        {
            var userupdate = await _userManager.GetUserAsync(User);
            if (Request.Form.Files["image"] != null)
            {
                if (userupdate.Image != null)
                {
                    await _storageService.DeleteFileAsync(userupdate.Image);
                }
                userupdate.Image = await SaveFile(Request.Form.Files["image"]);
            }
            await _userManager.UpdateAsync(userupdate);
            return RedirectToAction(nameof(Profile));
        }
        [Authorize]
        public async Task<IActionResult> Order(string keyword, int p = 1, int s = 10)
        {
            var userId=await _userManager.GetUserAsync(User);
            var query = _context.Orders.Include(x => x.ProductOrders).ThenInclude(x => x.Product).
                Where(x=>x.UserId.Equals(userId.Id)).AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.Country.Contains(keyword) || x.Email.Contains(keyword) || x.FirstName.Contains(keyword) ||
                x.Appartment.Contains(keyword) ||x.City.Contains(keyword) ||x.Phone.Contains(keyword));
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
        [Authorize]
        public async Task<IActionResult> RemoveOrder(Guid Id)
        {
            var order = await _context.Orders.FindAsync(Id);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Order));
        }
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        } 
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword change)
        {
            if (!ModelState.IsValid)
            {
                return View(change);
            }
            var user = await _userManager.GetUserAsync(User);
            var result=await _userManager.ChangePasswordAsync(user, change.OldPassword, change.NewPassword);
            if (result.Succeeded)
            {
                TempData["mess"] = "Change Success";
                return RedirectToAction(nameof(ChangePassword));
            }
            TempData["mess"] = "Password Old Wrong";
            return RedirectToAction(nameof(ChangePassword));
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
