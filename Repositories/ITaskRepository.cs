using WatchStore.Web.Entities;

namespace WatchStore.Web.Repositories;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetByUserAsync(
        int userId, TaskItemStatus? status, string? keyword);

    // Lấy task đúng người 
    Task<TaskItem?> GetOwnedAsync(int taskId, int userId);

    // Đếm số task theo từng trạng thái
    Task<Dictionary<TaskItemStatus, int>> GetStatusCountsAsync(int userId);

    Task AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(TaskItem task);
}