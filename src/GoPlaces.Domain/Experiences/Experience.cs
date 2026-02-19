using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace GoPlaces.Experiences
{
    public class Experience : FullAuditedAggregateRoot<Guid>
    {
        public Guid DestinationId { get; set; } // Conexión con el Destino
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
        public string Rating { get; set; } // 👈 NUEVO CAMPO: "Positiva", "Negativa", "Neutra"

        // Constructor vacío para Entity Framework
        protected Experience() { }

        public Experience(Guid id, Guid destinationId, string title, string description, decimal price, DateTime date, string rating)
            : base(id)
        {
            DestinationId = destinationId;
            Title = title;
            Description = description;
            Price = price;
            Date = date;
            Rating = rating; // 👈 Lo asignamos
        }
    }
}