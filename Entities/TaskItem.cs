using System.ComponentModel.DataAnnotations;

namespace WatchStore.Web.Entities;

public class TaskItem
{
    public int Id { get; set; }
    [Required, MaxLength(200)]
    public string Title {  get; set; } = string.Empty;

    // Dấu ? = cho phép NULL trong DB. Mô tả không bắt buộc.
    [MaxLength(1000)]
    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }

    // null! = EF Core sẽ gán khi load dữ liệu, tránh cảnh báo CS8618.
    public User User { get; set; } = null!;

}
