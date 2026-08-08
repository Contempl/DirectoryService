using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;

namespace FileService.Contracts.HttpCommunication;

public static class FileServiceExtensions
{
    public static IServiceCollection AddFileServiceHttpCommunication(this IServiceCollection services,
        IConfiguration configuration)
    {
        var optionsSection = configuration.GetSection(nameof(FileServiceOptions));
        var fileServiceOptions = optionsSection.Get<FileServiceOptions>() ?? new FileServiceOptions();

        services
            .AddOptions<FileServiceOptions>()
            .Bind(optionsSection)
            .Validate(
                options => Uri.TryCreate(options.Url, UriKind.Absolute, out var uri)
                           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
                $"{nameof(FileServiceOptions)}:{nameof(FileServiceOptions.Url)} must be an absolute HTTP(S) URL.")
            .Validate(
                options => options.TimeoutSeconds > 3,
                $"{nameof(FileServiceOptions)}:{nameof(FileServiceOptions.TimeoutSeconds)} must be greater than the 3-second attempt timeout.")
            .Validate(
                options => options.RetryCount >= 1,
                $"{nameof(FileServiceOptions)}:{nameof(FileServiceOptions.RetryCount)} must be at least 1.")
            .Validate(
                options => options.RetryDelayMilliseconds >= 0,
                $"{nameof(FileServiceOptions)}:{nameof(FileServiceOptions.RetryDelayMilliseconds)} cannot be negative.")
            .Validate(
                options => options.CircuitSamplingSeconds > 0,
                $"{nameof(FileServiceOptions)}:{nameof(FileServiceOptions.CircuitSamplingSeconds)} must be greater than zero.")
            .Validate(
                options => options.CircuitFailureRatio is > 0 and <= 1,
                $"{nameof(FileServiceOptions)}:{nameof(FileServiceOptions.CircuitFailureRatio)} must be in the range (0, 1].")
            .Validate(
                options => options.CircuitMinimumThroughput >= 2,
                $"{nameof(FileServiceOptions)}:{nameof(FileServiceOptions.CircuitMinimumThroughput)} must be at least 2.")
            .Validate(
                options => options.CircuitBreakSeconds > 0,
                $"{nameof(FileServiceOptions)}:{nameof(FileServiceOptions.CircuitBreakSeconds)} must be greater than zero.")
            .ValidateOnStart();

        services.AddHttpClient<IFileCommunicationService, FileHttpClient>((sp, config) =>
        {
            var fileOptions = sp.GetRequiredService<IOptions<FileServiceOptions>>().Value;

            config.BaseAddress = new Uri(fileOptions.Url);
            config.Timeout = Timeout.InfiniteTimeSpan;
        })
            .AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = fileServiceOptions.RetryCount;
            options.Retry.Delay = TimeSpan.FromMilliseconds(fileServiceOptions.RetryDelayMilliseconds);
            options.Retry.BackoffType = DelayBackoffType.Exponential;
            options.Retry.UseJitter = true;

            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);
            options.TotalRequestTimeout.Timeout =
                TimeSpan.FromSeconds(fileServiceOptions.TimeoutSeconds);

            options.CircuitBreaker.SamplingDuration =
                TimeSpan.FromSeconds(fileServiceOptions.CircuitSamplingSeconds);

            options.CircuitBreaker.FailureRatio = fileServiceOptions.CircuitFailureRatio;
            options.CircuitBreaker.MinimumThroughput = fileServiceOptions.CircuitMinimumThroughput;
            options.CircuitBreaker.BreakDuration =
                TimeSpan.FromSeconds(fileServiceOptions.CircuitBreakSeconds);
        });
        
        return services;
    }
}
