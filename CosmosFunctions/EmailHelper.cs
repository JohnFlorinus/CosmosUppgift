using CosmosUppgift.Entities;
using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosFunctions
{
    public static class EmailHelper
    {
        public static async Task SendEmailToResponsible(Customer customer)
        {
            var recipientName = customer.Responsible.Name;
            var recipientEmail = customer.Responsible.Email;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("CosmosDB DEMO", "johnflorinus@gmail.com"));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            message.Subject = "En ny kund har lagts till eller uppdaterats!";

            message.Body = new TextPart("plain")
            {
                Text = $"Hej {recipientName},\n\nEn ny kund har lagts till eller uppdaterats:\n\n" +
                       $"- Namn: {customer.Name}\n" +
                       $"- Titel: {customer.Title}\n" +
                       $"- Telefonnummer: {customer.Phone}\n" +
                       $"- E-post: {customer.Email}\n" +
                       $"- Adress: {customer.Address}\n\n" +
                       $"Vänliga hälsningar,\nAzure Functions"
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("johnflorinus@gmail.com", Environment.GetEnvironmentVariable("GMAIL_PASSWORD"));
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
