using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

        public async Task ChangePasswordAsync(ChangePasswordInputDto input)
        {
            // 1. Obtener el usuario actual a través del ID de la sesión
            var userId = _currentUser.Id.GetValueOrDefault();
            var user = await _userManager.GetByIdAsync(userId);

            // 2. Usar la funcionalidad nativa de IdentityUserManager
            // ChangePasswordAsync verifica internamente si la 'CurrentPassword' es correcta
            var result = await _userManager.ChangePasswordAsync(
                user,
                input.CurrentPassword,
                input.NewPassword
            );

            // 3. Verificar si hubo errores (ej: contraseña actual incorrecta o nueva contraseña débil)
            // CheckErrors() es una extensión de ABP que lanza excepciones amigables automáticamente
            result.CheckErrors();
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
        public async Task DeleteAsync()
        {
            var userId = _currentUser.Id.Value;

            // Buscamos al usuario (usamos FindByIdAsync para no lanzar excepción si no existe aun)
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                // Si por alguna razón extraña no existe, avisamos.
                throw new UserFriendlyException("El usuario no existe o ya fue eliminado.");
            }

            // ALERTA: ABP maneja esto automáticamente como "Soft Delete".
            // Al llamar a DeleteAsync, ABP intercepta la llamada y en vez de borrar la fila (DELETE FROM...),
            // simplemente pone "IsDeleted = 1" en la base de datos.
            (await _userManager.DeleteAsync(user)).CheckErrors();
        }
    }
}