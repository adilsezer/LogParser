using LogParser.Utilities.Models;
using LogParser.Utilities.Services;
using Microsoft.EntityFrameworkCore;

namespace LogParser.Utilities.Data
{
    public class LogParserDbContext : DbContext
    {
        public DbSet<CsvLog> CsvLogs { get; set; }

        public LogParserDbContext(DbContextOptions<LogParserDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CsvLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Fields)
                      .HasConversion(
                          v => JsonUtility.Serialize(v),
                          v => JsonUtility.Deserialize<Dictionary<string, object>>(v) ?? new Dictionary<string, object>());
            });
        }
    }
}
