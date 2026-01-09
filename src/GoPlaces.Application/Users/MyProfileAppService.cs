using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.Users;


namespace GoPlaces.Users
{
    [Authorize(AuthenticationSchemes = "Identity.Application")]
    public class MyProfileAppService : ApplicationService, IMyProfileAppService
    {
        private readonly IdentityUserManager _userManager;
        private readonly ICurrentUser _currentUser;

        public MyProfileAppService(IdentityUserManager userManager, ICurrentUser currentUser)
        {
            _userManager = userManager;
            _currentUser = currentUser;
        }

        public async Task<UserProfileDto> GetAsync()
        {
            // 1. Verificación de Seguridad: ¿ABP sabe quién eres?
            if (_currentUser.Id == null)
            {
                // Si entra aquí, es que la Cookie no le pasó el ID correctamente a ABP
                throw new UserFriendlyException("Error: El sistema no detecta tu ID de usuario. La sesión puede estar corrupta.");
            }

            var userId = _currentUser.Id.Value;

            // 2. Usamos FindByIdAsync en vez de GetByIdAsync (Find devuelve null si no existe, Get explota)
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                throw new UserFriendlyException($"Error: El usuario con ID {userId} no existe en la base de datos.");
            }

            // 3. Devolvemos el DTO
            return new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                PhoneNumber = user.PhoneNumber,

                // Usamos ?. para evitar errores si GetProperty devolviera algo raro
                PhotoUrl = user.ExtraProperties.ContainsKey("PhotoUrl") ? user.GetProperty<string>("PhotoUrl") : null,
                Preferences = user.ExtraProperties.ContainsKey("Preferences") ? user.GetProperty<string>("Preferences") : null
            };
        }

        public async Task UpdateAsync(UserProfileDto input)
        {
            var userId = _currentUser.Id.GetValueOrDefault();
            var user = await _userManager.GetByIdAsync(userId);

            // 1. Actualizamos campos estándar de ABP Identity
            user.Name = input.Name;
            user.Surname = input.Surname;
            user.SetPhoneNumber(input.PhoneNumber, false); // false = no confirmar de nuevo

            // Si cambia el email, ABP pide validaciones extra, por ahora lo actualizamos directo
            // (En producción real, esto requeriría re-confirmar email)
            await _userManager.SetEmailAsync(user, input.Email);

            // 2. Guardamos los campos personalizados en el JSON "ExtraProperties"
            user.SetProperty("PhotoUrl", input.PhotoUrl);
            user.SetProperty("Preferences", input.Preferences);

            // 3. Guardamos en Base de Datos
            await _userManager.UpdateAsync(user);
        }
    }
}