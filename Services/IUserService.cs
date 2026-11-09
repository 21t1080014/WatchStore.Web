using WatchStore.Web.Entities;

namespace WatchStore.Web.Services;

public interface IUserService
{
    // [SỬA] Thêm Role vào tuple — xem ghi chú ở IUserRepository.
    Task<IEnumerable<(User User, string Role, int TaskCount)>> GetAllAsync();

    // Cả ba hàm đều nhận currentUserId để kiểm tra ràng buộc an toàn.
    // Trả null nếu thành công, hoặc chuỗi mô tả lỗi.
    Task<string?> ChangeRoleAsync(int targetId, int currentUserId);
    Task<string?> ToggleActiveAsync(int targetId, int currentUserId);
    Task<string?> DeleteAsync(int targetId, int currentUserId);
}
