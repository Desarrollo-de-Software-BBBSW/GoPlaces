# CLAUDE.md — GoPlaces

Documento de contexto persistente para sesiones de trabajo con Claude Code.

---

## 1. Resumen del proyecto

**GoPlaces** es una plataforma web de búsqueda y seguimiento de destinos turísticos desarrollada como proyecto académico de la materia Desarrollo de Software.

Permite a los usuarios buscar ciudades/destinos vía APIs externas, guardarlos en listas personales de favoritos, calificarlos, registrar experiencias, y recibir notificaciones cuando hay cambios relevantes en sus destinos guardados.

Hay dos roles: **Usuario** (búsqueda y seguimiento) y **Administrador** (gestión de usuarios, métricas de uso de API externa, monitoreo).

El proyecto es desarrollado por un equipo de al menos 3 personas (Agustin Benedetti, Thiago, LucasBre55).

---

## 2. Stack tecnológico

### Backend
| Tecnología | Versión | Uso |
|---|---|---|
| .NET | 9.0 | Runtime |
| ASP.NET Core | 9.0 | HTTP API |
| ABP.IO Framework | 9.x | Scaffolding, DDD, módulos |
| Entity Framework Core | 9.0 | ORM |
| OpenIddict | — | OAuth2 / servidor de autenticación |
| SQL Server (SQLEXPRESS) | local | Base de datos |
| Serilog | — | Logging |
| Autofac | — | IoC container |
| Moq + Shouldly + Xunit | — | Testing |

### Frontend
| Tecnología | Versión | Uso |
|---|---|---|
| Angular | 20.0.0 | SPA framework |
| ABP Angular modules | 9.3.2 | UI base, autenticación, identidad |
| Lepton-X Lite | — | Tema visual ABP |
| RxJS | 7.8.0 | Observables / async |
| ngx-datatable | 22.0.0 | Tablas de datos |
| Jasmine / Karma | — | Testing frontend |
| ESLint + TypeScript-ESLint | — | Linting |

### APIs externas
- **GeoDB Cities** (via RapidAPI): búsqueda de ciudades por nombre, país, región, población
- **TicketMaster Discovery API** (oficial, directa — **no** vía RapidAPI): eventos en destinos. A diferencia de GeoDB, esto fue una decisión consciente: no se encontró un wrapper de RapidAPI confiable/estable para TicketMaster, así que se integra contra `app.ticketmaster.com/discovery/v2` con su propia API key (`TicketMaster:ApiKey`, separada de `RapidApi:ApiKey`)

---

## 3. Estructura del proyecto

