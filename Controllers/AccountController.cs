using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        var user = await _authService.ValidateLoginAsync(vm.Username, vm.Password);
        if (user is null)
        {
            // Báo chung một câu, không nói rõ sai tên hay sai mật khẩu,
            // để kẻ tấn công không dò được username nào có thật.
            ModelState.AddModelError(string.Empty,
                "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View(vm);
        }

        // Đóng gói danh tính để ghi vào cookie.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(new ClaimsPrincipal(identity));

        // Chỉ nhận returnUrl nội bộ, chặn chuyển hướng sang web ngoài.
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}