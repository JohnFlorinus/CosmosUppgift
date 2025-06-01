using Microsoft.Azure.Cosmos;
using CosmosUppgift.Endpoints;
using System.Xml.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string cosmosConn = builder.Configuration["CosmosConn"]!;
string databaseId = "SalesDB";
string customerContainerId = "Customers";


var cosmosClient = new CosmosClient(cosmosConn, new CosmosClientOptions
{
    ConnectionMode = ConnectionMode.Gateway
});

var database = cosmosClient.GetDatabase(databaseId);

var customersContainer = await database.CreateContainerIfNotExistsAsync("Customers", "/id");
var salespersonsContainer = await database.CreateContainerIfNotExistsAsync("Salespersons", "/id");

builder.Services.AddSingleton(customersContainer.Container);
builder.Services.AddSingleton(salespersonsContainer.Container);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapCustomerEndpoints();

app.Run();