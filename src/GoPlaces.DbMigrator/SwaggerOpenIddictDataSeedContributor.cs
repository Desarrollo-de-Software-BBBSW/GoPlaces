using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace GoPlaces.DbMigrator;

public class SwaggerOpenIddictDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IOpenIddictApplicationManager _appManager;
    private readonly IOpenIddictScopeManager _scopeManager;

    public SwaggerOpenIddictDataSeedContributor(
        IOpenIddictApplicationManager appManager,
        IOpenIddictScopeManager scopeManager)
    {
        _appManager = appManager;
        _scopeManager = scopeManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // 1) Scope para tu API (GoPlaces)
        if (await _scopeManager.FindByNameAsync("GoPlaces") is null)
        {
            await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "GoPlaces",
                DisplayName = "GoPlaces API"
                // Resources opcional si manejás API resources
            });
        }

        // 2) Cliente para Swagger (authorization_code + PKCE)
        if (await _appManager.FindByClientIdAsync("GoPlaces_Swagger") is null)
        {
            await _appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "GoPlaces_Swagger",
                DisplayName = "Swagger UI",
                ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                ClientType = OpenIddictConstants.ClientTypes.Public, // público = sin client_secret

                RedirectUris =
                {
                    new Uri("https://localhost:44300/swagger/oauth2-redirect.html")
                },

                Permissions =
                {
                    // Endpoints
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,

                    // Grant/response types
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,

                    // Scopes estándar + tu scope de API
                    OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "GoPlaces"
                },

                Requirements =
                {
                    // PKCE requerido para clientes públicos
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                }
            });
        }
    }
}
