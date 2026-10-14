using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Amazon.S3;
using FileService.Contracts;
using FileService.Contracts.Dto;
using FileService.Domain.Assets;
using FileService.Infrastructure.Postgres;
using Framework.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CompleteUploadRequest = FileService.Contracts.Dto.CompleteMultipartUploadRequest;
using CompleteUploadResponse = FileService.Contracts.Dto.CompleteMultipartUploadResponse;

namespace FileService.IntegrationTests.Infrastructure;

[Collection(FileServiceTestCollection.Name)]
public abstract class FileServiceBaseTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly FileServiceTestWebFactory _factory;

    protected FileServiceBaseTests(FileServiceTestWebFactory factory)
    {
        _factory = factory;
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", factory.CreateTestAccessToken());
    }

    protected HttpClient Client { get; }

    protected IServiceProvider Services => _factory.Services;

    protected IAmazonS3 S3Client => _factory.Services.GetRequiredService<IAmazonS3>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.ResetStateAsync();
    }

    protected async Task<T?> GetAssetAsync<T>(Guid id, Func<IQueryable<MediaAsset>, IQueryable<T>> selector)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
        return await selector(dbContext.MediaAssets.Where(a => a.Id == id)).FirstOrDefaultAsync();
    }

    protected async Task<bool> AssetExistsAsync(Guid id)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
        return await dbContext.MediaAssets.AnyAsync(a => a.Id == id);
    }

    protected async Task<T> ReadOkEnvelopeAsync<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<T>>(JsonOptions);
        Assert.NotNull(envelope);
        Assert.False(envelope.IsError);
        Assert.NotNull(envelope.Result);

        return envelope.Result;
    }

    protected static async Task AssertErrorStatusAsync(HttpResponseMessage response, HttpStatusCode expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("errorsList", body, StringComparison.OrdinalIgnoreCase);
    }

    protected async Task<bool> ObjectExistsAsync(string bucketName, string key)
    {
        try
        {
            await S3Client.GetObjectMetadataAsync(bucketName, key);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    protected async Task<long> GetObjectSizeAsync(string bucketName, string key)
    {
        var metadata = await S3Client.GetObjectMetadataAsync(bucketName, key);
        return metadata.ContentLength;
    }

    protected async Task<Guid> UploadOnePartAssetAsync(string fileName, string content)
    {
        var payload = Encoding.UTF8.GetBytes(content);
        var started = await StartMultipartUploadAsync(fileName, "preview", "image/png", payload.Length);
        var eTag = await PutPartAsync(started.ChunkUrls.Single().UploadUrl, payload);
        var completed = await CompleteMultipartUploadAsync(started.MediaAssetId, started.UploadId, [new PartETagDto(1, eTag)]);
        return completed.MediaAssetId;
    }

    protected async Task<StartMultipartUploadResponse> StartMultipartUploadAsync(
        string fileName,
        string assetType,
        string contentType,
        long size)
    {
        var request = new StartMultipartUploadRequest(fileName, assetType, contentType, size, "location", Guid.NewGuid());
        var response = await Client.PostAsJsonAsync("/api/files/multipart/start", request);
        return await ReadOkEnvelopeAsync<StartMultipartUploadResponse>(response);
    }

    protected static async Task<string> PutPartAsync(string uploadUrl, byte[] payload)
    {
        using var client = new HttpClient();
        using var content = new ByteArrayContent(payload);
        var response = await client.PutAsync(uploadUrl, content);
        response.EnsureSuccessStatusCode();

        Assert.True(response.Headers.ETag is not null || response.Headers.TryGetValues("ETag", out _));
        return response.Headers.ETag?.Tag
               ?? response.Headers.GetValues("ETag").Single();
    }

    protected async Task<CompleteUploadResponse> CompleteMultipartUploadAsync(
        Guid mediaAssetId,
        string uploadId,
        List<PartETagDto> partETags)
    {
        var response = await Client.PostAsJsonAsync(
            "/api/files/multipart/complete",
            new CompleteUploadRequest(mediaAssetId, uploadId, partETags));
        return await ReadOkEnvelopeAsync<CompleteUploadResponse>(response);
    }
}
