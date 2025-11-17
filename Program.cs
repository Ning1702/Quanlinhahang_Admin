using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Quanlinhahang_Admin.Data;

var builder = WebApplication.CreateBuilder(args);

// ====================== CẤU HÌNH DỊCH VỤ ======================
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<QlnhContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QLNH")));

builder.Services.AddDataProtection()
    .UseEphemeralDataProtectionProvider();

// 🧩 Cookie đăng nhập
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "QLNH.Auth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

// ⚙️ Session (dành cho controller lưu session)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ====================== BUILD APP ======================
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🧩 Thứ tự chuẩn: Auth -> Session -> Authorization
app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

// ====================== ROUTE ======================
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
