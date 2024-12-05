using ChallengeManagementServer.Services;
using ChallengeManagementServer.Utils;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ResourceShared.Models;
using SocialSync.Shared.Utils.ResourceShared.Utils;
using StackExchange.Redis;
using Xunit;

public class ChallengeServiceTests : IClassFixture<TestFixture>
{
    private readonly Mock<IConnectionMultiplexer> _mockConnectionMultiplexer;
    private readonly Mock<K8sHelper> _mockK8sHelper;
    private readonly Mock<RedisHelper> _mockRedisHelper;
    private readonly ChallengeService _challengeService;

    public ChallengeServiceTests()
    {
        _mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();
        _mockK8sHelper = new Mock<K8sHelper>(It.IsAny<int>(), _mockConnectionMultiplexer.Object);
        _mockRedisHelper = new Mock<RedisHelper>(_mockConnectionMultiplexer.Object);

        // Assuming YourClass is the class containing the method
        _challengeService = new ChallengeService(_mockConnectionMultiplexer.Object);
    }

}
