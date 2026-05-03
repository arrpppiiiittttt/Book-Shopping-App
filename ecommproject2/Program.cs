using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ecommproject2.DataAccess.Data;
using ecommproject2.DataAccess.Repository.IRepository;
using ecommproject2.DataAccess.Repository;
using Microsoft.AspNetCore.Identity.UI.Services;
using ecommproject2.Utility;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.OAuth;
using Stripe;
using Twilio.AspNet.Core;
using Twilio;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var cs = builder.Configuration.GetConnectionString("conStr") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(cs));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICoverTypeRepository, CoverTypeRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddRazorPages();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenies";
});

builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = "1035960331776586";
    options.AppSecret = "c8fa5b1d60a69096b724fee291d4d39a";
    options.AccessDeniedPath = $"/Identity/Account/Register";
});
builder.Services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId = "YOUR_CLIENT_SECRET";
    options.ClientSecret = "GOCSPX-xBChH-YOUR_CLIENT_SECRET-25X";
});
builder.Services.AddAuthentication().AddGitHub(options =>
{
    options.ClientId = "YOUR_CLIENT_SECRET";
    options.ClientSecret = "YOUR_CLIENT_SECRET";
});
builder.Services.AddAuthentication().AddTwitter(options =>
{
    options.ConsumerKey = "YOUR_ConsumerKey";
    options.ConsumerSecret = "YOUR_ConsumerSecret";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton<TwilioService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeSettings")["SecretKey"];

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
