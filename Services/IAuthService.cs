
using WatchStore.Web.Entities;
namespace WatchStore.Web.Services;

public interface IAuthService
{
    Task<string?> RegisterAsync(string username, string password);
    Task<User?> ValidateLoginAsync(string username, string password);
}

