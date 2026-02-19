using System;
using System.ComponentModel.DataAnnotations;

namespace GoPlaces.Experiences
{
    public class CreateUpdateExperienceDto
    {
        [Required]
        public Guid DestinationId { get; set; }

        [Required]
        [StringLength(128)]
        public string Title { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Rating { get; set; } // 👈 NUEVA PROPIEDAD REQUERIDA
    }
}