using ChallengeManagementServer.Services;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

public class TestFixture
{
    public ServiceProvider ServiceProvider { get; }

    public TestFixture()
    {
        var services = new ServiceCollection();

        // Đăng ký Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = "localhost:6379"; // Thay bằng cấu hình Redis của bạn
            return ConnectionMultiplexer.Connect(configuration);
        });

        // Đăng ký ChallengeService
        services.AddTransient<ChallengeService>();

        ServiceProvider = services.BuildServiceProvider();
    }
}
