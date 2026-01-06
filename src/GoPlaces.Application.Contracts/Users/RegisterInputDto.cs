using System.ComponentModel.DataAnnotations;

namespace GoPlaces.Users;

public class RegisterInputDto
{
    [Required]
    [StringLength(128)]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; }

    [Required]
    [StringLength(32, MinimumLength = 6)] // Mínimo 6 caracteres
    public string Password { get; set; }
}