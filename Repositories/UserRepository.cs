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

    public async Task AddAsync(User user)
    {
        _appDbContext.Users.Add(user);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
        => await _appDbContext.Users.FindAsync(id);

    public async Task<User?> GetByUsernameAsync(string username)
        => await _appDbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username);

    public async Task<bool> UsernameExistsAsync(string username)
        => await _appDbContext.Users
            .AnyAsync(u => u.Username == username);
    public async Task<IEnumerable<(User User, int TaskCount)>> GetAllWithTaskCountAsync()
    {
        // Select trong cùng một query để tránh N+1:
        // một câu SQL duy nhất, không phải mỗi user một lần đếm.
        var rows = await _appDbContext.Users
            .OrderBy(u => u.Username)
            .Select(u => new { User = u, TaskCount = u.Tasks.Count })
            .ToListAsync();

        return rows.Select(r => (r.User, r.TaskCount));
    }

    public async Task<int> CountAdminsAsync()
        => await _appDbContext.Users.CountAsync(u => u.Role == "Admin");

    public async Task UpdateAsync(User user)
    {
        _appDbContext.Users.Update(user);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _appDbContext.Users.Remove(user);
        await _appDbContext.SaveChangesAsync();
    }
}

