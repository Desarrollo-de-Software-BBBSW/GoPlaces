using System;

namespace GoPlaces.Notifications
{
    public class NotifyDestinationChangeInputDto
    {
        public Guid DestinationId { get; set; }
        public string ChangeDescription { get; set; }
    }
}