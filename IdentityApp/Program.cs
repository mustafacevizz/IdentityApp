using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEmailSender,SmtpEmailSender>(i=>
new SmtpEmailSender(
    builder.Configuration["EmailSender:Host"],
    builder.Configuration.GetValue<int>("EmailSender:Port"),
    builder.Configuration.GetValue<bool>("EmailSender:EnableSSL"),
    builder.Configuration["EmailSender:Username"],
    builder.Configuration["EmailSender:Password"])
    );
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<IdentityContext>(options => options.UseSqlite(builder.Configuration["ConnectionStrings:SQLite_Connection"]));
builder.Services.AddIdentity<AppUser,AppRole>().AddEntityFrameworkStores<IdentityContext>().AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
    
    options.User.RequireUniqueEmail = true;
    //options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz";
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); //users account will be locked for 5 minutes after reaching the maximum number of failed login attempts.
    options.Lockout.MaxFailedAccessAttempts = 5;    //users account will be locked after 5 consecutive failed login attempts.
    options.SignIn.RequireConfirmedEmail= true;
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;   //users session expiration time with each request, keeping the session active as long as the user remains active.
    options.ExpireTimeSpan = TimeSpan.FromDays(30); //users session will expire after 30 days of inactivity.
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
IdentitySeedData.IdentityTestUser(app);
app.Run();
