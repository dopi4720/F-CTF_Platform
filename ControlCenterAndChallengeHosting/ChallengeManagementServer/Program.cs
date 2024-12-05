
using ChallengeManagementServer.Configs;
using ChallengeManagementServer.ServiceInterfaces;
using ChallengeManagementServer.Services;
using ResourceShared.Configs;
using ResourceShared.Utils;
using StackExchange.Redis;

namespace ChallengeManagementServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IChallengeService, ChallengeService>();
            builder.Services.AddScoped<IPerformanceService, PerformanceService>();

            new ChallengeManagementConfigHelper().InitConfig();

            // Cấu hình Redis
            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(RedisConfigs.ConnectionString));

            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run($"{ServiceConfigs.ServerHost}:{ServiceConfigs.ServerPort}");
        }
    }
}
