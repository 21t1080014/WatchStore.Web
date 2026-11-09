using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchStore.Web.Models;
using WatchStore.Web.Services;

namespace WatchStore.Web.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    private int CurrentUserId
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentId = CurrentUserId;
        var rows = await _userService.GetAllAsync();

        // [SỬA - chuyển sang ASP.NET Core Identity]
        //   r.User.Username -> r.User.UserName  (property của IdentityUser, kiểu string?)
        //   r.User.Role     -> r.Role           (role lấy từ join trong repository,
        //                                        không còn là cột trên bảng Users)
        var vm = rows.Select(r => new UserListItemViewModel
        {
            Id = r.User.Id,
            Username = r.User.UserName ?? string.Empty,
            Role = r.Role,
            IsActive = r.User.IsActive,
            CreatedAt = r.User.CreatedAt,
            TaskCount = r.TaskCount,
            IsCurrentUser = r.User.Id == currentId
        }).ToList();

        return View(vm);
    }
    //chuyển đổi role

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(int id)
    {
        var error = await _userService.ChangeRoleAsync(id, CurrentUserId);
        if (error is not null)
            TempData["Error"] = error;

        return RedirectToAction(nameof(Index));
    }
    //khóa tài khoản người dùng
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var error = await _userService.ToggleActiveAsync(id, CurrentUserId);
        if (error is not null)
            TempData["Error"] = error;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var error = await _userService.DeleteAsync(id, CurrentUserId);
        if (error is not null)
            TempData["Error"] = error;

        return RedirectToAction(nameof(Index));
    }
}