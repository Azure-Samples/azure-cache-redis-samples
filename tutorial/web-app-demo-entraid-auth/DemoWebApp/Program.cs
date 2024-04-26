using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();

const string redisHostName = "yourcachename-redis.redis.cache.windows.net";
const string msiPrincipalID = "someguid";

var configOptions = await ConfigurationOptions.Parse($"{redisHostName}:6380").ConfigureForAzureWithSystemAssignedManagedIdentityAsync(msiPrincipalID);

var redisConnection = await ConnectionMultiplexer.ConnectAsync(configOptions);
var redisDB = redisConnection.GetDatabase();

builder.Services.AddSingleton(redisDB);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
