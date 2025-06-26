using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using TrueDetectWebAPI.Interfaces;
using TrueDetectWebAPI.Services;
using TrueDetectWebAPI.Services.Redis;
using TrueDetectWebAPI.Controllers;

namespace TrueDetectWebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IRedisBaseService, RedisBaseService>()
                .AddSingleton<ITaggingRedisService, TaggingRedisService>()
                .AddSingleton<IRetrainingRedisService, RetrainingRedisService>()
                .AddSingleton<IFetchForRetrainingService, FetchForRetrainingService>()
                .AddSingleton<StreamToClientController>()
                .AddHostedService<ScheduleChecker>()
                .AddSingleton<RetrainingContextQueue>();
        }
    }
}