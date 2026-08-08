using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Shared.Kernel;

namespace FileService.Contracts.HttpCommunication;

public class FileHttpClient : IFileCommunicationService
{
    private readonly HttpClient _httpClient; 
    private readonly ILogger<FileHttpClient> _logger;
    
    public FileHttpClient(HttpClient httpClient, ILogger<FileHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<MediaAssetInfoResponse, Error>> GetMediaAsset(
        Guid mediaAssetId,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(
                $"api/files/{mediaAssetId}",
                cancellationToken);

            return await response.HandleResponseAsync<MediaAssetInfoResponse>(
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return HandleUnavailable(ex, mediaAssetId);
        }
    }

    public async Task<Result<GetMediaAssetsInfoResponse, Error>> GetMediaAssets(GetMediaAssetsInfoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/files/batch",
                request,
                cancellationToken);

            return await response.HandleResponseAsync<GetMediaAssetsInfoResponse>(
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return HandleUnavailable(ex, request.MediaAssetIds);
        }
    }

    public async Task<Result<GetDownloadUrlResponse, Error>> GetDownloadUrl(
        GetDownloadUrlRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/files/url",
                request,
                cancellationToken);

            return await response.HandleResponseAsync<GetDownloadUrlResponse>(
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return HandleUnavailable(ex, request.MediaAssetId);
        }
    }

    private Error HandleUnavailable(Exception exception, object requestData)
    {
        _logger.LogError(
            exception,
            "File Service request failed for {RequestData}",
            requestData);

        return Error.Failure(
            "file-service.unavailable",
            "File Service is temporarily unavailable.");
    }
}