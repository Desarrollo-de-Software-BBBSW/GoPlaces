using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Users;

namespace GoPlaces.Ratings;

public class Rating : AuditedAggregateRoot<Guid>, IUserOwned
{
    // ✅ CAMBIO 1: Ahora guardamos un GUID, no un int
    public Guid DestinationId { get; private set; }

    public int Score { get; private set; }
    public string? Comment { get; private set; }
    public Guid UserId { get; set; }

    private Rating() { } // Constructor vacío para Entity Framework

    // ✅ CAMBIO 2: El constructor recibe Guid para el destino
    public Rating(Guid id, Guid destinationId, int score, string? comment, Guid userId)
        : base(id)
    {
        SetScore(score);
        DestinationId = destinationId;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        UserId = userId;
    }

    public void SetScore(int score)
    {
        if (score < 1 || score > 5)
            throw new BusinessException("Rating.ScoreOutOfRange");
        Score = score;
    }
}