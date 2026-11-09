using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WatchStore.Web.Entities;

public class User: IdentityUser<int>
{
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property: EF Core dựa vào đây để biết quan hệ 1-N.
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}