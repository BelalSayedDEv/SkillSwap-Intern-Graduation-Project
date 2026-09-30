using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace SkillSwap.API.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                var policy = context.HttpContext.GetEndpoint()?.Metadata
                    .GetMetadata<EnableRateLimitingAttribute>()?.PolicyName?.ToLowerInvariant();

                var (detail, retryAfter) = policy switch
                {
                    "login" => ("Too many login attempts. 5 per minute. Try again shortly.", "60"),
                    "register" => ("Too many accounts from this network. 3 per hour.", "3600"),
                    "refresh" => ("Too many token refreshes. Slow down.", "15"),
                    "logout" => ("Too many logouts. Try again shortly.", "60"),
                    "revoke-family" => ("Too many revocations. 3 per day. Try again tomorrow.", "86400"),
                    _ => ("Too many requests. Server busy. Try again shortly.", "5")
                };

                context.HttpContext.Response.Headers.RetryAfter = retryAfter;
                context.HttpContext.Response.ContentType = "application/problem+json";
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    title = "Too many requests.",
                    status = 429,
                    detail,
                    traceId = context.HttpContext.TraceIdentifier
                }, cancellationToken);
            };

            options.AddPolicy("login", httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy("register", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromHours(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy("refresh", httpContext =>
                RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 20,
                        TokensPerPeriod = 5,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(15),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy("logout", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy("revoke-family", httpContext =>
                RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: httpContext.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                        ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 3,
                        TokensPerPeriod = 1,
                        ReplenishmentPeriod = TimeSpan.FromDays(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetConcurrencyLimiter(
                    partitionKey: "global-cpu",
                    factory: _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = 50,
                        QueueLimit = 0
                    }));
        });

        return services;
    }
}
