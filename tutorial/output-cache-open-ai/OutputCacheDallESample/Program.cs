using Microsoft.AspNetCore.OutputCaching;
using OutputCacheDallESample;
using Redis.OM;
using Redis.OM.Vectorizers;

var builder = WebApplication.CreateBuilder(args);

// add services
builder.Services.AddStackExchangeRedisOutputCache(options => {
    options.Configuration = builder.Configuration["RedisCacheConnection"];
});
builder.Services.AddOutputCache(options => {
    // optional: named output-cache profiles
});

var app = builder.Build();

app.MapGet("/", async (HttpContext context) => { await context.Response.WriteAsync("<h1>Welcome to OpenAI Art Gallery</h1>"); });

app.MapGet("/nocache/{prompt}", async (HttpContext context, string prompt, IConfiguration config) => 
    { 
        await GenerateImageSDK.GenerateImageSDKAsync(context, prompt, config); 
    });

app.MapGet("/semanticcache/{prompt}", async (HttpContext context, string prompt, IConfiguration config) => 
    { 
        await GenerateImageSC.GenerateImageSCAsync(context, prompt, config);
    });

app.MapGet("/cached/{prompt}", async (HttpContext context, string prompt, IConfiguration config) => 
    { await GenerateImageSDK.GenerateImageSDKAsync(context, prompt, config); 
    }).CacheOutput();

app.MapGet("/cachedByAnnotation/{prompt}", [OutputCache(Duration = 15)] async (HttpContext context, string prompt, IConfiguration config) => 
    { await GenerateImageSDK.GenerateImageSDKAsync(context, prompt, config); 
    });

app.MapGet("/cached/Gardens/{prompt}", async (HttpContext context, string prompt, IConfiguration config) => 
    { await GenerateImageSDK.GenerateImageSDKAsync(context, prompt, config); 
    }).CacheOutput(x => x.Tag("Gardens"));

app.MapPost("/purge/{tag}", async (IOutputCacheStore cache, string tag) =>
    {await cache.EvictByTagAsync(tag, default);
    });

app.UseOutputCache();
app.Run();
