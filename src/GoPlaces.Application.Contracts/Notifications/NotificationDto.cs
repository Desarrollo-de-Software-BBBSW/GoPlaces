using System;

namespace GoPlaces.Notifications
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreationTime { get; set; }
    }
}