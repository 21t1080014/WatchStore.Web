using Microsoft.EntityFrameworkCore;
using WatchStore.Web.Entities;

namespace WatchStore.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Không cho 2 user trùng Username. Ràng buộc này nằm ở tầng DB
        // nên kể cả 2 request đăng ký cùng lúc cũng không lọt được.
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // Quan hệ 1-N: xóa User thì xóa luôn Task của họ.
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}