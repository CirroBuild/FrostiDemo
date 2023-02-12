using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FrostiDemo.Models;
using Microsoft.Azure.Cosmos;
using System.Net;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using System.Security.Cryptography.X509Certificates;
using Azure.Storage.Files.DataLake;
using System.Data;
using System.Data.SqlClient;

namespace FrostiDemo.Controllers;


public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly CosmosClient _cosmosClient;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, CosmosClient cosmosClient, IConfiguration configuration)
    {
        _logger = logger;
        _cosmosClient = cosmosClient;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<HttpStatusCode> Cosmos()
    {
        var dbResp = await _cosmosClient.CreateDatabaseIfNotExistsAsync("cosmicworks");
        var containerResp = await dbResp.Database.CreateContainerIfNotExistsAsync("products", "/category");
        return containerResp.StatusCode;
    }

    public async Task<HttpStatusCode> Storage()
    {
        //SHOULD MENTION AZURE STORAGE NAMING REQS IN DOCS. lowercase, dashes (no double dahes, and cant start or end w/ dash), numbers and letters only. table name cant have dashes.

        //Blobs -- https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-container-create
        var blobClient = new BlobServiceClient(_configuration["StorageConnection"]);
        var containerClient = blobClient.GetBlobContainerClient("your-blob-name");
        var response = await containerClient.CreateIfNotExistsAsync();

        //Tables -- https://learn.microsoft.com/en-us/dotnet/api/overview/azure/data.tables-readme?view=azure-dotnet
        var tableClient = new TableServiceClient(_configuration["StorageConnection"]);
        var table = tableClient.CreateTableIfNotExists("yourtablename");

        //Queues -- https://learn.microsoft.com/en-us/azure/storage/queues/storage-dotnet-how-to-use-queues?tabs=dotnet
        var queueClient = new QueueClient(_configuration["StorageConnection"], "your-queue-name");
        await queueClient.CreateIfNotExistsAsync();

        //File Shares -- https://learn.microsoft.com/en-us/azure/storage/files/storage-dotnet-how-to-use-files?tabs=dotnet
        var share = new ShareClient(_configuration["StorageConnection"], "your-share-name");
        await share.CreateIfNotExistsAsync();

        //Data Lake -- https://learn.microsoft.com/en-us/azure/storage/blobs/data-lake-storage-directory-file-acl-dotnet
        var dataLakeClient = new DataLakeServiceClient(_configuration["StorageConnection"]);
        await dataLakeClient.CreateFileSystemAsync("your-file-system");

        return HttpStatusCode.OK;
    }

    public HttpStatusCode ApplicationInsights()
    {
        _logger.LogError("Testing AI");
        return HttpStatusCode.OK;
    }

    public HttpStatusCode Sql()
    {
        using var myConn = new SqlConnection(_configuration["SQLConnection"]);
        var str = "CREATE DATABASE TestDB ( EDITION = 'Basic' )";    //See https://learn.microsoft.com/en-us/sql/t-sql/statements/create-database-transact-sql?view=sql-server-ver15&preserve-view=true&tabs=sqlpool

        myConn.Open();
        var myCommand = new SqlCommand(str, myConn);
        var result = myCommand.BeginExecuteNonQuery();

        //creating the database does take a second. This is just a quick sample
        while (!result.IsCompleted)
        {
            System.Threading.Thread.Sleep(1000);
        }

        myCommand.EndExecuteNonQuery(result);
        myConn.Close();

        //https://github.com/Azure-Samples/msdocs-app-service-sqldb-dotnetcore <- SEE for Entity Framework example
        return HttpStatusCode.OK;
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
