using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;

namespace GoPlaces.Tests;

// Esta implementación reemplaza a la oficial SOLO en el entorno de tests.
// Básicamente “apaga” la autorización.
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IMethodInvocationAuthorizationService))]
public class TestMethodInvocationAuthorizationService
    : IMethodInvocationAuthorizationService, ITransientDependency
{
    public Task CheckAsync(MethodInvocationAuthorizationContext context)
    {
        // No hacemos ninguna validación de permisos: siempre permite.
        return Task.CompletedTask;
    }
}

