using AirGnG.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ... (Existing service registrations)
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AirGnGContext>(options =>
    options.UseInMemoryDatabase("AirGnGDb"));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

//  IMPORTANT FIX STARTS HERE
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AirGnGContext>();
    // Forces the in-memory database to be created and applies seed data from OnModelCreating
    context.Database.EnsureCreated();
}
//  IMPORTANT FIX ENDS HERE

// ... (Existing middleware configuration)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();