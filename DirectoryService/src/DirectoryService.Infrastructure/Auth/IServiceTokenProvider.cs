namespace DirectoryService.Infrastructure.Auth;

internal interface IServiceTokenProvider
{
    ValueTask<string> GetTokenAsync(
        CancellationToken cancellationToken);
}
