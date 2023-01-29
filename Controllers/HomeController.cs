using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FrostiDemo.Models;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace FrostiDemo.Controllers;


public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly CosmosClient _cosmosClient;

    public HomeController(ILogger<HomeController> logger, CosmosClient cosmosClient)
    {
        _logger = logger;
        _cosmosClient = cosmosClient;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<HttpStatusCode> InitializeDatabase()
    {
        var dbResp = await _cosmosClient.CreateDatabaseIfNotExistsAsync("cosmicworks");
        var containerResp = await dbResp.Database.CreateContainerIfNotExistsAsync("products", "/category");
        return containerResp.StatusCode;
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
}
