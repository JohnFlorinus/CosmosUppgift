using CosmosUppgift.DTOs;
using CosmosUppgift.Entities;
using Microsoft.Azure.Cosmos;
using System.Text;

namespace CosmosUppgift.Services
{
    public static class CustomerService
    {
        public static async Task<string> PostCustomer(CustomerDTO input, Container sales, Container customers) {
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
                return "Salesperson not found";

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
                return "Created Customer " + customer.Name;
            }

        public static async Task UpdateCustomer(string id, Customer updated, Container customers)
        {
            updated.id = id;
            await customers.UpsertItemAsync(updated, new PartitionKey(id));
        }

        public static async Task DeleteCustomer(string id, Container customers)
        {
            await customers.DeleteItemAsync<Customer>(id, new PartitionKey(id));
        }

        public static async Task<List<Customer>> GetCustomersByNameAndSalesperson(string? name, string? salespersonName, Container customers, Container sales)
        {
            var queryText = "SELECT * FROM c WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(name))
                queryText += $" AND CONTAINS(c.Name, '{name}')";

            if (!string.IsNullOrWhiteSpace(salespersonName))
                queryText += $" AND CONTAINS(c.Responsible.Name, '{salespersonName}')";

            var query = customers.GetItemQueryIterator<Customer>(queryText);
            var results = new List<Customer>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }

        public static async Task<List<Salesperson>> GetSalespersons(Container sales)
        {
            var query = new QueryDefinition("SELECT * FROM s");
            var iterator = sales.GetItemQueryIterator<Salesperson>(query);
            var results = new List<Salesperson>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }
            return results;
        }
    }
}