```
GoPlaces/
├── GoPlaces.sln                        # Solución .NET con 10 proyectos
├── README.md
├── common.props                        # Props compartidas: <TargetFramework>net9.0</TargetFramework>, Nullable enable
│
├── src/                                # Backend — capas de la aplicación
│   ├── GoPlaces.Domain/                # Entidades, Value Objects, Domain Services
│   ├── GoPlaces.Domain.Shared/         # Constantes, enums, localizaciones compartidas
│   ├── GoPlaces.Application.Contracts/ # Interfaces de servicios (IXxxAppService), DTOs
│   ├── GoPlaces.Application/           # Implementación de servicios de aplicación
│   │   └── BackgroundWorkers/          # Workers periódicos de ABP (ej: EventSyncBackgroundWorker)
│   ├── GoPlaces.EntityFrameworkCore/   # DbContext, migraciones, configuración EF Core
│   ├── GoPlaces.HttpApi/               # Controller base (GoPlacesController)
│   ├── GoPlaces.HttpApi.Host/          # Host principal: Program.cs, appsettings, Swagger
│   ├── GoPlaces.HttpApi.Client/        # Cliente HTTP tipado (consume la API)
│   └── GoPlaces.DbMigrator/            # Ejecuta migraciones + datos semilla
│
├── test/                               # Tests
│   ├── GoPlaces.TestBase/              # Clases base compartidas
│   ├── GoPlaces.Domain.Tests/          # Tests de entidades y Domain Services
│   ├── GoPlaces.Application.Tests/     # Tests de Application Services (con Moq)
│   ├── GoPlaces.EntityFrameworkCore.Tests/ # Tests de persistencia
│   └── GoPlaces.HttpApi.Client.ConsoleTestApp/ # App de consola para probar el cliente HTTP
│
└── angular/                            # Frontend Angular
    └── src/
        ├── app/
        │   ├── home/                   # Landing page con búsqueda y destinos populares
        │   ├── pages/
        │   │   ├── cities-search/      # Búsqueda avanzada de ciudades (filtros)
        │   │   ├── city-detail/        # Detalle de una ciudad
        │   │   ├── login/              # Autenticación manual
        │   │   ├── register/           # Registro de usuario
        │   │   └── my-profile/         # Perfil, cambio de contraseña, eliminar cuenta
        │   ├── public-profile/         # Perfil público de otro usuario
        │   ├── proxy/                  # Servicios HTTP generados por ABP (NO editar a mano)
        │   │   ├── cities/
        │   │   ├── destinations/
        │   │   ├── experiences/
        │   │   ├── notifications/
        │   │   ├── ratings/
        │   │   └── users/
        │   ├── services/               # Servicios Angular custom (no generados)
        │   ├── shared/                 # Componentes reutilizables
        │   │   ├── notification-bell/      # Campana de notificaciones (dropdown, badge, polling)
        │   │   └── relative-time.pipe.ts   # Pipe de fecha relativa ("hace 2 horas")
        │   ├── app.routes.ts           # Routing principal
        │   ├── app.config.ts
        │   └── auth.interceptor.ts     # Interceptor OAuth2
        ├── environments/               # environment.ts / environment.prod.ts
        ├── assets/
        └── styles.scss                 # Estilos globales
```

### Módulos de negocio en `src/GoPlaces.Domain/`

| Carpeta | Entidad principal | Descripción |
|---|---|---|
| `Destinations/` | `Destination` | Destino turístico (nombre, país, población, coordenadas, imagen) |
| `Cities/` | — | Domain service de búsqueda y validación de filtros |
| `Ratings/` | `Rating` | Calificación 1-5 con comentario |
| `Experiences/` | `Experience` | Experiencia en un destino (título, precio, fecha, sentimiento) |
| `Notifications/` | `Notification` | Alerta de cambio en destino favorito. Incluye `DestinationNotificationDomainService`, un Domain Service (sin `[Authorize]`) que contiene la lógica real de notificar a los seguidores de un destino, separado de `NotificationAppService` porque este último tiene `[Authorize]` de clase y no puede ser invocado desde código sin usuario autenticado (ej: un Background Worker) |
| `Follow/` | `FollowList`, `FollowListItem` | Lista personal de favoritos |
| `ExternalApiMetrics/` | `ExternalApiCall` | Tracking de llamadas a APIs externas |
| `Events/` | `Event` | Evento de TicketMaster asociado a un `Destination` (nombre, fecha, venue, `TicketMasterId` para evitar duplicados al sincronizar) |

---

## 4. Convenciones de código

