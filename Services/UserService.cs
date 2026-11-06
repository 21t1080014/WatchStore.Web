using WatchStore.Web.Entities;
using WatchStore.Web.Repositories;

namespace WatchStore.Web.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<(User User, int TaskCount)>> GetAllAsync()
        => await _userRepository.GetAllWithTaskCountAsync();

    public async Task<string?> ChangeRoleAsync(int targetId, int currentUserId)
    {
        // Ràng buộc 1: không tự hạ quyền chính mình.
        if (targetId == currentUserId)
            return "Không thể thay đổi vai trò của chính mình.";

        var user = await _userRepository.GetByIdAsync(targetId);
        if (user is null)
            return "Không tìm thấy tài khoản.";

        // Ràng buộc 2: không hạ quyền admin cuối cùng.
        // Nếu không chặn, hệ thống sẽ không còn ai quản trị được.
        if (user.Role == "Admin" && await _userRepository.CountAdminsAsync() <= 1)
            return "Không thể hạ quyền admin cuối cùng.";

        user.Role = user.Role == "Admin" ? "User" : "Admin";
        await _userRepository.UpdateAsync(user);
        return null;
    }

    public async Task<string?> ToggleActiveAsync(int targetId, int currentUserId)
    {
        // Tự khóa mình thì lần sau không đăng nhập được nữa.
        if (targetId == currentUserId)
            return "Không thể khóa tài khoản của chính mình.";

        var user = await _userRepository.GetByIdAsync(targetId);
        if (user is null)
            return "Không tìm thấy tài khoản.";

        user.IsActive = !user.IsActive;
        await _userRepository.UpdateAsync(user);
        return null;
    }

    public async Task<string?> DeleteAsync(int targetId, int currentUserId)
    {
        if (targetId == currentUserId)
            return "Không thể xóa tài khoản của chính mình.";

        var user = await _userRepository.GetByIdAsync(targetId);
        if (user is null)
            return "Không tìm thấy tài khoản.";

        if (user.Role == "Admin" && await _userRepository.CountAdminsAsync() <= 1)
            return "Không thể xóa admin cuối cùng.";

        // Tasks có cascade delete nên task của user này mất theo.
        await _userRepository.DeleteAsync(user);
        return null;
    }
}