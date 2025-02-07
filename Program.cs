using EXE202.DAO;
using EXE202.Hubs;
using EXE202.Models;
using EXE202.Services;
using EXE202.Services.Impl;
using Microsoft.EntityFrameworkCore;

namespace EXE202
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Cấu hình session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddHttpClient("CassoAPI", client =>
            {
                client.BaseAddress = new Uri("https://oauth.casso.vn/v2/");
            });
            builder.Services.AddHttpClient("VietQRAPI", client =>
            {
                client.BaseAddress = new Uri("https://api.vietqr.io/v2/");
            });
            // Cấu hình DbContext
            builder.Services.AddDbContext<EXE202Context>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddSignalR();

            // Đăng ký các dịch vụ DAO
            builder.Services.AddScoped<AccountDAO>();
            builder.Services.AddScoped<FinanceDAO>();
            builder.Services.AddScoped<HomestayDAO>();
            builder.Services.AddScoped<BookingContractDAO>();
            builder.Services.AddScoped<TransactionDAO>();
            
            builder.Services.AddScoped<IQRService, QRService>();
            builder.Services.AddScoped<ITransactionService, TransactionService>();
            
            
            //Add Hangfire
            // builder.Services.AddHangfire(config => config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
            // builder.Services.AddHangfireServer();

            builder.Services.AddLogging();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
    
            app.UseRouting();

            // Sử dụng session
            app.UseSession();

            app.UseAuthorization();
            
            //
            // app.UseHangfireDashboard();
            // Định tuyến mặc định
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Homestay}/{action=LoadHomestay}/{id?}");
            
            // using(var scope = app.Services.CreateScope())
            // {
            //     var scheduleService = scope.ServiceProvider.GetRequiredService<IScheduleService>();
            //     
            //     RecurringJob.AddOrUpdate("1",() => scheduleService.ScanTransactionAsync(), "*/1 * * * *");
            // }
            
            app.UseEndpoints(endpoints =>
            {
                // Các endpoint khác...
                endpoints.MapHub<TransactionHub>("/transactionHub");
            });
            app.Run();
        }
    }
}
