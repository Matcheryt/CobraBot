using CobraBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CobraBot.Database
{
    public class BotContext : DbContext
    {
        public BotContext(DbContextOptions<BotContext> options) : base(options) { }

        public DbSet<Guild> Guilds { get; set; }
        public DbSet<ModCase> ModCases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Guild>().ToTable("Guilds");
            modelBuilder.Entity<ModCase>().ToTable("ModCases");

            // SQLite does not support DateTimeOffset
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                                                .SelectMany(t => t.GetProperties())
                                                .Where(p => p.ClrType == typeof(DateTimeOffset)))
            {
                property.SetValueConverter(
                    new ValueConverter<DateTimeOffset, DateTime>(
                        convertToProviderExpression: dateTimeOffset => dateTimeOffset.UtcDateTime,
                        convertFromProviderExpression: dateTime => new DateTimeOffset(dateTime)
                    ));
            }

            foreach (var property in modelBuilder.Model.GetEntityTypes()
                                                .SelectMany(t => t.GetProperties())
                                                .Where(p => p.ClrType == typeof(DateTimeOffset?)))
            {
                property.SetValueConverter(
                    new ValueConverter<DateTimeOffset?, DateTime>(
                        convertToProviderExpression: dateTimeOffset => dateTimeOffset.Value.UtcDateTime,
                        convertFromProviderExpression: dateTime => new DateTimeOffset(dateTime)
                    ));
            }


            // SQLite does not support ulong
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(ulong)))
            {
                property.SetValueConverter(
                    new ValueConverter<ulong, long>(
                        convertToProviderExpression: ulongValue => (long)ulongValue,
                        convertFromProviderExpression: longValue => (ulong)longValue
                    ));
            }

            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(ulong?)))
            {
                property.SetValueConverter(
                    new ValueConverter<ulong?, long>(
                        convertToProviderExpression: ulongValue => (long)ulongValue.Value,
                        convertFromProviderExpression: longValue => (ulong)longValue
                    ));
            }
        }


        /// <summary>Returns guild settings for specified guildId. If specified guild doesn't have a database entry, then creates one.
        /// </summary>
        public async Task<Guild> GetGuildSettings(ulong guildId)
        {
            var guild = Guilds.FirstOrDefault(x => x.GuildId == guildId);

            if (guild is not null) return guild;

            var addedGuild = await Guilds.AddAsync(new Guild { GuildId = guildId });
            await SaveChangesAsync();
            return addedGuild.Entity;
        }

        /// <summary>Gets guild prefix for specified guildId. If guild doesn't have a custom prefix, returns the default prefix: '-'
        /// </summary>
        public string GetGuildPrefix(ulong guildId)
        {
            return Guilds.AsNoTracking().FirstOrDefault(x => x.GuildId == guildId)?.CustomPrefix ?? "-";
        }
    }

    public class BotContextFactory : IDesignTimeDbContextFactory<BotContext>
    {
        public BotContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<BotContext>()
                .UseSqlite("Data Source=CobraDB.db");

            return new BotContext(options.Options);
        }
    }
}