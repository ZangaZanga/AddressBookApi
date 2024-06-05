using Microsoft.EntityFrameworkCore;
using AddressBookApi.Models;

namespace AddressBookApi.Data
{
    public class AddressBookContext : DbContext
    {
        public AddressBookContext(DbContextOptions<AddressBookContext> options) : base(options) { }

        public DbSet<Person> Personen { get; set; }
        public DbSet<Adresse> Adressen { get; set; }
    }
    
}