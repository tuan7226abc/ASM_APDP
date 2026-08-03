using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext;
using SIMS.Interfaces;
using SIMS.Repositories;
using SIMS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace SIMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // config connect to database
            builder.Services.AddDbContext<SimsDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSqlServer")));
            // registe services
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<ICourseService, CourseService>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option =>
            {
                option.LoginPath = "/Login";
                option.LogoutPath = "/Login/Logout";
                option.AccessDeniedPath = "/Auth/AccessDenied";
            });
            builder.Services.AddAuthorization(option =>
            {
                option.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                option.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
                option.AddPolicy("TeacherOnly", policy => policy.RequireRole("Teacher"));
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
                pattern: "{controller=Login}/{action=Index}/{id?}");

            app.Run();


        }
    }
}