### Backend (C#)
- **Naming:** PascalCase para todo (clases, métodos, propiedades). Prefijo `I` para interfaces.
- **DTOs:** Sufijo `Dto` (ej: `DestinationDto`, `CitySearchRequestDto`)
- **Interfaces de servicio:** Prefijo `I` + nombre + `AppService` (ej: `ICityAppService`)
- **Tests:** Nombre en español descriptivo del escenario, separados por guiones bajos (ej: `SearchCitiesAsync_devuelve_resultados_del_servicio`)
- **Entidades:** Heredan de `FullAuditedAggregateRoot<Guid>` (ABP), propiedades `private set`, setters explícitos con validación mediante `Check.NotNullOrWhiteSpace`
- **Migraciones EF:** Nombre en inglés descriptivo (ej: `Added_Experience_Entity`, `Added_Rating_To_Experience`)
- **Background Workers:** Viven en `GoPlaces.Application/BackgroundWorkers/`, heredan de `AsyncPeriodicBackgroundWorkerBase` (ABP). La lógica real va en un método público separado de `DoWorkAsync` (ej: `SyncFollowedDestinationsAsync`), que recibe el `IServiceProvider` como parámetro — así se puede invocar directo desde los tests sin depender del timer real. **Importante:** a diferencia de los Application Services, los Background Workers **no** tienen Unit of Work ambiente automático. Cualquier acceso a repositorios dentro de un worker necesita un `IUnitOfWorkManager.Begin()` explícito — si no, tirás `ObjectDisposedException` en runtime (el DbContext efímero de una llamada se cierra antes de que otra llamada materialice su query), y los tests con mocks de Moq no lo detectan porque un `IQueryable` mockeado es LINQ-to-Objects puro, sin ningún concepto de contexto disponible/disponible.

### Frontend (Angular/TypeScript)
- **Componentes:** PascalCase para clase, kebab-case para selector y archivo (ej: `CitiesSearchComponent`, archivo `cities-search.ts`)
- **Servicios proxy:** En `angular/src/app/proxy/` — generados automáticamente por ABP, **no editar manualmente**
- **Servicios custom:** En `angular/src/app/services/`
- **Formularios:** Reactive Forms de Angular (`FormBuilder`, `FormGroup`)
- **HTTP async:** RxJS Observables con `.subscribe({ next, error })`
- **Guards:** `AuthGuard` y `PermissionGuard` de ABP para rutas protegidas
- **Polling con RxJS (`interval` + `switchMap` + `.subscribe()`):** patrón usado en `NotificationBellComponent` para refrescar el contador de no leídas cada 45s. Los componentes raíz de Lepton-X Lite (`lpx-layout`, `lpx-toolbar`) usan `ChangeDetectionStrategy.OnPush`, así que cualquier componente propio insertado en esa barra (vía `NavItemsService`) que actualice su estado desde un origen async que **no** sea un `@Input` que cambia ni un evento DOM disparado dentro de ese mismo árbol (polling, websockets, etc.) necesita llamar `ChangeDetectorRef.markForCheck()` explícitamente en el callback del `.subscribe()` — si no, la vista no se repinta sola hasta que ocurra otro evento DOM en ese árbol (ver gotcha 13).

