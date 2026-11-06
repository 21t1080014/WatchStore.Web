namespace WatchStore.Web.Models;

// Một dòng trong bảng quản lý tài khoản.
// KHÔNG có PasswordHash — đó là lý do phải có ViewModel thay vì
// trả thẳng entity User ra View.
public class UserListItemViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TaskCount { get; set; }

    // Đánh dấu dòng của chính người đang đăng nhập,
    // để View làm mờ các nút thao tác.
    public bool IsCurrentUser { get; set; }
}