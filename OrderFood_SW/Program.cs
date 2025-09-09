using OrderFood_SW.Helper;
using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Repositories;
using OrderFood_SW.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DatabaseHelperEF>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();

// add builder Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Timeout session after 30 minutes of inactivity
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Scan all Repository
builder.Services.Scan(scan => scan
    .FromAssemblyOf<CategoryRepository>() // select 1 class root to get assembly
    .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Repository")))
    .AsSelf() // self sign-up (can replace .AsImplementedInterfaces() if use interface)
    .WithScopedLifetime()
);

// Scan all Service
builder.Services.Scan(scan => scan
    .FromAssemblyOf<CategoryService>()
    .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Service")))
    .AsSelf()
    .WithScopedLifetime()
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// allow serving static files
app.UseStaticFiles();

// enable HSTS
app.UseRouting();

// add middleware Session before Authorization
app.UseSession();

// Custom middleware for session-based authentication
app.UseMiddleware<SessionAuthMiddleware>();

// Use authentication and authorization
app.UseAuthentication();

// Ensure that the session is available before authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=CustomerOrder}/{action=Index}/{id?}");

app.Run();
