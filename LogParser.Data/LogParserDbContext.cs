using Microsoft.EntityFrameworkCore;

namespace LogParser.Data
{
    public class LogParserDbContext : DbContext
    {
        public DbSet<CsvRecord> CsvRecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("InMemoryDb");
        }
    }
}