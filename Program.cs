using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WatchStore.Web.Data;
using WatchStore.Web.Repositories;
using WatchStore.Web.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
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
// Add services to the container.
builder.Services.AddControllersWithViews();
// Đăng ký AppDbContext vào DI, đọc chuỗi kết nối từ appsettings.json.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();
//tạo admin đầu tiên 
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAdminAsync(context);
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
