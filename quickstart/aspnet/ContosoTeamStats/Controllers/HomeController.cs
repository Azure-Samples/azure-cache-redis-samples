using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ContosoTeamStats.Controllers
{
    public class HomeController : Controller
    {
        // In a real-world application you may want to dependency-inject this connection.
        private static Task<RedisConnection> _redisConnectionFactory = RedisConnection.InitializeAsync(connectionString: ConfigurationManager.AppSettings["CacheConnection"].ToString());
        private RedisConnection _redisConnection;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public async Task<ActionResult> RedisCache()
        {
            _redisConnection = await _redisConnectionFactory;
            ViewBag.Message = "A simple example with Azure Cache for Redis on ASP.NET.";

            // Perform cache operations using the cache object...

            // Simple PING command
            ViewBag.command1 = "PING";
            ViewBag.command1Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.ExecuteAsync("PING"))).ToString();

            string key = "Message";
            string value = "Hello! The cache is working from ASP.NET!";

            ViewBag.command2 = $"SET {key} \"{value}\"";
            ViewBag.command2Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(key, value))).ToString();

            ViewBag.command3 = $"GET {key}";
            ViewBag.command3Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(key))).ToString();

            key = "LastUpdateTime";
            value = DateTime.UtcNow.ToString();

            ViewBag.command4 = $"GET {key}";
            ViewBag.command4Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(key))).ToString();

            ViewBag.command5 = $"SET {key}";
            ViewBag.command5Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(key, value))).ToString();


            return View();
        }
    }
}