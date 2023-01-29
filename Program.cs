using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.frosti.json");
}

var client = new SecretClient(new Uri(builder.Configuration["KV_ENDPOINT"]), new DefaultAzureCredential());

// New instance of CosmosClient class
var cosmosConnection = client.GetSecret("CosmosConnection").Value.Value;
builder.Services.AddSingleton(s =>
{
    return new CosmosClient(cosmosConnection);
});

// Add services to the container.
builder.Services.AddControllersWithViews();

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

