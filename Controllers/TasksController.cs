using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchStore.Web.Entities;
using WatchStore.Web.Models;
using WatchStore.Web.Repositories;

namespace WatchStore.Web.Controllers;

// [Authorize] đặt ở cấp class: MỌI action bên trong đều yêu cầu đăng nhập.
// Chưa đăng nhập sẽ bị đẩy về LoginPath đã khai trong Program.cs.
[Authorize]
public class TasksController : Controller
{
    private readonly ITaskRepository _taskRepository;

    public TasksController(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    // Lấy id người đang đăng nhập từ cookie.
    // Dùng ở mọi action, nên tách ra một chỗ cho khỏi lặp.
    private int CurrentUserId
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index(TaskItemStatus? status, string? keyword)
    {
        var userId = CurrentUserId;

        var items = await _taskRepository.GetByUserAsync(userId, status, keyword);
        var counts = await _taskRepository.GetStatusCountsAsync(userId);

        var vm = new TaskListViewModel
        {
            Items = items,
            FilterStatus = status,
            Keyword = keyword,
            // GetValueOrDefault: trạng thái chưa có task nào thì đếm là 0.
            ToDoCount = counts.GetValueOrDefault(TaskItemStatus.ToDo),
            InProgressCount = counts.GetValueOrDefault(TaskItemStatus.InProgress),
            DoneCount = counts.GetValueOrDefault(TaskItemStatus.Done)
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult Create() => View(new TaskFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var task = new TaskItem
        {
            Title = vm.Title,
            Description = vm.Description,
            Status = vm.Status,
            DueDate = vm.DueDate,
            // Chủ sở hữu lấy từ cookie, KHÔNG lấy từ form.
            UserId = CurrentUserId
        };

        await _taskRepository.AddAsync(task);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        // Query có cả Id VÀ UserId. Task của người khác trả về null.
        var task = await _taskRepository.GetOwnedAsync(id, CurrentUserId);
        if (task is null)
            return NotFound();

        return View(ToFormViewModel(task));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TaskFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        // Nạp lại từ DB thay vì tin Id trong form.
        var task = await _taskRepository.GetOwnedAsync(vm.Id, CurrentUserId);
        if (task is null)
            return NotFound();

        task.Title = vm.Title;
        task.Description = vm.Description;
        task.Status = vm.Status;
        task.DueDate = vm.DueDate;

        await _taskRepository.UpdateAsync(task);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _taskRepository.GetOwnedAsync(id, CurrentUserId);
        if (task is null)
            return NotFound();

        return View(task);
    }

    // Tên action khác tên hàm vì C# không cho 2 hàm cùng tên,
    // cùng tham số. ActionName đưa nó về đúng URL /Tasks/Delete.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var task = await _taskRepository.GetOwnedAsync(id, CurrentUserId);
        if (task is null)
            return NotFound();

        await _taskRepository.DeleteAsync(task);
        return RedirectToAction(nameof(Index));
    }

    private static TaskFormViewModel ToFormViewModel(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        DueDate = task.DueDate
    };
}