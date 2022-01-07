using System.Threading.Tasks;
using System.Web.Mvc;

namespace ContosoTeamStats.Controllers
{
    public class HomeController : Controller
    {
        private readonly RedisConnection _redisConnection;

        public HomeController()
        {
            _redisConnection = new RedisConnection();
        }

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
            ViewBag.Message = "A simple example with Azure Cache for Redis on ASP.NET.";

            // Perform cache operations using the cache object...

            // Simple PING command
            ViewBag.command1 = "PING";
            ViewBag.command1Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.ExecuteAsync("PING"))).ToString();

            // Simple get and put of integral data types into the cache
            ViewBag.command2 = "GET Message";
            ViewBag.command2Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync("Message"))).ToString();

            ViewBag.command3 = "SET Message \"Hello! The cache is working from ASP.NET!\"";
            ViewBag.command3Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync("Message", "Hello! The cache is working from ASP.NET!"))).ToString();

            ViewBag.command4 = "GET Message";
            ViewBag.command4Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync("Message"))).ToString();

            return View();
        }
    }
}