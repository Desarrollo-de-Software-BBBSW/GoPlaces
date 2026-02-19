using System;
using Volo.Abp.Application.Dtos;

namespace GoPlaces.Experiences
{
    public class ExperienceDto : AuditedEntityDto<Guid>
    {
        public Guid DestinationId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
        public string Rating { get; set; } // 👈 NUEVA PROPIEDAD
    }
}