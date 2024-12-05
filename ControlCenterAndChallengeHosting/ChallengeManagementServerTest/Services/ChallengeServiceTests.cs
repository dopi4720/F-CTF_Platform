namespace ChallengeManagementServerTest.Services
{
    using System;
    using System.Threading.Tasks;
    using ChallengeManagementServer.Configs;
    using ChallengeManagementServer.Services;
    using ChallengeManagementServer.Utils;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using ResourceShared.Models;
    using ResourceShared.Utils;
    using SocialSync.Shared.Utils.ResourceShared.Utils;
    using StackExchange.Redis;
    using Xunit;
    using Xunit.Sdk;

    public class ChallengeServiceTests
    {
        private readonly ChallengeService _testClass;
        private readonly Mock<IConnectionMultiplexer> _connectionMultiplexer;
        private readonly ChallengeManagementConfigHelper configHelper;
        private readonly Mock<K8sHelper> _mockK8sHelper;
        private readonly Mock<RedisHelper> _mockRedisHelper;

        private readonly Mock<IFormFile> mockFile;

        public ChallengeServiceTests()
        {
            _connectionMultiplexer = new Mock<IConnectionMultiplexer>();
            _testClass = new ChallengeService(_connectionMultiplexer.Object);
            configHelper = new ChallengeManagementConfigHelper();
            configHelper.InitConfig();
            _mockK8sHelper = new Mock<K8sHelper>(/* constructor arguments */);
            _mockRedisHelper = new Mock<RedisHelper>(_connectionMultiplexer.Object);
            // Mocks and common setup
            mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.zip");
            mockFile.Setup(f => f.ContentType).Returns("application/zip");
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns<Stream, CancellationToken>((stream, token) =>
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write("Fake zip file content");
                        }
                        return Task.CompletedTask;
                    });
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new ChallengeService(_connectionMultiplexer.Object);

            // Assert
            Assert.NotNull(instance);
        }

        //[Fact]
        //public async Task CanCallBuildDeployAndUpdatetoCDAsync()
        //{
        //    // Arrange
        //    var ChallengeId = 900692082;

        //    // Act
        //    await _testClass.BuildDeployAndUpdatetoCDAsync(ChallengeId);

        //    // Assert
        //    // throw new NotImplementedException("Create or modify test");
        //}

        //[Fact]
        //public async Task CanCallUpdateChallengeStatusToCTFdAsync()
        //{
        //    // Arrange
        //    var ChallengeId = 683571267;
        //    var DeployLogs = "TestValue1139432885";
        //    var status = "TestValue278165425";
        //    var ImageLink = "TestValue658279911";

        //    // Act
        //    await _testClass.UpdateChallengeStatusToCTFd(ChallengeId, DeployLogs, status, ImageLink);

        //    // Assert
        //    throw new NotImplementedException("Create or modify test");
        //}

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateChallengeStatusToCTFdWithInvalidDeployLogsAsync(string value)
        {
            await Assert.ThrowsAsync<Exception>(() => _testClass.UpdateChallengeStatusToCTFd(578117443, value, "TestValue1080160722", "TestValue1979672467"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateChallengeStatusToCTFdWithInvalidStatusAsync(string value)
        {
            await Assert.ThrowsAsync<Exception>(() => _testClass.UpdateChallengeStatusToCTFd(1498168412, "TestValue1439158902", value, "TestValue1216856411"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CannotCallUpdateChallengeStatusToCTFdWithInvalidImageLinkAsync(string value)
        {
            await Assert.ThrowsAsync<Exception>(() => _testClass.UpdateChallengeStatusToCTFd(1096916424, "TestValue1999827220", "TestValue1959230235", value));
        }

        [Fact]
        public async Task CanCallGetDeploymentLogsAsync()
        {
            // Arrange
            var ChallengeId = 1756736611;
            var TeamId = 1324959284;

            // Act
            var result = await _testClass.GetDeploymentLogsAsync(ChallengeId, TeamId);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task CanCallSaveFileAsync()
        {
            // Arrange
            var ChallangeId = 295863569;
            var @file = new Mock<IFormFile>().Object;

            // Act
            var result = await _testClass.SaveFileAsync(ChallangeId, file);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public async Task SaveFileAsync_ShouldThrowExceptionWhenFileIsNull()
        {
            // Arrange
            var challengeId = 123;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _testClass.SaveFileAsync(challengeId, null));
            Assert.Contains("Upload file fail. File not found", exception.Message);
        }

        [Fact]
        public async Task SaveFileAsync_ShouldThrowExceptionForInvalidFileExtension()
        {
            // Arrange
            var challengeId = 123;
            mockFile.Setup(f => f.FileName).Returns("invalid.txt");
            mockFile.Setup(f => f.ContentType).Returns("text/plain");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _testClass.SaveFileAsync(challengeId, mockFile.Object));
            Assert.Contains("Upload file fail. File zip only", exception.Message);
        }

        [Fact]
        public async Task SaveFileAsync_ShouldThrowExceptionWhenValidationFails()
        {
            // Arrange
            var challengeId = 10;

            string fileName = "fileinvalid.zip";
            string relativePath = Path.Combine("Files", fileName);
            string fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);
            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            IFormFile file =new FormFile(fileStream, 0, fileStream.Length, "formfile", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/zip"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _testClass.SaveFileAsync(challengeId, file));
            Assert.NotEmpty(exception.Message);
        }


        //[Fact]
        //public async Task CanCallStartAsync()
        //{
        //    // Arrange
        //    var ChallengeId = 1107794442;
        //    var TeamId = 800625761;

        //    // Act
        //    var result = await _testClass.StartAsync(ChallengeId, TeamId);

        //    // Assert
        //    Assert.NotNull(result);
        //}


        //[Fact]
        //public async Task CanCallStopAsync()
        //{
        //    // Arrange
        //    var ChallengeId = 118997425;
        //    var TeamId = 2068122536;

        //    // Act
        //    var result = await _testClass.StopAsync(ChallengeId, TeamId);

        //    // Assert
        //    Assert.True(result);
        //}

        [Fact]
        public async Task StartAsync_Failed_NotExsited_Challenge()
        {
            // Arrange
            var ChallengeId = 1107794442;
            var TeamId = 800625761;

            // Assert
            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => _testClass.StartAsync(ChallengeId, TeamId));
        }

        [Fact]
        public async Task StopAsync_Failed_NotExsited_Challenge()
        {
            // Arrange
            var ChallengeId = 1107794442;
            var TeamId = 800625761;

            // Assert
            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => _testClass.StopAsync(ChallengeId, TeamId));
        }

        [Fact]
        public async Task BuildDeployAndUpdatetoCDAsync_SuccessfulExecution_ShouldUpdateRedis()
        {
            // Arrange
            int challengeId = 1;
            int teamId = -1;
            string deployLogs = "Deployment successful.";
            Dictionary<string, string> imageLink = new Dictionary<string, string>() { };

            _mockK8sHelper.Setup(k => k.BuildImagesAndUpdateYamlAsync())
                          .ReturnsAsync(imageLink);

            _mockK8sHelper.Setup(k => k.DeployToK8s(teamId)).Returns(Task.CompletedTask);

            _mockK8sHelper.Setup(k => k.CheckPodStatusAsync(teamId)).Returns((Task<bool>)Task.CompletedTask);

            _mockK8sHelper.Setup(k => k.GetDeploymentLogsAsync(teamId))
                          .ReturnsAsync(deployLogs);

            _mockRedisHelper.Setup(r => r.SetCacheAsync(It.IsAny<string>(), It.IsAny<DeploymentInfo>(), It.IsAny<TimeSpan>()))
                            .Returns((Task<bool>)Task.CompletedTask);

            // Act
            await _testClass.BuildDeployAndUpdatetoCDAsync(challengeId);

            // Assert
            _mockK8sHelper.Verify(k => k.BuildImagesAndUpdateYamlAsync(), Times.Once);
            _mockK8sHelper.Verify(k => k.DeployToK8s(teamId), Times.Once);
            _mockK8sHelper.Verify(k => k.CheckPodStatusAsync(teamId), Times.Once);
            _mockK8sHelper.Verify(k => k.GetDeploymentLogsAsync(teamId), Times.Once);
            _mockRedisHelper.Verify(r => r.SetCacheAsync(It.IsAny<string>(), It.IsAny<DeploymentInfo>(), It.IsAny<TimeSpan>()), Times.Once);
        }

    }
}