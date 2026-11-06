using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WatchStore.Web.Entities;

namespace WatchStore.Web.Data;

// Tạo tài khoản admin đầu tiên nếu DB chưa có admin nào.
// Chạy một lần lúc app khởi động.
public static class DbSeeder
{
    public static async Task SeedAdminAsync(AppDbContext context)
    {
        // Đã có admin rồi thì không làm gì. Tránh tạo trùng mỗi lần chạy app.
        if (await context.Users.AnyAsync(u => u.Role == "Admin"))
            return;

        var admin = new User
        {
            Username = "admin",
            Role = "Admin",
            IsActive = true
        };

        // Dùng chung thuật toán hash với AuthService để đăng nhập khớp.
        admin.PasswordHash = new PasswordHasher<User>()
            .HashPassword(admin, "Admin@123");

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}