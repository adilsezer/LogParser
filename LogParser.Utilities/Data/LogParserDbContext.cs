using LogParser.Utilities.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());
            });
        }
    }
}
