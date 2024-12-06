namespace ChallengeManagementServerTest.Utils
{
    using System;
    using System.Threading.Tasks;
    using ChallengeManagementServer.Configs;
    using ChallengeManagementServer.Utils;
    using Moq;
    using StackExchange.Redis;
    using Xunit;

    public class K8sHelperTests
    {
        private readonly K8sHelper _testClass;
        private int _challengeId;
        private readonly ChallengeManagementConfigHelper configHelper;
        private readonly Mock<IConnectionMultiplexer> _connectionMultiplexer;

        public K8sHelperTests()
        {
            _challengeId = 1;
            _connectionMultiplexer = new Mock<IConnectionMultiplexer>();
            _testClass = new K8sHelper(_challengeId, _connectionMultiplexer.Object);
            configHelper = new ChallengeManagementConfigHelper();
            configHelper.InitConfig();
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new K8sHelper(_challengeId, _connectionMultiplexer.Object);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullConnectionMultiplexer()
        {
            Assert.Throws<Exception>(() => new K8sHelper(_challengeId, default(IConnectionMultiplexer)));
        }

        [Fact]
        public async Task CanCallBuildImagesAndUpdateYamlAsync()
        {
            // Act
           var exception = await Assert.ThrowsAsync<Exception>(() => _testClass.BuildImagesAndUpdateYamlAsync());

            // Assert
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task CanCallRemoveImageFromDiskAsync()
        {
            // Act
            var exception = await Assert.ThrowsAsync<Exception>(() => _testClass.RemoveImageFromDiskAsync());

            // Assert
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task CanCallDeployToK8sAsync()
        {
            // Act
            var exception = await Assert.ThrowsAsync<Exception>(() => _testClass.DeployToK8s(2));

            // Assert
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task CanCallGetPodNameFromDeploymentAsync()
        {
            // Act
            var exception = await Assert.ThrowsAsync<Exception>(() => _testClass.GetPodNameFromDeployment(2));

            // Assert
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task CanCallGetDeploymentLogsAsync()
        {
            // Act
            var exception = await Assert.ThrowsAsync<Exception>(() => _testClass.GetDeploymentLogsAsync(2));

            // Assert
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task CanCallForwardPortAsync()
        {
            // Act
            var exception = await Assert.ThrowsAsync<Exception>(() => _testClass.ForwardPort(2));

            // Assert
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task CanCallClosePortAsync()
        {
            // Act
            var exception = await Assert.ThrowsAsync<Exception>(() => _testClass.StopChallengeAsync(2));

            // Assert
            Assert.Empty(exception.Message);
        }

        [Fact]
        public async Task CanCallCheckPodStatusAsync()
        {
            // Act
            var exception = await Assert.ThrowsAsync<Exception>(() => _testClass.CheckPodStatusAsync(2));

            // Assert
            Assert.Empty(exception.Message);
        }
    }
}