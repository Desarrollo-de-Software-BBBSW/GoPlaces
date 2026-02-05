using System;
using Volo.Abp.Application.Dtos;

namespace GoPlaces.Users
{
    public class PublicUserProfileDto : EntityDto<Guid>
    {
        // Agregamos 'required' para obligar a que tengan valor al crearse
        // O usamos '= string.Empty' para que tengan un valor por defecto.

        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;

        // Estos pueden ser nulos (el usuario puede no tener foto o preferencias)
        public string? PhotoUrl { get; set; }
        public string? Preferences { get; set; }
    }
}