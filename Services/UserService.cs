using Microsoft.AspNetCore.Identity;
using WatchStore.Web.Entities;
using WatchStore.Web.Repositories;

namespace WatchStore.Web.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;

    public UserService(IUserRepository userRepository, UserManager<User> userManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<IEnumerable<(User User, string Role, int TaskCount)>> GetAllAsync()
        => await _userRepository.GetAllWithTaskCountAsync();

    public async Task<string?> ChangeRoleAsync(int targetId, int currentUserId)
    {
        // Ràng buộc 1: không tự hạ quyền chính mình.
        if (targetId == currentUserId)
            return "Không thể thay đổi vai trò của chính mình.";

        // [SỬA] FindByIdAsync nhận string vì Identity thiết kế chung cho cả khóa GUID.
        var user = await _userManager.FindByIdAsync(targetId.ToString());
        if (user is null)
            return "Không tìm thấy tài khoản.";

        // [SỬA] Đọc role từ bảng UserRoles thay vì từ cột user.Role.
        var currentRoles = await _userManager.GetRolesAsync(user);
        var isAdmin = currentRoles.Contains("Admin");

        // Ràng buộc 2: không hạ quyền admin cuối cùng.
        // Nếu không chặn, hệ thống sẽ không còn ai quản trị được.
        if (isAdmin && await _userRepository.CountAdminsAsync() <= 1)
            return "Không thể hạ quyền admin cuối cùng.";

        // Xóa hết role cũ rồi gán role mới, tránh user mang 2 role cùng lúc.
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
            return string.Join(" ", removeResult.Errors.Select(e => e.Description));

        var addResult = await _userManager.AddToRoleAsync(user, isAdmin ? "User" : "Admin");
        return addResult.Succeeded
            ? null
            : string.Join(" ", addResult.Errors.Select(e => e.Description));
    }

    public async Task<string?> ToggleActiveAsync(int targetId, int currentUserId)
    {
        // Tự khóa mình thì lần sau không đăng nhập được nữa.
        if (targetId == currentUserId)
            return "Không thể khóa tài khoản của chính mình.";

        var user = await _userManager.FindByIdAsync(targetId.ToString());
        if (user is null)
            return "Không tìm thấy tài khoản.";

        user.IsActive = !user.IsActive;

        // [SỬA] UpdateAsync của UserManager thay cho repository.UpdateAsync.
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? null
            : string.Join(" ", result.Errors.Select(e => e.Description));
    }

    public async Task<string?> DeleteAsync(int targetId, int currentUserId)
    {
        if (targetId == currentUserId)
            return "Không thể xóa tài khoản của chính mình.";

        var user = await _userManager.FindByIdAsync(targetId.ToString());
        if (user is null)
            return "Không tìm thấy tài khoản.";

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin") && await _userRepository.CountAdminsAsync() <= 1)
            return "Không thể xóa admin cuối cùng.";

        // Tasks có cascade delete nên task của user này mất theo.
        // Bản ghi trong UserRoles / UserClaims cũng bị xóa theo cascade của Identity.
        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded
            ? null
            : string.Join(" ", result.Errors.Select(e => e.Description));
    }
}
