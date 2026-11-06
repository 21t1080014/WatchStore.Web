using WatchStore.Web.Entities;

namespace WatchStore.Web.Services;

public interface IUserService
{
    Task<IEnumerable<(User User, int TaskCount)>> GetAllAsync();

    // Cả ba hàm đều nhận currentUserId để kiểm tra ràng buộc an toàn.
    // Trả null nếu thành công, hoặc chuỗi mô tả lỗi.
    Task<string?> ChangeRoleAsync(int targetId, int currentUserId);
    Task<string?> ToggleActiveAsync(int targetId, int currentUserId);
    Task<string?> DeleteAsync(int targetId, int currentUserId);
}