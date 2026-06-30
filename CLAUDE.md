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
- **TicketMaster** (via RapidAPI): eventos en destinos

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
        │   │   ├── ratings/
        │   │   └── users/
        │   ├── services/               # Servicios Angular custom (no generados)
        │   ├── shared/                 # Componentes reutilizables
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
| `Notifications/` | `Notification` | Alerta de cambio en destino favorito |
| `Follow/` | `FollowList`, `FollowListItem` | Lista personal de favoritos |
| `ExternalApiMetrics/` | `ExternalApiCall` | Tracking de llamadas a APIs externas |

---

## 4. Convenciones de código

### Backend (C#)
- **Naming:** PascalCase para todo (clases, métodos, propiedades). Prefijo `I` para interfaces.
- **DTOs:** Sufijo `Dto` (ej: `DestinationDto`, `CitySearchRequestDto`)
- **Interfaces de servicio:** Prefijo `I` + nombre + `AppService` (ej: `ICityAppService`)
- **Tests:** Nombre en español descriptivo del escenario, separados por guiones bajos (ej: `SearchCitiesAsync_devuelve_resultados_del_servicio`)
- **Entidades:** Heredan de `FullAuditedAggregateRoot<Guid>` (ABP), propiedades `private set`, setters explícitos con validación mediante `Check.NotNullOrWhiteSpace`
- **Migraciones EF:** Nombre en inglés descriptivo (ej: `Added_Experience_Entity`, `Added_Rating_To_Experience`)

### Frontend (Angular/TypeScript)
- **Componentes:** PascalCase para clase, kebab-case para selector y archivo (ej: `CitiesSearchComponent`, archivo `cities-search.ts`)
- **Servicios proxy:** En `angular/src/app/proxy/` — generados automáticamente por ABP, **no editar manualmente**
- **Servicios custom:** En `angular/src/app/services/`
- **Formularios:** Reactive Forms de Angular (`FormBuilder`, `FormGroup`)
- **HTTP async:** RxJS Observables con `.subscribe({ next, error })`
- **Guards:** `AuthGuard` y `PermissionGuard` de ABP para rutas protegidas

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

---

## 6. Sistema de diseño / UI

- **Tema base:** Lepton-X Lite (ABP), cargado via `angular.json` en los styles
- **Estilos globales:** `angular/src/styles.scss`
- **Íconos:** FontAwesome (incluido en angular.json)
- **Tablas:** `ngx-datatable` para listados de datos
- **Formularios:** Angular Reactive Forms + clases de Lepton-X para inputs y feedback visual
- **Toaster:** ABP Toaster service para mensajes de error/éxito (`this.toaster.error(...)`, `this.toaster.success(...)`)
- **No hay design system custom** documentado; se usan las clases de Lepton-X directamente

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

**Cobertura actual:** Application Services (Cities, Destinations, Experiences, Ratings, Notifications, Follow, ExternalApiMetrics) y Domain Services.

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
  "AuthServer": {
    "Authority": "https://localhost:44300"
  }
}
```

Valores que hay que configurar localmente (no están en el repo):
- `ConnectionStrings:Default` — cadena de conexión a SQL Server local
- `RapidApi:ApiKey` — key de RapidAPI para GeoDB y TicketMaster
- Certificado para OpenIddict (configurado en `appsettings.json` bajo `OpenIddict`)

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
