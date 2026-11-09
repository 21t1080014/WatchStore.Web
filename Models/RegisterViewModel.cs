using System.ComponentModel.DataAnnotations;

namespace WatchStore.Web.Models;

// Dữ liệu form đăng ký. KHÔNG có property Role —
// nếu có, người dùng sửa HTML là tự cấp quyền admin được.
public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự.")]
    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = string.Empty;

    // Đổi câu thông báo cho khớp với rào mật khẩu đã cấu hình trong Program.cs.
    // Lưu ý: kiểm tra ở đây chỉ là lớp nhanh phía client; lá chắn thật là
    // UserManager.CreateAsync — nó mới là nơi áp đủ các rào của Identity.
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự, có cả chữ hoa, chữ thường và số.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}