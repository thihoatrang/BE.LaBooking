using Appointments.Infrastructure.Data;
using Appointments.Infrastructure.Repository;
using Appointments.Application.Services;
using Appointments.Application.Services.IService;
using Appointments.Application.Services.Saga;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace Appointments.Services.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<AppointmentDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("AppointmentConnection")));
            
            // Add Saga Database Context
            builder.Services.AddDbContext<SagaDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SagaConnection")));

            // Add AutoMapper with specific configuration
            builder.Services.AddAutoMapper(typeof(MappingConfig));

            // Configure HTTP clients
            builder.Services.AddHttpClient("UserService", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:UsersAPI"]);
            });
            builder.Services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = 7071; // Port cho HTTPS
            });

            builder.Services.AddHttpClient("LawyerService", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:LawyersAPI"]);
            });

            builder.Services.AddHttpClient<WorkSlotApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:LawyersAPI"]);
            });

            builder.Services.AddHttpClient<LawyerProfileApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:LawyersAPI"]);
            });
            builder.Services.AddHttpClient<UserApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:UsersAPI"]);
            });
            builder.Services.AddScoped<IEmailService, EmailService>();

            // Add Services
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<AppointmentWithUserLawyerService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IPaymentCalculationService, PaymentCalculationService>();
            builder.Services.AddScoped<ITransactionService, TransactionService>();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IPaymentProvider, VnPayService>();
            builder.Services.AddScoped<IEnumerable<IPaymentProvider>>(provider => 
                new[] { provider.GetRequiredService<IPaymentProvider>() });
            
            // Add Saga Services
            builder.Services.AddScoped<ISagaStateRepository, SagaStateRepository>();
            builder.Services.AddScoped<IAppointmentSagaService, AppointmentSagaService>();
       

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Appointments API", Version = "v1" });
                c.EnableAnnotations();
                c.AddServer(new OpenApiServer { Url = "https://localhost:5000/api/appointments" });
            });

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

            // Ensure Saga database is created on startup (for simple bootstrapping)
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var sagaDb = scope.ServiceProvider.GetRequiredService<SagaDbContext>();
                    sagaDb.Database.EnsureCreated();
                }
                catch { }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Appointments API v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors("AllowFrontend");

            app.MapControllers();

            app.Run();
        }
    }
}
