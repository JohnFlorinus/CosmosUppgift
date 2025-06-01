namespace CosmosUppgift.Entities
{
    public class Customer
    {
        public string id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public Salesperson Responsible { get; set; }
    }
}
