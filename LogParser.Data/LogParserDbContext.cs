using Microsoft.EntityFrameworkCore;

namespace LogParser.Data
{
    public class LogParserDbContext : DbContext
    {
        public LogParserDbContext(DbContextOptions<LogParserDbContext> options)
            : base(options)
        {
        }

        public DbSet<CsvRecord> CsvRecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase("InMemoryDb");
            }
        }
    }
}
