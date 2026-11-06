using System.ComponentModel.DataAnnotations;

namespace WatchStore.Web.Entities;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; set; } = "User";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property: EF Core dựa vào đây để biết quan hệ 1-N.
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}