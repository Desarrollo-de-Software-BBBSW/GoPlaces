using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace GoPlaces.Users
{
    public class UserProfileDto : EntityDto<Guid>
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string PhoneNumber { get; set; }

        // Estos dos no existen en la tabla original, los guardaremos como "Extras"
        public string PhotoUrl { get; set; }
        public string Preferences { get; set; }
    }
}