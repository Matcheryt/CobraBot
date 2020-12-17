using System;
using System.Threading;
using System.Threading.Tasks;
using CobraBot.Database.Models;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace CobraBot.Database
{
    public class BotContext : DbContext
    {
        public DbSet<Guild> Guilds { get; set; }

        private readonly IServiceProvider _services;
        
        public BotContext(IServiceProvider services)
        {
            _services = services;
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=CobraDB.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Guild>().ToTable("Guilds");
        }

        public async Task SaveChangesAndExpireAsync(string tag)
        {
            QueryCacheManager.ExpireTag(tag);
            await SaveChangesAsync();
        }

        public void SaveChangesAndExpire(string tag)
        {
            QueryCacheManager.ExpireTag(tag);
            SaveChanges();
        }

        public async Task <Guild> GetGuildSettings(ulong guildId)
        {
            var guild = await Guilds
                .FromSqlInterpolated($"Select * from Guilds where GuildId = {guildId}")
                .FirstOrDefaultAsync();

            if (guild is not null) return guild;
            
            var addedGuild = await Guilds.AddAsync(new Guild{GuildId = guildId});
            await SaveChangesAsync();
            return addedGuild.Entity;
        }
    }
}