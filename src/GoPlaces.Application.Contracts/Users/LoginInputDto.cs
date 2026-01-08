using System.ComponentModel.DataAnnotations;

namespace GoPlaces.Users
{
    public class LoginInputDto
    {
        [Required]
        public string UserNameOrEmail { get; set; }

        [Required]
        public string Password { get; set; }
    }
}