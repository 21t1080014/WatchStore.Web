using System.ComponentModel.DataAnnotations;
using WatchStore.Web.Entities;

namespace WatchStore.Web.Models;

// Dữ liệu form tạo/sửa task.
// KHÔNG có UserId — chủ sở hữu lấy từ cookie, không lấy từ form.
public class TaskFormViewModel
{
    // 0 = đang tạo mới, khác 0 = đang sửa task có sẵn.
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
    [StringLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự.")]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự.")]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Trạng thái")]
    public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;

    [DataType(DataType.Date)]
    [Display(Name = "Hạn hoàn thành")]
    public DateTime? DueDate { get; set; }
}