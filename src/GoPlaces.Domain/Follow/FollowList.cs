using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Guids;

namespace GoPlaces.Follows;

public class FollowList : FullAuditedAggregateRoot<Guid>
{
    public const int NameMaxLength = 64;
    public const int DescriptionMaxLength = 256;

    public Guid OwnerUserId { get; private set; }
    public bool IsDefault { get; private set; }

    public string Name { get; private set; }
    public string? Description { get; private set; }
    public DateTime LastUpdatedDate { get; private set; }

    private readonly List<FollowListItem> _items = new();
    public IReadOnlyCollection<FollowListItem> Items => _items;

    private FollowList() { } // EF

    public FollowList(
        Guid id,
        Guid ownerUserId,
        string name,
        bool isDefault = true,
        string? description = null,
        DateTime? lastUpdatedDate = null) : base(id)
    {
        OwnerUserId = ownerUserId;
        IsDefault = isDefault;

        SetName(name);
        SetDescription(description);
        Touch(lastUpdatedDate ?? DateTime.UtcNow);
    }

    public void SetName(string name) =>
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: NameMaxLength);

    public void SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description))
            Check.Length(description, nameof(description), maxLength: DescriptionMaxLength);

        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Touch(); // lo consideramos una actualización “de negocio”
    }

    private void Touch(DateTime? when = null) =>
        LastUpdatedDate = (when is null || when == default) ? DateTime.UtcNow : when.Value;

    public bool HasDestination(Guid destinationId) =>
        _items.Any(i => i.DestinationId == destinationId);

    public FollowListItem AddDestination(Guid destinationId)
    {
        if (HasDestination(destinationId))
            throw new BusinessException("GoPlaces:DestinationAlreadySaved");

        var item = new FollowListItem(Guid.NewGuid(), Id, destinationId);
        _items.Add(item);
        Touch(); // actualización
        return item;
    }

    public void RemoveDestination(Guid destinationId)
    {
        var item = _items.FirstOrDefault(i => i.DestinationId == destinationId);
        if (item != null)
        {
            _items.Remove(item);
            Touch(); // actualización
        }
    }
}
