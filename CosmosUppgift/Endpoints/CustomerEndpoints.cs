using CosmosUppgift.Entities;
using System.Collections.Concurrent;
using System.Net;
using Microsoft.Azure.Cosmos;
using CosmosUppgift.DTOs;
using System.Text;
using CosmosUppgift.Services;

namespace CosmosUppgift.Endpoints
{
    // hej
    public static class CustomerEndpoints
    {
        public static void MapCustomerEndpoints(this WebApplication app)
        {
            var allContainers = app.Services.GetServices<Container>().ToList();
            Container customers = allContainers.First(c => c.Id == "Customers");
            Container sales = allContainers.First(c => c.Id == "Salespersons");

            app.MapPost("/customers", async (CustomerDTO input) =>
            {
                string result = await CustomerService.PostCustomer(input, sales, customers);
                return Results.Ok(result);
            });

            app.MapPut("/customers/{id}", async (string id, Customer updated) =>
            {
                await CustomerService.UpdateCustomer(id, updated, customers);
                return Results.Ok();
            });

            app.MapDelete("/customers/{id}", async (string id) =>
            {
                await CustomerService.DeleteCustomer(id, customers);
                return Results.Ok();
            });

            app.MapGet("/customers/search", async (string? name, string? salespersonName) =>
            {
                List<Customer> results = await CustomerService.GetCustomersByNameAndSalesperson(name, salespersonName, customers, sales);
                return Results.Ok(results);
            });

            app.MapGet("/salespersons/list", async () =>
            {
                List<Salesperson> results = await CustomerService.GetSalespersons(sales);
                return Results.Ok(results);
            });
        }
    }
}
