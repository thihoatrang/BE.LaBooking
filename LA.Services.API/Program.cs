using Lawyer.Application.Services;
using Lawyer.Application.Services.IService;
using Lawyer.Application.Services.Saga;
using Lawyers.Application.Services;
using Lawyers.Application.Services.IService;
using Lawyers.Infrastructure.Data;
using Lawyers.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace LA.Services.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đăng ký Service
            builder.Services.AddScoped<ILawyerService, LawyerService>();
            builder.Services.AddScoped<ILawyerDiplomaService, LawyerDiplomaService>();
            builder.Services.AddScoped<IWorkSlotService, WorkSlotService>();
            builder.Services.AddScoped<IPracticeAreaService, PracticeAreaService>();
            builder.Services.AddScoped<IServiceService, ServiceService>();
            
            // Đăng ký Repository
            builder.Services.AddScoped<IWorkSlotRepository, WorkSlotRepository>();
            builder.Services.AddScoped<ILawyerDiplomaRepository, LawyerDiplomaRepository>();
            builder.Services.AddScoped<ILawyerProfileRepository, LawyerProfileRepository>();
            builder.Services.AddScoped<ILawyerPracticeAreaRepository, LawyerPracticeAreaRepository>();
            builder.Services.AddScoped<IPracticeAreaRepository, PracticeAreaRepository>();
            builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
            
            // Add Saga Services
            builder.Services.AddScoped<ILawyerSagaService, LawyerSagaService>();


            // Add services to the container.

            builder.Services.AddControllers();
            //Add DbContext

          

            //        builder.Services.AddDbContext<AppointmentDbContext>(options =>
            //options.UseSqlServer(builder.Configuration.GetConnectionString("AppointmentDbConnection")));

            builder.Services.AddDbContext<LawyerDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("LawyerDbConnection")));



            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lawyers API", Version = "v1" });
                c.EnableAnnotations();
                c.AddServer(new OpenApiServer { Url = "https://localhost:5000/api/lawyers" });
            });
            //AutoMapper
            builder.Services.AddAutoMapper(typeof(MappingConfig));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:5173")
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lawyers API v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // Use CORS
            app.UseCors("AllowFrontend");

            app.MapControllers();
            //ApplyMigrations(app); // Áp dụng migration khi ứng dụng khởi động
            app.Run();

            //tự động áp dụng các migration vào database khi ứng dụng khởi động
            //void ApplyMigrations(IApplicationBuilder app)
            //{
            //    using (var scope = app.ApplicationServices.CreateScope())
            //    {
            //        var dbContext = scope.ServiceProvider.GetRequiredService<LawyerDbContext>();
            //        dbContext.Database.Migrate();
            //    }
            //}
        }
    }
}
