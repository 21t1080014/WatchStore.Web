using Microsoft.EntityFrameworkCore;
using WatchStore.Web.Data;
using WatchStore.Web.Entities;

namespace WatchStore.Web.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItem>> GetByUserAsync(
        int userId, TaskItemStatus? status, string? keyword)
    {
        // Điều kiện UserId đặt ngay từ đầu, không bao giờ bỏ qua.
        var query = _context.Tasks
            .Where(t => t.UserId == userId);

        // Chỉ thêm điều kiện khi người dùng thực sự chọn lọc.
        if (status is not null)
            query = query.Where(t => t.Status == status);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(t => t.Title.Contains(keyword));

        // ToListAsync là lúc câu SQL thật sự chạy.
        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    // Hai điều kiện Id VÀ UserId cùng nằm trong một câu query.
    // Task của người khác sẽ trả về null, không có đường nào lọt.
    public async Task<TaskItem?> GetOwnedAsync(int taskId, int userId)
        => await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

    public async Task<Dictionary<TaskItemStatus, int>> GetStatusCountsAsync(int userId)
        => await _context.Tasks
            .Where(t => t.UserId == userId)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

    public async Task AddAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }
}