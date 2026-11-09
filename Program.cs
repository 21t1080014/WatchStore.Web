using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WatchStore.Web.Data;
using WatchStore.Web.Entities;
using WatchStore.Web.Repositories;
using WatchStore.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký AppDbContext vào DI, đọc chuỗi kết nối từ appsettings.json.
// AddIdentity đặt trước, vì AddEntityFrameworkStores cần DbContext đã có trong DI.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// SỬA - chuyển sang ASP.NET Core Identity
// Đã XÓA khối AddAuthentication(CookieAuthenticationDefaults...).AddCookie(...) cũ.
// Lý do: AddIdentity tự đăng ký 3 cookie scheme riêng của nó
// (Identity.Application, Identity.External, Identity.TwoFactorUserId).
// Để cả hai thì scheme mặc định bị ghi đè lẫn nhau và đăng nhập sẽ không nhận.
builder.Services
    .AddIdentity<User, IdentityRole<int>>(options =>
    {
        // Mặc định Identity bắt mật khẩu có: chữ hoa, chữ thường, số, ký tự đặc biệt.
        // Giữ nguyên các rào còn lại, chỉ bỏ ký tự đặc biệt cho đỡ khó dùng khi demo.
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;

        // Sai 5 lần thì khóa 5 phút. Đây là thứ bản tự viết trước đây chưa có.
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

        // Username chỉ cho ký tự an toàn, chặn khoảng trắng và ký tự lạ.
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
    })
    .AddEntityFrameworkStores<AppDbContext>()   // lưu vào AppDbContext
    .AddDefaultTokenProviders();                // cần cho đổi/quên mật khẩu sau này

// Cấu hình cookie của Identity. Thay cho options trong AddCookie cũ.
builder.Services.ConfigureApplicationCookie(options =>
{
    // Chưa đăng nhập mà vào trang cần quyền → đẩy về đây.
    options.LoginPath = "/Account/Login";

    // Đã đăng nhập nhưng không đủ quyền (user thường vào /Users) → đẩy về đây.
    options.AccessDeniedPath = "/Account/Login";

    // Cookie hết hạn sau 8 tiếng.
    options.ExpireTimeSpan = TimeSpan.FromHours(8);

    // Mỗi lần có hoạt động thì gia hạn lại, tránh đang làm thì bị đá ra.
    options.SlidingExpiration = true;
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Tạo role và admin đầu tiên.
using (var scope = app.Services.CreateScope())
{
    // Lấy UserManager + RoleManager thay vì AppDbContext.
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    await DbSeeder.SeedAsync(userManager, roleManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
