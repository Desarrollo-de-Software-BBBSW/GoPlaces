# Plataforma de Búsqueda y Seguimiento de Destinos Turísticos

## 📌 Descripción del Proyecto
Este proyecto consiste en una **aplicación web modular** desarrollada con el framework **[ABP.IO](https://abp.io/)** bajo los principios de **Domain-Driven Design (DDD)**.  
Permite a los usuarios **buscar destinos turísticos** a través de una API externa, **gestionar listas personalizadas** de lugares de interés y recibir **notificaciones** sobre cambios relevantes o eventos asociados.

La solución cuenta con una arquitectura multicapa que integra:

- **Backend:** ASP.NET Core + Entity Framework Core + SQL Server.
- **Frontend:** Angular (generado desde ABP.IO).
- **APIs Externas:**
  - GeoDB Cities (o similar) para datos geográficos.
  - TicketMaster (o similar) para eventos.

---

## 🎯 Objetivos
- Implementar una aplicación orientada a servicios, modular y segura.
- Aplicar principios de DDD y arquitectura en capas con ABP.IO.
- Desarrollar interfaces web dinámicas con Angular.
- Integrar autenticación, autorización y buenas prácticas de seguridad.
- Implementar pruebas unitarias en capa de dominio y aplicación.
- Garantizar persistencia escalable con Entity Framework Core.

---

## 🚀 Funcionalidades Principales

### 1. Autenticación y Autorización
- Inicio de sesión con usuario y contraseña.
- Roles:
  - **Administrador:** gestión de usuarios, métricas y monitoreo.
  - **Usuario:** búsqueda y seguimiento de destinos.
- Control de acceso basado en roles (**RBAC**).

### 2. Búsqueda de Destinos
- Consultas por nombre y país.
- Detalles: nombre, país, población, coordenadas e imagen.
- Almacenamiento opcional en base de datos.

### 3. Gestión de Favoritos
- Agregar y eliminar destinos en lista personal.
- Visualización de información básica y actualizaciones.
- Notificaciones configurables en pantalla o por correo.

### 4. Notificaciones
- Panel de novedades en la interfaz.
- Envío por email (opcional e inmediato o semanal).
- Historial mínimo de 30 días.

### 5. Calificaciones y Comentarios
- Puntuar destinos (1 a 5 estrellas).
- Comentarios privados y editables.

### 6. Panel de Administración
- Métricas de uso y estadísticas.
- Registro de errores y fallos de API.
- Exportación de datos en CSV o PDF.

### 7. Auditoría y Seguridad
- Registro de acciones críticas.
- Hash de contraseñas.
- Protección contra XSS, SQL Injection y CSRF.

---

## 🛠️ Requerimientos Técnicos

- **Framework:** ABP.IO
- **Backend:** ASP.NET Core + EF Core
- **Frontend:** Angular
- **Base de Datos:** SQL Server
- **APIs Externas:** GeoDB Cities, TicketMaster
- **Patrón de diseño:** DDD
- **Pruebas:** Unitarias en dominio y aplicación


---
> Rama asociada al issue #2 (setup ABP con SQL Server)
