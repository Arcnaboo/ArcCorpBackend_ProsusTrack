using ArcCorpBackend.Core.Messages;
using ArcCorpBackend.Core.Users;
using Microsoft.EntityFrameworkCore;



namespace ArcCorpBackend.Domain
{
    public class ArcDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserData> UserDatas { get; set; }   
        

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=arcapp.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<Chat>().HasKey(c => c.ChatId);
            modelBuilder.Entity<Message>().HasKey(m => m.MessageId);
            modelBuilder.Entity<UserData>().HasKey(y => y.UserDataId);
            
        }
    }
}
