using Microsoft.AspNetCore.Mvc;
using WatchStore.Web.Models;
using WatchStore.Web.Services;

namespace WatchStore.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    // Hiện form đăng ký rỗng.
    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        // Binding và validation đã chạy xong trước khi vào đây.
        if (!ModelState.IsValid)
            return View(vm);

        // Service trả null nếu thành công, hoặc câu báo lỗi.
        var error = await _authService.RegisterAsync(vm.Username, vm.Password);
        if (error is not null)
        {
            // Chuỗi rỗng = lỗi của cả form, không thuộc ô nào.
            ModelState.AddModelError(string.Empty, error);
            return View(vm);
        }

        return RedirectToAction(nameof(Login));
    }

    // returnUrl: trang người dùng định vào trước khi bị đẩy về đây.
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(vm);

        // [SỬA - chuyển sang ASP.NET Core Identity]
        // Đã XÓA toàn bộ đoạn dựng claim thủ công:
        //   new Claim(ClaimTypes.NameIdentifier, user.Id...) / Name / Role
        //   new ClaimsIdentity(...) + HttpContext.SignInAsync(...)
        // Lý do: SignInManager tự sinh đúng 3 claim đó từ user và role trong DB.
        // Nhờ vậy TasksController, UsersController và _Layout.cshtml không phải sửa gì.
        var error = await _authService.LoginAsync(vm.Username, vm.Password);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(vm);
        }

        // Chỉ nhận returnUrl nội bộ, chặn chuyển hướng sang web ngoài.
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // [SỬA] HttpContext.SignOutAsync() -> _authService.LogoutAsync()
        // vì Identity dùng 3 cookie scheme, phải xóa cả ba.
        await _authService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }
}
