
using ControlCenterServer.Configs;
using ResourceShared.Configs;
using ResourceShared.Utils;
using StackExchange.Redis;

namespace ControlCenterServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            Console.WriteLine("Starting config");
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Init config from ControlConfig, SharedConfig
            new ControlCenterConfigHelper().InitConfig();

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

            Console.WriteLine("Starting Receive Request");

            app.Run($"{ServiceConfigs.ServerHost}:{ServiceConfigs.ServerPort}");
            //app.Run();
        }
    }
}
