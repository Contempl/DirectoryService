using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthService.Configuration;

public sealed class OpenIddictSeedService(
    IServiceProvider serviceProvider,
    IOptions<OpenIddictClientsOptions> clientOptions,
    IOptions<AuthorizationServerOptions> serverOptions)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var scopeManager = scope.ServiceProvider
            .GetRequiredService<IOpenIddictScopeManager>();
        var applicationManager = scope.ServiceProvider
            .GetRequiredService<IOpenIddictApplicationManager>();

        await EnsureApiScopeAsync(scopeManager, cancellationToken);
        await EnsureFrontendClientAsync(applicationManager, cancellationToken);
        await EnsureDirectoryServiceClientAsync(
            applicationManager,
            cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    private async Task EnsureApiScopeAsync(
        IOpenIddictScopeManager manager,
        CancellationToken cancellationToken)
    {
        var name = serverOptions.Value.ApiScope;
        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = name,
            DisplayName = "Directory API"
        };
        descriptor.Resources.Add(name);

        var existing = await manager.FindByNameAsync(name, cancellationToken);
        if (existing is null)
            await manager.CreateAsync(descriptor, cancellationToken);
        else
            await manager.UpdateAsync(existing, descriptor, cancellationToken);
    }

    private async Task EnsureFrontendClientAsync(
        IOpenIddictApplicationManager manager,
        CancellationToken cancellationToken)
    {
        var client = clientOptions.Value.Frontend;
        var descriptor = CreateBaseDescriptor(client);
        descriptor.ClientType = ClientTypes.Confidential;
        descriptor.ConsentType = ConsentTypes.Implicit;
        descriptor.RedirectUris.Add(new Uri(client.RedirectUri!, UriKind.Absolute));

        descriptor.Permissions.UnionWith(
        [
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.GrantTypes.AuthorizationCode,
            Permissions.GrantTypes.RefreshToken,
            Permissions.ResponseTypes.Code,
            Permissions.Scopes.Email,
            Permissions.Scopes.Profile,
            Permissions.Scopes.Roles,
            Permissions.Prefixes.Scope + serverOptions.Value.ApiScope
        ]);

        await UpsertAsync(manager, descriptor, cancellationToken);
    }

    private async Task EnsureDirectoryServiceClientAsync(
        IOpenIddictApplicationManager manager,
        CancellationToken cancellationToken)
    {
        var descriptor = CreateBaseDescriptor(
            clientOptions.Value.DirectoryService);
        descriptor.ClientType = ClientTypes.Confidential;
        descriptor.ConsentType = ConsentTypes.Implicit;
        descriptor.Permissions.UnionWith(
        [
            Permissions.Endpoints.Token,
            Permissions.GrantTypes.ClientCredentials,
            Permissions.Prefixes.Scope + serverOptions.Value.ApiScope
        ]);

        await UpsertAsync(manager, descriptor, cancellationToken);
    }

    private static OpenIddictApplicationDescriptor CreateBaseDescriptor(
        OpenIddictClientOptions client)
    {
        return new OpenIddictApplicationDescriptor
        {
            ClientId = client.ClientId,
            ClientSecret = client.ClientSecret,
            DisplayName = client.DisplayName
        };
    }

    private static async Task UpsertAsync(
        IOpenIddictApplicationManager manager,
        OpenIddictApplicationDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        var existing = await manager.FindByClientIdAsync(
            descriptor.ClientId!,
            cancellationToken);

        if (existing is null)
            await manager.CreateAsync(descriptor, cancellationToken);
        else
            await manager.UpdateAsync(existing, descriptor, cancellationToken);
    }
}
