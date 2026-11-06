using WatchStore.Web.Entities;

namespace WatchStore.Web.Repositories;

// Tầng truy cập dữ liệu cho bảng Users.
// CHỈ chứa câu truy vấn, không chứa quy tắc nghiệp vụ.
public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(int id);
    Task<bool> UsernameExistsAsync(string username);
    Task AddAsync(User user);
    // Danh sách user kèm số task của từng người, cho màn hình quản lý.
    Task<IEnumerable<(User User, int TaskCount)>> GetAllWithTaskCountAsync();

    // Đếm số admin hiện có, để chặn việc hạ quyền admin cuối cùng.
    Task<int> CountAdminsAsync();

    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
}