using DemoWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Diagnostics;

namespace DemoWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDatabase _redisDB;
        const string key = "mykey";

        public HomeController(ILogger<HomeController> logger, IDatabase redisDB)
        {
            _logger = logger;
            _redisDB = redisDB ?? throw new ArgumentNullException();
        }

        public IActionResult Index()
        {
            return View();
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

        [HttpGet]
        public async Task<IActionResult> UpdateTimeStamp()
        {
                await _redisDB.StringSetAsync(key, DateTime.UtcNow.ToString("s"));
                return Ok("Last timestamp: " + (await _redisDB.StringGetAsync(key)).ToString());
            
        }
    }
}
