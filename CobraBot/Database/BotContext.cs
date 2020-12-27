using System.Linq;
using System.Threading.Tasks;
using CobraBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace CobraBot.Database
{
    public class BotContext : DbContext
    {
        public DbSet<Guild> Guilds { get; set; }

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
            await SaveChangesAsync();
            QueryCacheManager.ExpireTag(tag);
        }

        public void SaveChangesAndExpire(string tag)
        {
            SaveChanges();
            QueryCacheManager.ExpireTag(tag);
        }

        /// <summary>Returns guild settings for specified guildId. If specified guild doesn't have a database entry, then creates one.
        /// </summary>
        public async Task <Guild> GetGuildSettings(ulong guildId)
        {
            var guild = Guilds.FirstOrDefault(x => x.GuildId == guildId);

            if (guild is not null) return guild;
            
            var addedGuild = await Guilds.AddAsync(new Guild{GuildId = guildId});
            await SaveChangesAndExpireAsync(guildId.ToString());
            return addedGuild.Entity;
        }

        /// <summary>Gets guild prefix for specified guildId. If guild doesn't have a custom prefix, returns the default prefix: '-'
        /// </summary>
        public string GetGuildPrefix(ulong guildId)
        {
            return Guilds.AsNoTracking().Where(x => x.GuildId == guildId)
                .FromCache(guildId.ToString()).FirstOrDefault()?.CustomPrefix ?? "-";
        }
    }
}