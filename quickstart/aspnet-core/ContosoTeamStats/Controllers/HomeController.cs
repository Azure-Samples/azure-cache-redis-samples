using ContosoTeamStats.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ContosoTeamStats.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RedisConnection _redisConnection;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _redisConnection = new RedisConnection(configuration);
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

        public async Task<ActionResult> RedisCache()
        {
            ViewBag.Message = "A simple example with Azure Cache for Redis on ASP.NET Core.";

            // Perform cache operations using the cache object...

            // Simple PING command
            ViewBag.command1 = "PING";
            ViewBag.command1Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.ExecuteAsync(ViewBag.command1))).ToString();

            // Simple get and put of integral data types into the cache
            ViewBag.command2 = "GET Message";
            ViewBag.command2Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync("Message"))).ToString();

            ViewBag.command3 = "SET Message \"Hello! The cache is working from ASP.NET Core!\"";
            ViewBag.command3Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync("Message", "Hello! The cache is working from ASP.NET Core!"))).ToString();

            ViewBag.command4 = "GET Message";
            ViewBag.command4Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync("Message"))).ToString();

            return View();
        }
    }
}