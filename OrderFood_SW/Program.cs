using Microsoft.EntityFrameworkCore;
using OrderFood_SW.Helper;
using OrderFood_SW.Hubs;
using OrderFood_SW.Repositories;
using OrderFood_SW.Services;
using Serilog;
using Serilog.Exceptions;


var builder = WebApplication.CreateBuilder(args);

// -------------------- Serilog config --------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .CreateLogger();

builder.Host.UseSerilog();

// -------------------- Services --------------------
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DatabaseHelperEF>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Scan all Repository
builder.Services.Scan(scan => scan
    .FromAssemblyOf<CategoryRepository>()
    .AddClasses(c => c.Where(t => t.Name.EndsWith("Repository")))
    .AsSelf()
    .WithScopedLifetime()
);

// Scan all Service
builder.Services.Scan(scan => scan
    .FromAssemblyOf<CategoryService>()
    .AddClasses(c => c.Where(t => t.Name.EndsWith("Service")))
    .AsSelf()
    .WithScopedLifetime()
);

// Manual fallback
builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<NotificationService>();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseMiddleware<ErrorHandlingMiddleware>();

// -------------------- Middleware pipeline --------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseMiddleware<SessionAuthMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=CustomerOrder}/{action=Index}/{id?}");

try
{
    Log.Information("Starting OrderFood_SW web host...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

