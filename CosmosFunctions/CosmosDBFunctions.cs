using System;
using System.Collections.Generic;
using System.Net.Mail;
using CosmosUppgift.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CosmosFunctions;

public class Function1
{
    private readonly ILogger<Function1> _logger;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }

    [Function("CustomerFunction")]
    public async void Run([CosmosDBTrigger(
        databaseName: "SalesDB",
        containerName: "Customers",
        Connection = "CosmosDB_Connection",
        LeaseContainerName = "leases",
        CreateLeaseContainerIfNotExists = true)] IReadOnlyList<Customer> input)
    {

        if (input != null && input.Count > 0)
        {
            _logger.LogInformation(@$"En ny kund har lagts till eller uppdaterats: {input[0].Name}
            Skickar mail till ansvarig säljare {input[0].Responsible.Name}");
            await EmailHelper.SendEmailToResponsible(input[0]);
        }
    }
}