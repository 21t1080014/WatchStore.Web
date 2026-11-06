using Microsoft.AspNetCore.Identity;
using WatchStore.Web.Entities;
using WatchStore.Web.Repositories;

namespace WatchStore.Web.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<string?> RegisterAsync(string username, string password)
    {
        if (await _userRepository.UsernameExistsAsync(username))
            return "Tên đăng nhập đã tồn tại.";

        var user = new User
        {
            Username = username,
            Role = "User",
            IsActive = true
        };

        user.PasswordHash = _hasher.HashPassword(user, password);

        await _userRepository.AddAsync(user);
        return null;
    }

    public async Task<User?> ValidateLoginAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user is null)
            return null;

        // Tài khoản bị admin khóa thì coi như đăng nhập thất bại.
        if (!user.IsActive)
            return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        return result == PasswordVerificationResult.Success
            ? user
            : null;
    }
}