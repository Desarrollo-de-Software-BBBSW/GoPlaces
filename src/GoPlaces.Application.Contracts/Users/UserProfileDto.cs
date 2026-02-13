using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace GoPlaces.Users;

public class UserProfileDto : EntityDto<Guid>
{
    public string? UserName { get; set; } // Opcional en update

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? PhotoUrl { get; set; }
}