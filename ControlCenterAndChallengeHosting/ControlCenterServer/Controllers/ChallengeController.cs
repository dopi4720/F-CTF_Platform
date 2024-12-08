using ControlCenterServer.Configs;
using ControlCenterServer.DTOs.ChallengeDTOs;
using ControlCenterServer.Middlewares;
using ControlCenterServer.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ResourceShared.Configs;
using ResourceShared.DTOs;
using ResourceShared.Models;
using ResourceShared.ResponseViews;
using ResourceShared.Utils;
using RestSharp;
using SocialSync.Shared.Utils.ResourceShared.Utils;
using StackExchange.Redis;

namespace ControlCenterServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequireSecretKey]
    public class ChallengeController : ControllerBase
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        public ChallengeController(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] int challengeId, [FromForm] IFormFile file)
        {
            try
            {
                await Console.Out.WriteLineAsync($"Received upload request for challenge ID {challengeId}");

                // check key redis deploy ton tai hay khong
                RedisHelper redisHelper = new RedisHelper(_connectionMultiplexer);

                // set gia tri cho redis deploy key
                string redisDeployKey = $"{RedisConfigs.RedisDeployKey}{challengeId}";
                // get value redis deploy key 
                var redisGetDeployInfo = await redisHelper.GetFromCacheAsync<DeploymentInfo>(redisDeployKey);

                var UnixTimeNow = DateTimeHelper.GetDateTimeNowInUnix();
                string SecretKey = SecretKeyHelper.CreateSecretKey(UnixTimeNow, new());

                string TargetServerId = "";
                // neu chua deploy
                if (redisGetDeployInfo == null)
                {
                    List<ClusterStatisticInfo> performanceStatList = new List<ClusterStatisticInfo>();
                    foreach (ChallengeServerInfo challengeServerInfo in ControlCenterServiceConfig.ChallengeServerInfoList)
                    {
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Resource = "api/performance/stat";
                        request.AddHeader("SecretKey", SecretKey);

                        Dictionary<string, object> requestDictionary = new Dictionary<string, object>
                        {
                            { "UnixTime", UnixTimeNow },
                        };
                        string baseUrl = challengeServerInfo.ServerHost + ":" + challengeServerInfo.ServerPort;
                        MultiServiceConnector multiServiceConnector = new MultiServiceConnector(baseUrl);
                        var response
                          = await multiServiceConnector.ExecuteRequest<GenaralViewResponseData<ClusterStatisticInfo>>(request, requestDictionary, RequestContentType.Form);
                        if (response != null && !response.IsSuccess)
                        {
                            return BadRequest(response);
                        }

                        if (response != null && response.data != null)
                        {
                            ClusterStatisticInfo? performanceStatistic = response.data;
                            performanceStatList.Add(performanceStatistic!);
                        }
                    }

                    // uu tien nhung con host co cpu usage < 50% va con nhieu available memory 
                    var bestPerformanceHost = performanceStatList.Where(stat => stat.CpuUsage < 50)
                    .OrderByDescending(stat => stat.RamAvailable)
                    .FirstOrDefault();

                    // neu khong co nhung con co cpu usage < 50% thi uu tien nhung con co available memory > 20% va cpu usage min
                    if (bestPerformanceHost == null)
                    {
                        bestPerformanceHost = performanceStatList.Where(stat => stat.RamAvailable / stat.RamTotal * 100 > 20)
                        .OrderBy(stat => stat.CpuUsage)
                        .FirstOrDefault();
                    }

                    if (bestPerformanceHost == null)
                    {
                        return BadRequest(new GeneralView
                        {
                            Message = "No host machine available to deploy",
                            IsSuccess = false
                        });
                    }

                    TargetServerId = bestPerformanceHost.ServerId;
                }
                else
                {
                    // trước khi gọi sang upload xóa hết cache các instance đang chạy

                    string redisInstanceKey = $"{RedisConfigs.RedisChallengeInstanceListKey}";

                    var instanceList = await redisHelper.GetFromCacheAsync<List<DeploymentInfo>>(redisInstanceKey);

                    if (instanceList != null && instanceList.Count > 0)
                    {
                        instanceList = instanceList.Where(p => p.ChallengeId != challengeId).ToList();
                        await redisHelper.SetCacheAsync(redisInstanceKey, instanceList, TimeSpan.MaxValue);
                    }

                    redisGetDeployInfo.LastDeployTime = null;

                    await redisHelper.SetCacheAsync<DeploymentInfo>(redisDeployKey, redisGetDeployInfo, TimeSpan.FromDays(90));

                    TargetServerId = redisGetDeployInfo.ServerId;
                }

                var challengeHostServer = ControlCenterServiceConfig.ChallengeServerInfoList.FirstOrDefault(c => c.ServerId == TargetServerId);

                if (challengeHostServer == null) return BadRequest(new GeneralView()
                {
                    Message = "ChallengeHostServer is null, check config machine id",
                    IsSuccess = false
                });

                var UnixTimeDeploy = DateTimeHelper.GetDateTimeNowInUnix();
                var deployrequest = new RestRequest();
                deployrequest.Method = Method.Post;
                deployrequest.Resource = "api/challenge/upload";
                Dictionary<string, string> createScretKeyDictionary = new Dictionary<string, string>
                {
                    { "ChallengeId", challengeId.ToString()},
                };

                string secretkeyDeploy = SecretKeyHelper.CreateSecretKey(UnixTimeDeploy, createScretKeyDictionary);
                deployrequest.AddHeader("SecretKey", secretkeyDeploy);
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.ToArray();
                    deployrequest.AddFile("File", memoryStream.ToArray(), file.FileName, file.ContentType);
                }

                await Console.Out.WriteLineAsync($"Returned for challenge: {challengeId}");

                Dictionary<string, object> requestDeployDictionary = new Dictionary<string, object>
                 {
                        { "UnixTime", UnixTimeDeploy},
                        { "ChallengeId", challengeId},
                 };

                string baseDeployUrl = challengeHostServer.ServerHost + ":" + challengeHostServer.ServerPort;
                MultiServiceConnector multiServiceDeployConnector = new MultiServiceConnector(baseDeployUrl);

                GeneralView? deployResult
                  = await multiServiceDeployConnector.ExecuteRequest<GeneralView>(deployrequest, requestDeployDictionary, RequestContentType.Form);

                await Console.Out.WriteLineAsync(JsonConvert.SerializeObject(deployResult, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));

                DeploymentInfo deploymentInfo = new DeploymentInfo()
                {
                    ChallengeId = challengeId,
                    ServerId = challengeHostServer.ServerId,
                    LastDeployTime = DateTime.Now,
                };

                if (deployResult == null || !deployResult.IsSuccess)
                {
                    return BadRequest(new GeneralView
                    {
                        Message = deployResult != null ? deployResult.Message : $"Deploy fail challenge {challengeId}",
                        IsSuccess = false
                    });
                }

                await redisHelper.SetCacheAsync<DeploymentInfo>(redisDeployKey, deploymentInfo, TimeSpan.FromDays(90));

                await Console.Out.WriteLineAsync($"Deployed challenge {challengeId} to server {challengeHostServer.ServerId}");

                return Ok(new GeneralView
                {
                    Message = $"Just waiting deploying challenge ${challengeId}",
                    IsSuccess = true
                });
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Error in Upload file: " + ex.Message);
                return BadRequest(new GeneralView
                {
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message,
                    IsSuccess = false
                });
            }

        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteChallenge([FromForm] int challengeId)
        {
            await Console.Out.WriteLineAsync($"Received delete request for challenge ID {challengeId}");
            try
            {
                // tao connection redis server 
                RedisHelper redisHelper = new RedisHelper(_connectionMultiplexer);
                // set gia tri cho redis deploy key
                string redisDeployKey = $"{RedisConfigs.RedisDeployKey}{challengeId}";
                // check key redis deploy ton tai hay khong, get value redis deploy key, 
                var redisGetDeployInfo = await redisHelper.GetFromCacheAsync<DeploymentInfo>(redisDeployKey);

                if (redisGetDeployInfo == null || redisGetDeployInfo.LastDeployTime == null)
                {
                    return Ok(new GeneralView()
                    {
                        Message = $"Deleted",
                        IsSuccess = true
                    });
                }

                var challengeHostServer = ControlCenterServiceConfig.ChallengeServerInfoList.FirstOrDefault(c => c.ServerId == redisGetDeployInfo.ServerId);
                if (challengeHostServer == null)
                {
                    return Ok(new GeneralView()
                    {
                        Message = $"Deleted",
                        IsSuccess = true
                    });
                }

                var deleteRequest = new RestRequest();
                deleteRequest.Method = Method.Post;
                deleteRequest.Resource = "api/challenge/delete";

                long unixTime = DateTimeHelper.GetDateTimeNowInUnix();

                var DictScrKey = new Dictionary<string, string>()
                {
                    {"ChallengeId",challengeId.ToString()}
                };
                var DictMultiService = new Dictionary<string, object>
                {
                    { "UnixTime",unixTime},
                    {"ChallengeId",challengeId }
                };


                if (!DictMultiService.ContainsKey("UnixTime"))
                {
                    DictMultiService.Add("UnixTime", unixTime);
                }

                string secretKeyDeleteChallenge = SecretKeyHelper.CreateSecretKey(unixTime, DictScrKey);

                deleteRequest.AddHeader("SecretKey", secretKeyDeleteChallenge);

                string baseDeployUrl = challengeHostServer.ServerHost + ":" + challengeHostServer.ServerPort;


                MultiServiceConnector connector = new MultiServiceConnector(baseDeployUrl);

                GeneralView? deleteResult
                  = await connector.ExecuteRequest<GeneralView>(deleteRequest, DictMultiService, RequestContentType.Form);

                await Console.Out.WriteLineAsync($"Returned for challenge: {challengeId}");

                await Console.Out.WriteLineAsync($"Delete Result: {JsonConvert.SerializeObject(deleteResult)}");

                if (deleteResult == null || !deleteResult.IsSuccess)
                {
                    return BadRequest(deleteResult);
                }

                string redisInstanceKey = $"{RedisConfigs.RedisChallengeInstanceListKey}";

                var instanceList = await redisHelper.GetFromCacheAsync<List<DeploymentInfo>>(redisInstanceKey);

                if (instanceList != null && instanceList.Count > 0)
                {
                    instanceList = instanceList.Where(p => p.ChallengeId != challengeId).ToList();
                    await redisHelper.SetCacheAsync(redisInstanceKey, instanceList, TimeSpan.MaxValue);
                }

                await redisHelper.RemoveCacheAsync(redisDeployKey);

                return Ok(deleteResult);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Error in Delete Challenge: " + ex.Message);
                return BadRequest(new GeneralView
                {
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message,
                    IsSuccess = false
                });
            }
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartInstance([FromHeader] string SecretKey, [FromForm] StartChallengeInstanceRequest instanceInfo)
        {
            var redisQueueCount = 0;
            await Console.Out.WriteLineAsync($"Received start request for challenge ID {instanceInfo.ChallengeId} - Team {instanceInfo.TeamId}");
            // tao connection redis server 
            RedisHelper redisHelper = new RedisHelper(_connectionMultiplexer);
            //Check hàng chờ, số hàng chờ trùng với số Max Instance At Time của 1 team
            string RedisQueueDeployKey = $"{RedisConfigs.RedisQueueDeployKey}{instanceInfo.TeamId}";

            //Phục vụ việc trả về Bad Request
            string ErrorMessage = "Error while starting challange, please try again in some seconds";

            try
            {
                redisQueueCount = await redisHelper.GetFromCacheAsync<int>(RedisQueueDeployKey);
                if (redisQueueCount >= ServiceConfigs.MaxInstanceAtTime)
                {
                    return BadRequest(new GeneralView()
                    {
                        Message = $"Too many start challenge request, please try again later",
                        IsSuccess = false
                    });
                }
                redisQueueCount++;
                await redisHelper.SetCacheAsync(RedisQueueDeployKey, redisQueueCount, TimeSpan.MaxValue);

                // set gia tri cho redis deploy key
                string redisDeployKey = $"{RedisConfigs.RedisDeployKey}{instanceInfo.ChallengeId}";
                // check key redis deploy ton tai hay khong, get value redis deploy key, 
                var redisGetDeployInfo = await redisHelper.GetFromCacheAsync<DeploymentInfo>(redisDeployKey);
                if (redisGetDeployInfo == null || redisGetDeployInfo.LastDeployTime == null)
                {
                    return BadRequest(new GeneralView()
                    {
                        Message = $"Challenge {instanceInfo.ChallengeId} is not yet deployed, can't start",
                        IsSuccess = false
                    });
                }

                var challengeHostServer = ControlCenterServiceConfig.ChallengeServerInfoList.FirstOrDefault(c => c.ServerId == redisGetDeployInfo.ServerId);
                if (challengeHostServer == null) return BadRequest(new GeneralView()
                {
                    Message = "ChallengeHostServer is null, check config machine id",
                    IsSuccess = false
                });

                string redisInstanceKey = $"{RedisConfigs.RedisChallengeInstanceListKey}";

                var instanceList = await redisHelper.GetFromCacheAsync<List<DeploymentInfo>>(redisInstanceKey);

                if (instanceList != null)
                {
                    var instanceListByTeam = instanceList?.Where(p => p.TeamId == instanceInfo.TeamId).ToList();
                    string messageMaxInstanceAtTime = $"Each team can run only {ServiceConfigs.MaxInstanceAtTime} same time";

                    if (instanceListByTeam != null && instanceListByTeam.Count > 0)
                    {
                        DeploymentInfo? DeploymentInfo = instanceListByTeam.Where(p => p.ChallengeId == instanceInfo.ChallengeId).FirstOrDefault();
                        if (DeploymentInfo != null)
                        {
                            return Ok(new GenaralViewResponseData<string>
                            {
                                Message = $"Challenge {instanceInfo.ChallengeId} instance already running",
                                IsSuccess = true,
                                data = DeploymentInfo.DeploymentDomainName,
                            });
                        }
                    }
                    await Console.Out.WriteLineAsync($"Start for {instanceInfo.TeamId} - instanceListByTeam.Count: " + instanceListByTeam.Count);
                    List<int> challengeInstanceAreRunning = new List<int>();
                    if (instanceInfo.TeamId != -1 && instanceListByTeam != null && instanceListByTeam.Count >= ServiceConfigs.MaxInstanceAtTime)
                    {
                        messageMaxInstanceAtTime += "<br> The instances list are running:";
                        foreach (var instance in instanceListByTeam)
                        {
                            challengeInstanceAreRunning.Add(instance.ChallengeId);
                        }
                        return BadRequest(new GenaralViewResponseData<List<int>>
                        {
                            Message = messageMaxInstanceAtTime,
                            IsSuccess = false,
                            data = challengeInstanceAreRunning
                        });
                    }
                }
                else
                {
                    instanceList = new List<DeploymentInfo>();
                }

                #region Call to Challenge Hosting Platform to deploy to k8s
                var startRequest = new RestRequest();
                startRequest.Method = Method.Post;
                startRequest.Resource = "api/challenge/start";

                long unixTime = DateTimeHelper.GetDateTimeNowInUnix();

                var instanceInfoJson = JsonConvert.SerializeObject(instanceInfo);
                var DictScrKey = JsonConvert.DeserializeObject<Dictionary<string, string>>(instanceInfoJson);
                var DictMultiService = JsonConvert.DeserializeObject<Dictionary<string, object>>(instanceInfoJson);

                if (DictScrKey == null || DictMultiService == null)
                {
                    throw new Exception("Convert from obj instance info to dict failed");
                }

                if (!DictMultiService.ContainsKey("UnixTime"))
                {
                    DictMultiService.Add("UnixTime", unixTime);
                }

                string secretKeyStartChallenge = SecretKeyHelper.CreateSecretKey(unixTime, DictScrKey);

                startRequest.AddHeader("SecretKey", secretKeyStartChallenge);

                string baseDeployUrl = challengeHostServer.ServerHost + ":" + challengeHostServer.ServerPort;

                MultiServiceConnector connector = new MultiServiceConnector(baseDeployUrl);

                GenaralViewResponseData<DeploymentInfo>? startResult
                  = await connector.ExecuteRequest<GenaralViewResponseData<DeploymentInfo>>(startRequest, DictMultiService, RequestContentType.Form);

                await Console.Out.WriteLineAsync($"Returned for challenge: {instanceInfo.ChallengeId}");

                await Console.Out.WriteLineAsync($"startResult: {JsonConvert.SerializeObject(startResult)}");

                if (startResult == null || !startResult.IsSuccess || startResult.data == null)
                {
                    return BadRequest(startResult);
                }
                #endregion

                DeploymentInfo challengeInstance = startResult.data;

                instanceList?.Add(challengeInstance);
                await redisHelper.SetCacheAsync(redisInstanceKey, instanceList, TimeSpan.MaxValue);

                #region Commented Code
                //_ = Task.Run(async () =>
                //{
                //    if (challengeInstance.EndTime != null)
                //    {
                //        int delayInMinutes = (int)(challengeInstance.EndTime - DateTime.Now).Value.TotalMilliseconds;
                //        await Task.Delay(delayInMinutes);
                //        var instanceListAtNow = await redisHelper.GetFromCacheAsync<List<DeploymentInfo>>(redisInstanceKey);

                //        if (instanceListAtNow != null && instanceListAtNow.Count > 0)
                //        {
                //            var instance = instanceListAtNow.Where(p => p.ChallengeId == instanceInfo.ChallengeId && p.TeamId == instanceInfo.TeamId).ToList();
                //            instanceListAtNow.RemoveAll(p => p.ChallengeId == instanceInfo.ChallengeId && p.TeamId == instanceInfo.TeamId);
                //            await redisHelper.SetCacheAsync(redisInstanceKey, instanceListAtNow, TimeSpan.MaxValue);
                //        }
                //    }
                //});
                #endregion

                return Ok(new GenaralViewResponseData<string>
                {
                    Message = $"Start success challenge {instanceInfo.ChallengeId} for team {instanceInfo.TeamId}",
                    IsSuccess = true,
                    data = startResult.data.DeploymentDomainName,
                });

            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Error in Start Instance: " + ex.Message);
                if (instanceInfo.TeamId == -1)
                {
                    ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                }
                redisQueueCount--;
            }
            finally
            {
                await redisHelper.SetCacheAsync(RedisQueueDeployKey, redisQueueCount, TimeSpan.MaxValue);
            }

            return BadRequest(new GeneralView
            {
                Message = ErrorMessage,
                IsSuccess = false
            });
        }

        [HttpPost("stop")]
        public async Task<IActionResult> StopInstance([FromHeader] string SecretKey, [FromForm] StopChallengeInstanceRequest stopInstanceRequest)
        {
            int redisQueueCount = 0;
            // tao connection redis server 
            RedisHelper redisHelper = new RedisHelper(_connectionMultiplexer);
            string ErrorMessage = "Error while stopping, please try again in some seconds.";
            //Xóa bớt hàng chờ deploy (Cái này phục vụ mục đích nếu bấm start quá nhanh sẽ không xly kịp)
            string RedisQueueDeployKey = $"{RedisConfigs.RedisQueueDeployKey}{stopInstanceRequest.TeamId}";
            try
            {
                redisQueueCount = await redisHelper.GetFromCacheAsync<int>(RedisQueueDeployKey);
                if (redisQueueCount > 0)
                {
                    redisQueueCount--;
                }

                string redisDeployKey = $"{RedisConfigs.RedisDeployKey}{stopInstanceRequest.ChallengeId}";
                // check key redis deploy ton tai hay khong, get value redis deploy key, 
                var redisGetDeployInfo = await redisHelper.GetFromCacheAsync<DeploymentInfo>(redisDeployKey);
                if (redisGetDeployInfo == null || redisGetDeployInfo.LastDeployTime == null)
                {
                    return BadRequest(new GeneralView()
                    {
                        Message = $"Challenge {stopInstanceRequest.ChallengeId} is not yet deployed, can't stop",
                        IsSuccess = false
                    });
                }

                var challengeHostServer = ControlCenterServiceConfig.ChallengeServerInfoList.FirstOrDefault(c => c.ServerId == redisGetDeployInfo.ServerId);
                if (challengeHostServer == null) return BadRequest(new GeneralView()
                {
                    Message = "ChallengeHostServer is null, check config machine id",
                    IsSuccess = false
                });

                var stopRequest = new RestRequest();
                stopRequest.Method = Method.Post;
                stopRequest.Resource = "api/challenge/stop";

                long unixTime = DateTimeHelper.GetDateTimeNowInUnix();

                var instanceInfoJson = JsonConvert.SerializeObject(stopInstanceRequest);
                var DictScrKey = JsonConvert.DeserializeObject<Dictionary<string, string>>(instanceInfoJson);
                var DictMultiService = JsonConvert.DeserializeObject<Dictionary<string, object>>(instanceInfoJson);

                if (DictScrKey == null || DictMultiService == null)
                {
                    throw new Exception("Convert from obj instance info to dict failed");
                }

                if (!DictMultiService.ContainsKey("UnixTime"))
                {
                    DictMultiService.Add("UnixTime", unixTime);
                }

                string secretKeyStartChallenge = SecretKeyHelper.CreateSecretKey(unixTime, DictScrKey);

                stopRequest.AddHeader("SecretKey", secretKeyStartChallenge);

                string baseDeployUrl = challengeHostServer.ServerHost + ":" + challengeHostServer.ServerPort;
                MultiServiceConnector multiServiceDeployConnector = new MultiServiceConnector(baseDeployUrl);
                GeneralView? stopResult
                  = await multiServiceDeployConnector.ExecuteRequest<GeneralView>(stopRequest, DictMultiService, RequestContentType.Form);

                if (stopResult == null || !stopResult.IsSuccess)
                {
                    return BadRequest(stopResult);
                }

                string redisInstanceKey = $"{RedisConfigs.RedisChallengeInstanceListKey}";

                var instanceList = await redisHelper.GetFromCacheAsync<List<DeploymentInfo>>(redisInstanceKey);

                if (instanceList != null && instanceList.Count > 0)
                {
                    var instance = instanceList.Where(p => p.ChallengeId == stopInstanceRequest.ChallengeId && p.TeamId == stopInstanceRequest.TeamId).ToList();
                    instanceList.RemoveAll(p => p.ChallengeId == stopInstanceRequest.ChallengeId && p.TeamId == stopInstanceRequest.TeamId);
                    await redisHelper.SetCacheAsync(redisInstanceKey, instanceList, TimeSpan.MaxValue);
                }

                return Ok(stopResult);
            }
            catch (Exception ex)
            {
                redisQueueCount++;
                await Console.Out.WriteLineAsync("Error in Start Instance: " + ex.Message);
                if (stopInstanceRequest.TeamId == -1)
                {
                    ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                }
            }
            finally
            {
                await redisHelper.SetCacheAsync(RedisQueueDeployKey, redisQueueCount, TimeSpan.MaxValue);
            }

            return BadRequest(new GeneralView
            {
                Message = ErrorMessage,
                IsSuccess = false
            });
        }
    }
}
