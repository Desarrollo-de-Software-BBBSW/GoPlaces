using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace GoPlaces.Ratings;

public class CreateRatingDto
{
    [Required]
    // 👇 CAMBIO CRÍTICO: De int a Guid
    public Guid DestinationId { get; set; }

    [Range(1, 5, ErrorMessage = "Score must be between 1 and 5.")]
    public int Score { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}