using eShop.Data;
using eShop.Interfaces;
using eShop.Models;
using eShop.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace eShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;

        public HomeController(ILogger<HomeController> logger, IProductService productService)
        {
            _logger = logger;
            _productService = productService;

        }

        public IActionResult IndexAsync()
        {
            Stopwatch sw = Stopwatch.StartNew();

            List<Product> productList = (_productService.GetAllProductsAsync()).GetAwaiter().GetResult();
            
            var _lastViewedId = HttpContext.Session.GetInt32(SessionConstants.LastViewed);

            if (_lastViewedId != null)
            {
                //var _lastViewedProduct = await _productService.GetProductByIdAsync((int) _lastViewedId);
                var _lastViewedProduct = productList.Where(_item => _item.Id == _lastViewedId).FirstOrDefault();
                if( _lastViewedProduct != null )
                {
                    ViewData["lastViewedName"] = _lastViewedProduct.Name;
                    ViewData["lastViewedBrand"] = _lastViewedProduct.Brand;
                    ViewData["_id"]= _lastViewedProduct.Id;
                    ViewData["_name"]= _lastViewedProduct.Name;
                    ViewData["_image"]=_lastViewedProduct.Image;
                    ViewData["_price"]=_lastViewedProduct.Price;
                }
            }
            sw.Stop();
            double ms = sw.ElapsedTicks / (Stopwatch.Frequency / (1000.0));



            //var userOrSessionName = Request.HttpContext.User.Identity.IsAuthenticated? Request.HttpContext.User.Identity.Name : Guid.NewGuid().ToString();
            var userOrSessionName = "";
            if (Request.HttpContext.User.Identity.IsAuthenticated)
            {
                userOrSessionName = Request.HttpContext.User.Identity.Name;
            }
            else if (Request.Cookies.ContainsKey(Constants.UNIQUE_CACHE_TAG))
            {
                userOrSessionName = Request.Cookies[Constants.UNIQUE_CACHE_TAG];
            }
            else 
            { 
                userOrSessionName = Guid.NewGuid().ToString();
                var cookieOptions = new CookieOptions { IsEssential = true };
                Response.Cookies.Append(Constants.UNIQUE_CACHE_TAG, userOrSessionName, cookieOptions);
            }


            ViewData["userUniqueShoppingKey"] = userOrSessionName;

            ViewData["pageLoadTime"] = ms;


            return View(productList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Details(int id)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Product _product = await _productService.GetProductByIdAsync(id);
            sw.Stop();
            double ms = sw.ElapsedTicks / (Stopwatch.Frequency / (1000.0));

            if (_product == null)
            {
                return NotFound();
            }

            HttpContext.Session.SetInt32(SessionConstants.LastViewed, id);

            ViewData["pageLoadTime"] = ms;

            return View(_product);
        }
    }
}