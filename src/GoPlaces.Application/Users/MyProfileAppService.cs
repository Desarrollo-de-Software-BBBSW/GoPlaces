using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data; // Necesario para .SetProperty / .GetProperty
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace GoPlaces.Users
{
    [Authorize] // Asegura que solo usuarios logueados entren aquí
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
                throw new UserFriendlyException("Error: No estás logueado.");
            }

            var userId = _currentUser.Id.Value;

            // 2. Buscamos al usuario ACTUAL en la base de datos
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                throw new UserFriendlyException($"Error: El usuario con ID {userId} no existe.");
            }

            // 3. Devolvemos el DTO con los datos REALES
            return new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                PhoneNumber = user.PhoneNumber,

                // Propiedades extra (si las usas)
                PhotoUrl = user.ExtraProperties.ContainsKey("PhotoUrl") ? (string)user.ExtraProperties["PhotoUrl"] : null,
                Preferences = user.ExtraProperties.ContainsKey("Preferences") ? (string)user.ExtraProperties["Preferences"] : null
            };
        }

        public async Task UpdateAsync(UserProfileDto input)
        {
            if (_currentUser.Id == null) throw new UserFriendlyException("No estás logueado.");

            var user = await _userManager.GetByIdAsync(_currentUser.Id.Value);

            // 1. Actualizamos campos estándar
            user.Name = input.Name;
            user.Surname = input.Surname;
            await _userManager.SetPhoneNumberAsync(user, input.PhoneNumber);
            await _userManager.SetEmailAsync(user, input.Email);

            // 2. Guardamos los campos personalizados en ExtraProperties
            // Usamos SetProperty que es la forma segura de ABP
            user.SetProperty("PhotoUrl", input.PhotoUrl);
            user.SetProperty("Preferences", input.Preferences);

            // 3. Guardamos en Base de Datos
            (await _userManager.UpdateAsync(user)).CheckErrors();
        }

        public async Task ChangePasswordAsync(ChangePasswordInputDto input)
        {
            if (_currentUser.Id == null) throw new UserFriendlyException("No estás logueado.");

            var user = await _userManager.GetByIdAsync(_currentUser.Id.Value);

            // IdentityUserManager tiene un método específico para cambiar password validando la actual
            var result = await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);

            if (!result.Succeeded)
            {
                // Si falla (ej: password actual incorrecta), lanzamos error amigable
                // Usamos CheckErrors para convertir los errores de Identity a excepciones ABP
                result.CheckErrors();
            }
        }

        public async Task DeleteAsync()
        {
            if (_currentUser.Id == null) throw new UserFriendlyException("No estás logueado.");

            var user = await _userManager.FindByIdAsync(_currentUser.Id.Value.ToString());
            if (user == null) throw new UserFriendlyException("El usuario no existe.");

            (await _userManager.DeleteAsync(user)).CheckErrors();
        }
    }
}