### Commits
- Sin formato convencional estricto (no hay Conventional Commits)
- Idioma mixto (español/inglés): predomina el español para descripciones
- Ramas: `feature/<número>-<descripción-kebab>`, ej: `feature/32--frontend-home`
- PRs numeradas secuencialmente (#25 en adelante)
- Mensajes frecuentes en rama: `back y pruebas`, `arreglo de ruta X`, `cambios front-home`

---

## 5. Arquitectura

Arquitectura en capas DDD estricta, implementada con ABP.IO:

```
Angular SPA (puerto 4200)
    │  OAuth2 / Bearer Token
    ▼
ASP.NET Core HttpApi.Host (puerto 44300)
    │  ABP Application Services
    ├── Application Layer  →  lógica de casos de uso
    ├── Domain Layer       →  entidades, reglas de negocio, domain services
    ├── EF Core Layer      →  DbContext + repositorios ABP
    │       │
    │       └── SQL Server (SQLEXPRESS, base GoPlaces)
    └── External APIs
            ├── GeoDB Cities (RapidAPI) — búsqueda de ciudades
            └── TicketMaster (RapidAPI) — eventos
```

### Decisiones arquitectónicas relevantes

1. **ABP.IO como base:** Provee scaffolding de módulos, autenticación (OpenIddict), repositorios genéricos, auditoría automática, localización y el módulo de identidad de usuarios. Muchas cosas que en otro proyecto serían custom aquí son automáticas.

2. **Servicios proxy Angular generados:** Los archivos en `angular/src/app/proxy/` son auto-generados por el CLI de ABP desde las interfaces `IXxxAppService`. Modificarlos a mano hace que se sobreescriban. Si se cambia un DTO o interfaz en el backend, hay que regenerar los proxies.

3. **Tracking de métricas de API:** `ExternalApiCall` registra cada llamada a GeoDB (endpoint, tiempo de respuesta, éxito/error). Visible solo para admins.

4. **Notificaciones por cambio en favoritos:** Cuando un destino cambia, `NotificationAppService.NotifyDestinationChangeAsync` busca todas las `FollowList` que contienen ese destino y crea una `Notification` por usuario afectado.

5. **Validación de dominio:** La validación de filtros de búsqueda (ej: mínimo 3 caracteres, rangos de población) vive en `CitySearchDomainService`, no en el controller ni en el Application Service directamente.

6. **Sin multi-tenancy activo:** El módulo de tenant-management está comentado en `app.routes.ts`.

7. **Sincronización periódica de eventos (`EventSyncBackgroundWorker`):** un Background Worker de ABP corre cada X tiempo (configurable, default 12hs — ver sección 10) y, para cada `Destination` que tenga al menos un usuario siguiéndolo, sincroniza sus eventos de TicketMaster (reusando `EventAppService.SearchEventsByCityAsync`) y dispara notificaciones si aparecen eventos nuevos. La lógica de notificar vive en `DestinationNotificationDomainService` (Domain Service), **no** en `NotificationAppService` (Application Service): este último tiene `[Authorize]` de clase, y el worker corre sin ningún usuario autenticado en el scope — invocar un método `[Authorize]` desde ahí tira `AbpAuthorizationException`. El Domain Service no tiene ese problema porque los Domain Services de ABP no llevan autorización HTTP por diseño.

8. **Marcar todas las notificaciones como leídas (`MarkAllAsReadAsync`):** agregado a `INotificationAppService`/`NotificationAppService` en `feature/notifications-frontend` — no existía antes de esta rama (antes solo se podía marcar de a una vía `ChangeReadStateAsync`). Busca las notificaciones no leídas del usuario actual y las actualiza con `UpdateManyAsync`. Se agregó junto con el frontend de notificaciones porque el dropdown necesitaba un botón de "marcar todas como leídas".

---

## 6. Sistema de diseño / UI

- **Tema base:** Lepton-X Lite (ABP), cargado via `angular.json` en los styles
- **Estilos globales:** `angular/src/styles.scss`
- **Íconos:** FontAwesome (incluido en angular.json)
- **Tablas:** `ngx-datatable` para listados de datos
- **Formularios:** Angular Reactive Forms + clases de Lepton-X para inputs y feedback visual
- **Toaster:** ABP Toaster service para mensajes de error/éxito (`this.toaster.error(...)`, `this.toaster.success(...)`)
- **No hay design system custom** documentado; se usan las clases de Lepton-X directamente
- **Notificaciones (campana + dropdown):** `NotificationBellComponent` (`angular/src/app/shared/notification-bell/`), registrado en la barra superior vía `NavItemsService` (mismo mecanismo que ya usaba `UserSearchComponent`). Usa clases de Bootstrap/Lepton-X ya presentes en el proyecto (`btn`, `badge`, etc.), sin introducir estilos custom nuevos.

---

## 7. Testing

### Backend
**Framework:** Xunit + Moq + Shouldly

**Correr tests:**
```bash
dotnet test
```

**Organización:**
- `test/GoPlaces.Application.Tests/` — Tests de Application Services usando Moq para mockear repositorios y servicios externos
- `test/GoPlaces.Domain.Tests/` — Tests de entidades de dominio y Domain Services
- `test/GoPlaces.EntityFrameworkCore.Tests/` — Tests de persistencia (requieren DB)

**Ejemplo de test (patrón):**
```csharp
// Nombre en español, describe el escenario exacto
[Fact]
public async Task SearchCitiesAsync_devuelve_resultados_del_servicio()
{
    // Arrange: setup Moq mocks
    // Act: llamar al servicio
    // Assert: usar Shouldly (result.ShouldNotBeNull())
}
```

**Cobertura actual:** Application Services (Cities, Destinations, Experiences, Ratings, Notifications, Follow, ExternalApiMetrics, Events) y Domain Services.

**Tests de integración vs. unitarios:** los tests que pegan contra una API externa real (no mockeada) se marcan con `[Trait("Category", "Integration")]` — convención introducida en `feature/ticketmaster-integration` (ver `TicketMasterEventSearchService_IntegrationTests`). `CitySearchService_IntegrationTests` (que llama a GeoDB real) le faltaba este Trait y corría siempre, incluso con el filtro `Category!=Integration`; esto se corrigió en `feature/events-background-worker` porque sus reintentos con backoff contra GeoDB podían tardar varios minutos y hacían parecer colgada a la corrida completa de la suite. Ahora está taggeado igual que el resto. Para excluirlos de la corrida normal:
```bash
dotnet test --filter "Category!=Integration"
```
Para correr solo los de integración:
```bash
dotnet test --filter "Category=Integration"
```
Los tests de integración de TicketMaster requieren una `TicketMaster:ApiKey` real configurada localmente (`appsettings.secrets.json` del proyecto de tests); si no está configurada, esos casos se omiten silenciosamente (no fallan) en vez de requerir la key.

**Tests contra un DbContext real (SQLite in-memory):** además de los tests con Moq (repos/servicios mockeados), hay precedente de tests que resuelven el contenedor de DI real de `GoPlacesApplicationTestModule` (el mismo Sqlite in-memory que usan `FollowAppService_Tests`/`NotificationAppService_Tests`) para ejercitar código contra un `DbContext` de verdad — ver `EventSyncBackgroundWorker_RealDbContextTests.cs`. Son más lentos que los tests con Moq, pero son los únicos que detectan bugs de lifetime de EF Core (ej: `ObjectDisposedException` por falta de Unit of Work — ver gotcha 12) que un `IQueryable` mockeado nunca va a reproducir.

### Frontend
**Framework:** Jasmine + Karma

**Correr tests:**
```bash
cd angular
npm test
```

Los tests de Angular son los generados por defecto por ABP/Angular CLI; no hay cobertura custom significativa en el frontend.

---

## 8. Convenciones de commits y PRs

- **Ramas:** `feature/<número-issue>-<descripción>` (ej: `feature/32--frontend-home`)
- **Rama fix:** `fix/<número-issue>-<descripción>` (ej: `fix/33-34`, rama actual)
- **Mensajes de commit:** Texto libre en español, sin formato estricto
  - Descriptivos del qué: `"Notificar sobre cambios relevantes en destinos"`
  - A veces con sufijos del scope: `"(back-test)"`, `"(back),(solo-admin)"`
  - Correcciones: `"arreglo de ruta my-profile"`, `"reparo-pruebas"`
- **PRs:** Se mergean a `main` con PR numerada. El título sigue el nombre de la feature del issue.
- **Base branch:** `main`
- **Organización GitHub:** `Desarrollo-de-Software-BBBSW`

---

## 9. Comandos útiles

### Setup inicial

```bash
# 1. Instalar dependencias frontend
cd angular && npm install

# 2. Restaurar paquetes .NET
dotnet restore

# 3. Aplicar migraciones (requiere SQL Server corriendo)
cd src/GoPlaces.DbMigrator
dotnet run
```

### Desarrollo

```bash
# Backend (desde raíz)
cd src/GoPlaces.HttpApi.Host
dotnet run
# Corre en https://localhost:44300
# Swagger en https://localhost:44300/swagger

# Frontend (desde /angular)
cd angular
npm start
# Corre en http://localhost:4200
```

### Build

```bash
# Backend
dotnet build

# Frontend
cd angular
npm run build
```

### Tests

```bash
# Backend (todos los proyectos de test)
dotnet test

# Frontend
cd angular
npm test
```

### Lint

```bash
cd angular
npm run lint
```

---

## 10. Variables de entorno y configuración

### Backend — `src/GoPlaces.HttpApi.Host/appsettings.json`

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost\\SQLEXPRESS;Database=GoPlaces;..."
  },
  "RapidApi": {
    "ApiKey": "<tu-key-de-rapidapi>"
  },
  "TicketMaster": {
    "BaseUrl": "https://app.ticketmaster.com/discovery/v2/",
    "ApiKey": "<tu-key-de-ticketmaster-discovery-api>"
  },
  "AuthServer": {
    "Authority": "https://localhost:44300"
  },
  "EventSyncWorker": {
    "IntervalHours": 12
  }
}
```

Valores que hay que configurar localmente (no están en el repo):
- `ConnectionStrings:Default` — cadena de conexión a SQL Server local
- `RapidApi:ApiKey` — key de RapidAPI, usada solo por GeoDB
- `TicketMaster:ApiKey` — key **separada** de `RapidApi:ApiKey`, obtenida en el TicketMaster Developer Portal (no en RapidAPI)
- Certificado para OpenIddict (configurado en `appsettings.json` bajo `OpenIddict`)

**`EventSyncWorker` (intervalo de `EventSyncBackgroundWorker`):**
- `EventSyncWorker:IntervalHours` — cada cuántas horas corre el worker en producción. Default `12` si no está presente (fallback hardcodeado en el worker, no hace falta setearlo).
- `EventSyncWorker:IntervalMinutes` — override en minutos para desarrollo/testing local, **tiene prioridad sobre `IntervalHours` si está presente** (permite probar el worker sin esperar horas reales). Va en `appsettings.Development.json`, **no debe commitearse con valores de producción** en el `appsettings.json` base.

### Frontend — `angular/src/environments/environment.ts`

```typescript
export const environment = {
  production: false,
  application: {
    baseUrl: 'http://localhost:4200',
    name: 'GoPlaces',
  },
  oAuthConfig: {
    issuer: 'https://localhost:44300',
    clientId: 'GoPlaces_App',
    // ...
  },
  apis: {
    default: { url: 'https://localhost:44300' }
  }
};
```

Para producción, usar `environment.prod.ts` con las URLs reales del servidor.

---

## 11. Cosas a tener en cuenta / gotchas

1. **Proxies Angular generados por ABP:** Los archivos en `angular/src/app/proxy/` son auto-generados. Si se modifica una interfaz de Application Service en el backend, hay que regenerar los proxies con el CLI de ABP (`abp generate-proxy`). Editarlos a mano genera inconsistencias.

2. **Rutas Angular comentadas:** La ruta de `tenant-management` está comentada en `app.routes.ts`. Tampoco está activa la protección con `AuthGuard` en la ruta `/cities/search` (comentada). Habría que activarla si se requiere autenticación obligatoria.

3. **Validación de búsqueda con mínimo 3 caracteres:** Está duplicada: en el frontend (`cities-search.ts`) y en el Domain Service (`CitySearchDomainService.cs`). Si se cambia el umbral, hay que actualizarlo en ambos lugares.

4. **`account/manage` redirige a `my-profile`:** Hay un redirect explícito en `app.routes.ts` para compatibilidad con el módulo de cuenta de ABP.

5. **Entidades con propiedades `private set`:** Siguiendo DDD, las propiedades de las entidades solo se modifican a través de métodos explícitos (ej: `SetName()`, `SetCoordinates()`). No usar asignación directa desde fuera de la entidad.

6. **`StringEncryption.DefaultPassPhrase`** en `appsettings.json` está hardcodeada en el repo. No es una buena práctica para producción — debería moverse a un secret manager.

7. **No hay CI/CD configurado:** `.github/workflows/` existe pero está vacío. Hay Dockerfiles disponibles para `HttpApi.Host` y `DbMigrator`, pero no están integrados en ningún pipeline.

8. **Migraciones EF Core:** Hay 5 migraciones acumuladas desde el inicio. Si se agregan nuevas entidades, hay que crear la migración con:
   ```bash
   cd src/GoPlaces.EntityFrameworkCore
   dotnet ef migrations add <NombreMigracion>
   ```
   Y luego correr el DbMigrator para aplicarlas.

9. **Upsert de `Event` por `TicketMasterId`:** al sincronizar eventos, si ya existe un registro con el mismo `TicketMasterId` (de una sync previa sin destino, o disparada desde otra búsqueda) hay que actualizar explícitamente su `DestinationId` si viene uno nuevo — si el código solo hace `continue` al encontrar el existente (como pasó en un bug real de esta rama), el evento queda huérfano para siempre y `GET /events-by-destination/{id}` nunca lo va a encontrar. Test de regresión: `EventAppService_Tests.SearchEventsByCityAsync_vincula_el_DestinationId_a_un_evento_que_ya_existia_sin_destino`.

10. **Dos endpoints de `Event` con propósitos distintos:** `search-events-by-city` dispara la sincronización real contra TicketMaster (llama a la API externa y hace upsert en la base); `events-by-destination/{id}` solo lee lo que ya está sincronizado en la tabla `AppEvents`. Si `events-by-destination` "no trae nada", no es necesariamente un bug — probablemente nadie ejecutó todavía un `search-events-by-city` para ese destino.

11. **401 "The specified token doesn't contain any audience" al llamar un endpoint `[Authorize]` real desde Swagger:** el síntoma es un 401 con `error="invalid_token", error_description="The specified token doesn't contain any audience."` justo después de autenticarse correctamente por el botón "Authorize" de Swagger (flujo `authorization_code`, marcado como "Authorized"). Se investigó a fondo en `feature/events-background-worker` y se descartaron el scope `GoPlaces` (tiene `Resources = ["GoPlaces"]` bien poblado en `OpenIddictScopes`) y los permisos del cliente (`GoPlaces_Swagger` tiene `scp:GoPlaces` en `OpenIddictApplications.Permissions`) — ambos estaban correctos en la base. La causa raíz, confirmada con los logs de Serilog (`src/GoPlaces.HttpApi.Host/Logs/logs.txt`, comparando requests a `/connect/authorize` con y sin `scope=GoPlaces` en la URL): el checkbox del scope `GoPlaces` en el modal de "Authorize" de Swagger no venía pre-tildado por default, así que si no se tildaba a mano antes de confirmar, el request de autorización salía sin `scope=GoPlaces` y el token emitido no traía ni el claim `scope` ni `aud`. Se corrigió agregando `options.OAuthScopes("GoPlaces")` en el bloque `UseAbpSwaggerUI` de `GoPlacesHttpApiHostModule.cs` (pre-tilda el scope automáticamente). Si después de este fix el checkbox sigue sin aparecer tildado, puede hacer falta agregar `options.EnablePersistAuthorization()` (para que Swagger recuerde el estado entre reloads) o simplemente limpiar la caché del navegador / recargar sin caché, ya que Swagger UI puede servir una versión vieja del `index.html` cacheada.

12. **Background Workers sin Unit of Work ambiente → `ObjectDisposedException` en runtime:** a diferencia de los Application Services (que ABP envuelve automáticamente en un Unit of Work), un `AsyncPeriodicBackgroundWorkerBase` no tiene UOW ambiente. Bug real encontrado en `feature/events-background-worker`: `EventSyncBackgroundWorker.GetFollowedDestinationIdsAsync` hacía `await followListRepository.WithDetailsAsync(...)` y materializaba el resultado con `.ToList()` en la línea siguiente — sin UOW explícito, cada llamada a repositorio abre y cierra su propio `DbContext` efímero, así que para cuando corría el `.ToList()` el `DbContext` de la llamada anterior ya estaba disposed. Se manifestó recién corriendo el worker de verdad (`dotnet run`), no en los tests unitarios (que mockean todo). Se solucionó envolviendo el acceso a repositorios en `unitOfWorkManager.Begin(new AbpUnitOfWorkOptions(...), requiresNew: true)` explícito dentro del worker. Ver también el gotcha de convención en sección 4 y el test de regresión `EventSyncBackgroundWorker_RealDbContextTests` (sección 7), que reproduce el error contra un DbContext real.

13. **Componentes hijos de Lepton-X Lite y `ChangeDetectionStrategy.OnPush` → la vista no se repinta sola tras un update async:** `lpx-layout` (`SideMenuLayoutComponent`) y `lpx-toolbar` (`ToolbarComponent`), ambos de `@volo/ngx-lepton-x.lite`/`.core`, están declarados con `ChangeDetectionStrategy.OnPush`. Cualquier componente propio insertado en la barra superior vía `NavItemsService` (como `NotificationBellComponent`) es descendiente de esos dos. Bug real encontrado en `feature/notifications-frontend`: el polling de `NotificationBellComponent` (`RxJS interval` + `.subscribe()`) actualizaba `unreadCount` correctamente cada 45s — el dato ya estaba bien en memoria — pero la vista no se repintaba sola. Como el update se origina puramente dentro del componente (sin que cambie ningún `@Input` ni se dispare un evento DOM sobre `lpx-layout`/`lpx-toolbar`), esos ancestros OnPush nunca quedan marcados "dirty" y el tick global de Angular se detiene ahí, sin llegar a la campana. Se confirmaba con un síntoma engañoso: el contador aparecía actualizado apenas se hacía click en cualquier otro ítem de la misma barra (ej. el ícono de perfil), porque ese click sí dispara un evento DOM dentro de ese árbol OnPush y fuerza el repintado de todo el subárbol, incluida la campana. Se solucionó inyectando `ChangeDetectorRef` y llamando `markForCheck()` en el callback `next`/`error` del `.subscribe()` del polling. Si se agrega otro componente en la barra superior con actualización async que no sea un `@Input` o evento DOM (websockets, otro polling, etc.), va a necesitar el mismo `markForCheck()` — ver también la convención de sección 4.

---

## 12. Cómo trabajar en este proyecto

### Flujo para una nueva feature

1. **Crear rama:** `git checkout -b feature/<número-issue>-<descripcion-kebab>`

2. **Backend — en orden de capas (DDD):**
   - `Domain/`: Agregar entidad o modificar existente. Las propiedades van con `private set` y setters con validación.
   - `Application.Contracts/`: Agregar DTO(s) e interfaz `IXxxAppService`.
   - `Application/`: Implementar el servicio. Si hay llamada a API externa, trackear con `ExternalApiCall`.
   - `EntityFrameworkCore/`: Agregar `DbSet` en `GoPlacesDbContext` si es entidad nueva. Crear migración.
   - `HttpApi/`: ABP genera los endpoints automáticamente desde los Application Services — no suele necesitar tocar el controller.

3. **Regenerar proxies Angular** si cambiaron interfaces/DTOs del backend:
   ```bash
   # Desde la raíz, con el backend corriendo
   abp generate-proxy -t ng
   ```

4. **Frontend:**
   - Crear componente en `angular/src/app/pages/<nombre>/`
   - Usar los servicios proxy de `angular/src/app/proxy/` para llamadas HTTP
   - Registrar la ruta nueva en `app.routes.ts`
   - Formularios: usar Reactive Forms (`FormBuilder`)
   - Feedback al usuario: `this.toaster.error()`/`this.toaster.success()`

5. **Tests (backend):**
   - Crear test en `test/GoPlaces.Application.Tests/` usando Moq + Shouldly
   - Nombrar tests en español con guiones bajos: `MetodoAsync_descripcion_escenario`

6. **Commit y PR:**
   - Commit descriptivo en español
   - Abrir PR a `main` con título que referencie el issue/feature

### Para probar rápido la API
Swagger disponible en `https://localhost:44300/swagger` con el backend corriendo. También hay un `GoPlaces.HttpApi.Client.ConsoleTestApp` para probar el cliente HTTP tipado.
