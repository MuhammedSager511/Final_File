using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FileUploadDownlaod.DataContext;
using FileUploadDownlaod.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;






var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<FileDetailsDbContext>(options => options.UseSqlServer(

    "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=FileDetails")
);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme ).AddCookie(
    
    options =>
    {
        options.LoginPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(5);
        options.SlidingExpiration=true;
    }
    );





builder.Services.AddScoped<IFileDetailsRepository, FileDetailsRepository>();
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

app.Run();
