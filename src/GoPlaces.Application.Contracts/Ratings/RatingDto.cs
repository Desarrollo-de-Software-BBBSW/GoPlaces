using System;
using Volo.Abp.Application.Dtos;

namespace GoPlaces.Ratings
{
    public class RatingDto : EntityDto<Guid>
    {
        public Guid DestinationId { get; set; } // 👈 También debe ser Guid
        public int Score { get; set; }
        public string? Comment { get; set; }
        public Guid UserId { get; set; }
    }
}