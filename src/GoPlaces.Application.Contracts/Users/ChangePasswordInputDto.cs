using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace GoPlaces.Users
{
    public class ChangePasswordInputDto
    {
        [Required]
        public required string CurrentPassword { get; set; }

        [Required]
        [StringLength(32, MinimumLength = 6)] // Consistente con RegisterInputDto
        public required string NewPassword { get; set; }
    }
}