using Microsoft.AspNetCore.Identity;
using WatchStore.Web.Entities;

namespace WatchStore.Web.Services;

// [SỬA - chuyển sang ASP.NET Core Identity]
// Interface IAuthService giữ nguyên vai trò, chỉ thay ruột bên trong:
//   IUserRepository      -> UserManager<User>   (tạo user, hash, kiểm tra trùng tên)
//   PasswordHasher<User> -> UserManager tự lo
//   dựng claim thủ công  -> SignInManager<User> tự lo
public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthService(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<string?> RegisterAsync(string username, string password)
    {
        var user = new User
        {
            UserName = username,
            IsActive = true
        };

        // [SỬA] CreateAsync làm 3 việc trong một lần gọi:
        // kiểm tra username trùng, validate độ mạnh mật khẩu, hash rồi lưu.
        // Nên không cần UsernameExistsAsync và _hasher.HashPassword nữa.
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            // Identity trả danh sách lỗi (trùng tên, mật khẩu yếu...). Gộp thành một câu.
            return string.Join(" ", result.Errors.Select(e => e.Description));
        }

        // [SỬA] Role không còn là cột để gán. Phải ghi vào bảng UserRoles.
        // Vẫn gán cứng "User" — người đăng ký không tự chọn được quyền.
        await _userManager.AddToRoleAsync(user, "User");
        return null;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        // Báo chung một câu cho mọi trường hợp sai, không nói rõ sai tên hay sai
        // mật khẩu, để kẻ tấn công không dò được username nào có thật.
        const string genericError = "Tên đăng nhập hoặc mật khẩu không đúng.";

        var user = await _userManager.FindByNameAsync(username);
        if (user is null)
            return genericError;

        // Tài khoản bị admin khóa thì coi như đăng nhập thất bại.
        // Kiểm tra trước khi gọi PasswordSignInAsync để không ghi cookie nhầm.
        if (!user.IsActive)
            return genericError;

        // [SỬA] PasswordSignInAsync vừa kiểm tra mật khẩu vừa ghi cookie.
        // lockoutOnFailure: true -> đếm số lần sai, đủ ngưỡng thì khóa tạm.
        var result = await _signInManager.PasswordSignInAsync(
            user, password, isPersistent: false, lockoutOnFailure: true);

        if (result.IsLockedOut)
            return "Tài khoản tạm thời bị khóa do đăng nhập sai quá nhiều lần. Thử lại sau ít phút.";

        return result.Succeeded ? null : genericError;
    }

    public Task LogoutAsync() => _signInManager.SignOutAsync();
}
