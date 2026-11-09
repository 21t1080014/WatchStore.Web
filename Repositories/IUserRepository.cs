using WatchStore.Web.Entities;

namespace WatchStore.Web.Repositories;

// Tầng truy cập dữ liệu cho bảng Users.
// CHỈ chứa câu truy vấn, không chứa quy tắc nghiệp vụ.
public interface IUserRepository
{
    Task<IEnumerable<(User User, string Role, int TaskCount)>> GetAllWithTaskCountAsync();

    // Đếm số admin hiện có, để chặn việc hạ quyền admin cuối cùng.
    Task<int> CountAdminsAsync();
}