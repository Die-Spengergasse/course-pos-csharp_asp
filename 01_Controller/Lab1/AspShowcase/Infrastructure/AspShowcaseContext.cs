using Bogus;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace AspShowcase.Infrastructure
{
    public class AspShowcaseContext : DbContext
    {
        public AspShowcaseContext(DbContextOptions opt) : base(opt) { }
        public DbSet<Models.Person> Persons => Set<Models.Person>();

        public void Seed()
        {
            Randomizer.Seed = new Random(1804);
            var persons = new Faker<Models.Person>("de").CustomInstantiator(f =>
            {
                // In 50% der Fälle soll das Geburtsdatum NULL sein.
                var dateOfBirth = f.Date.Between(new DateTime(1995, 1, 1), new DateTime(2005, 1, 1)).Date.OrNull(f, 0.5f);
                return new Models.Person(firstname: f.Name.FirstName(),
                    lastname: f.Name.LastName(), dateOfBirth: dateOfBirth);
            })
            .Generate(20)
            .ToList();
            Persons.AddRange(persons);
            SaveChanges();

        }
    }
}
