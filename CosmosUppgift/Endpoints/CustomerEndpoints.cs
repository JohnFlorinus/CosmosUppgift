using CosmosUppgift.Entities;
using System.Collections.Concurrent;
using System.Net;
using Microsoft.Azure.Cosmos;
using CosmosUppgift.DTOs;
using System.Text;

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
                var query = new QueryDefinition("SELECT * FROM s WHERE LOWER(s.name) = @name")
                    .WithParameter("@name", input.ResponsibleName.ToLower());

                var iterator = sales.GetItemQueryIterator<Salesperson>(query);
                Salesperson? responsible = null;

                while (iterator.HasMoreResults && responsible == null)
                {
                    var page = await iterator.ReadNextAsync();
                    responsible = page.FirstOrDefault();
                }

                if (responsible == null)
                    return Results.BadRequest("Salesperson not found");

                var customer = new Customer
                {
                    id = Guid.NewGuid().ToString(),
                    Name = input.Name,
                    Title = input.Title,
                    Phone = input.Phone,
                    Email = input.Email,
                    Address = input.Address,
                    Responsible = responsible
                };

                await customers.CreateItemAsync(customer, new PartitionKey(customer.id));
                return Results.Created($"/customers/{customer.id}", customer);
            });

            app.MapPut("/customers/{id}", async (string id, Customer updated) =>
            {
                updated.id = id;
                await customers.UpsertItemAsync(updated, new PartitionKey(id));
                return Results.Ok(updated);
            });

            app.MapDelete("/customers/{id}", async (string id) =>
            {
                await customers.DeleteItemAsync<Customer>(id, new PartitionKey(id));
                return Results.NoContent();
            });

            app.MapGet("/customers/search", async (string? name, string? salespersonName) =>
            {
                var hasName = !string.IsNullOrWhiteSpace(name);
                var hasSales = !string.IsNullOrWhiteSpace(salespersonName);

                // 1) Build the SQL text with the right conditionals
                var sql = new StringBuilder("SELECT * FROM c WHERE 1=1");
                if (hasName)
                    sql.Append(" AND CONTAINS(c.name, @name)");
                if (hasSales)
                    sql.Append(" AND CONTAINS(c.responsible.name, @salesName)");

                // 2) Create the QueryDefinition only once, with the final SQL
                var query = new QueryDefinition(sql.ToString());

                // 3) Add parameters only if needed
                if (hasName)
                    query.WithParameter("@name", name!);
                if (hasSales)
                    query.WithParameter("@salesName", salespersonName!);

                // 4) Execute
                var iterator = customers.GetItemQueryIterator<Customer>(query);
                var results = new List<Customer>();
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return Results.Ok(results);
            });

            app.MapGet("/salespersons/list", async () =>
            {
                var query = new QueryDefinition("SELECT * FROM s");
                var iterator = sales.GetItemQueryIterator<Salesperson>(query);
                var results = new List<Salesperson>();
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return Results.Ok(results);
            });
        }
    }
}
