using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace WeatherAPI.Controllers
{
    [ApiController]
    [Route("[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDistributedCache _cache;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        [Route("/")]
        public async IAsyncEnumerable<WeatherForecast> Index(CancellationToken abort)
        {
            var WeatherForecastNextWeek = await _cache.GetAsync("OneWeekFromNow", () =>
            {
                return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();
            }, CacheOptions.ThirtyMinutes, abort);

            foreach (WeatherForecast _item in WeatherForecastNextWeek)
            { 
                yield return _item;
            }
        }
        [HttpGet(Name = "GetMoreWeatherForecast")]
        [Route("{numWeeks}/{numMonths}")]
        public async IAsyncEnumerable<WeatherForecast> GetFuture(int numWeeks, int numMonths, CancellationToken abort)
        {
            var WeatherForecastNextWeek = await _cache.GetAsync($"{numWeeks}:{numMonths}-fromToday", (NumWeeks: numWeeks, NumMonths: numMonths),
            static state =>
            {
                return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index + state.NumWeeks*7.0 + state.NumMonths*30.0)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();
            }, CacheOptions.ThirtyMinutes, abort);

            foreach (WeatherForecast _item in WeatherForecastNextWeek)
            {
                yield return _item;
            }
        }

    }

    static class CacheOptions
    {
        public static DistributedCacheEntryOptions ThirtyMinutes { get; } = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) };
    }
}