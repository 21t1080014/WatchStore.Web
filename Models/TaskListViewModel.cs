using WatchStore.Web.Entities;

namespace WatchStore.Web.Models;

// Dữ liệu cho trang danh sách: các task, điều kiện lọc đang áp dụng,
// và số đếm theo từng trạng thái.
public class TaskListViewModel
{
    public IEnumerable<TaskItem> Items { get; set; } = new List<TaskItem>();

    // Giữ lại lựa chọn lọc để form không bị reset sau khi submit.
    public TaskItemStatus? FilterStatus { get; set; }
    public string? Keyword { get; set; }

    public int ToDoCount { get; set; }
    public int InProgressCount { get; set; }
    public int DoneCount { get; set; }

    // Tiện cho View: có task nào không, để quyết định hiện bảng
    // hay hiện lời mời tạo task đầu tiên.
    public bool HasAnyTask => ToDoCount + InProgressCount + DoneCount > 0;
}