using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace GoPlaces.Follows;

public class FollowListItem : CreationAuditedEntity<Guid>
{
    public Guid FollowListId { get; private set; }
    public Guid DestinationId { get; private set; }

    private FollowListItem() { } // EF Core

    public FollowListItem(Guid id, Guid followListId, Guid destinationId) : base(id)
    {
        FollowListId = followListId;
        DestinationId = destinationId;
    }
}
