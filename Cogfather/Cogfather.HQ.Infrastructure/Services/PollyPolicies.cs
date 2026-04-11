using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client.Exceptions;

namespace Cogfather.HQ.Infrastructure.Services;

public static class PollyPolicies
{
    public static AsyncRetryPolicy GetRetryPolicy(ILogger logger)
    {
        return Policy
            .Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(exception, "RabbitMQ connection failed. Retry {RetryCount} after {TimeSpan}s",
                        retryCount, timeSpan.TotalSeconds);
                });
    }
}