
using IdentityApp.Context;
using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IEMailSender, SmtpEmailSender>(i =>
new SmtpEmailSender(
	builder.Configuration["EmailSender:Host"],
	builder.Configuration.GetValue<int>("EmailSender:Port"),
	builder.Configuration.GetValue<bool>("EmailSender:EnableSSl"),
	builder.Configuration["EmailSender:Username"],
	builder.Configuration["EmailSender:Password"]
));
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<IdentityContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DBConStr"));
});
builder.Services.AddIdentity<AppUser,AppRole>().AddEntityFrameworkStores<IdentityContext>().AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
	options.Password.RequiredLength = 6;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireLowercase = false;
	options.Password.RequireUppercase = false;
	options.Password.RequireDigit = false;

	options.User.RequireUniqueEmail = true;
	//options.User.AllowedUserNameCharacters = "asdfghjkl";
	
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
	options.Lockout.MaxFailedAccessAttempts = 5; 

	options.SignIn.RequireConfirmedEmail = true;

});
builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/Account/Login";
	options.AccessDeniedPath = "/Accout/AccessDenied";//erisim reddedildi sayfasi
	options.SlidingExpiration = true;//uygulamayi kullanirken surenin artmasi
	options.ExpireTimeSpan = TimeSpan.FromMinutes(30);//uygulamaya giris yaptiktan sonra bitecek sure
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

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
IdentitySeedData.IdentityTestUser(app);

app.Run();
