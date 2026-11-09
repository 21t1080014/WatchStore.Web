using Microsoft.EntityFrameworkCore;
using WatchStore.Web.Data;
using WatchStore.Web.Entities;

namespace WatchStore.Web.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _appDbContext;

    public UserRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<IEnumerable<(User User, string Role, int TaskCount)>> GetAllWithTaskCountAsync()
    {
        // Select trong cùng một query để tránh N+1:
        // một câu SQL duy nhất, không phải mỗi user một lần đếm.
        //
        // [SỬA] Thêm phần lấy Role. UserRoles và Roles là DbSet của IdentityDbContext.
        // Một user về lý thuyết có nhiều role, nhưng app này chỉ gán 1 nên lấy FirstOrDefault.
        var rows = await _appDbContext.Users
            .OrderBy(u => u.UserName)
            .Select(u => new
            {
                User = u,
                TaskCount = u.Tasks.Count,
                Role = _appDbContext.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Join(_appDbContext.Roles,
                          ur => ur.RoleId,
                          r => r.Id,
                          (ur, r) => r.Name)
                    .FirstOrDefault()
            })
            .ToListAsync();

        // Role null = user chưa được gán role nào. Hiển thị như "User" cho an toàn.
        return rows.Select(r => (r.User, r.Role ?? "User", r.TaskCount));
    }

    // [SỬA] Trước: đếm theo cột u.Role == "Admin".
    // Sau:  join UserRoles với Roles vì cột đó không còn.
    public async Task<int> CountAdminsAsync()
        => await _appDbContext.UserRoles
            .Join(_appDbContext.Roles,
                  ur => ur.RoleId,
                  r => r.Id,
                  (ur, r) => r.Name)
            .CountAsync(name => name == "Admin");
}
