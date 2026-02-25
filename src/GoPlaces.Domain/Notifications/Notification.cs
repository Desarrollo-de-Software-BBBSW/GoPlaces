using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace GoPlaces.Notifications
{
    // Usamos CreationAuditedAggregateRoot para que ABP le guarde la fecha de creación automáticamente
    public class Notification : CreationAuditedAggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }
        public string Title { get; private set; }
        public string Message { get; private set; }
        public bool IsRead { get; private set; }
        public Guid? DestinationId { get; private set; } // Opcional, para saber qué destino disparó la alerta

        private Notification() { } // Constructor vacío requerido por Entity Framework

        public Notification(Guid id, Guid userId, string title, string message, Guid? destinationId = null)
            : base(id)
        {
            UserId = userId;
            Title = title;
            Message = message;
            IsRead = false; // Arranca como "No leída"
            DestinationId = destinationId;
        }

        // 👇 NUEVO: Método actualizado para permitir ambos estados (leída/no leída)
        public void SetReadState(bool isRead)
        {
            IsRead = isRead;
        }
    }
}