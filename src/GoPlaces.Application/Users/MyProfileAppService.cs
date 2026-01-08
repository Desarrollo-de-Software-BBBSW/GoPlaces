using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.Users;


namespace GoPlaces.Users
{
    [Authorize] // 🔒 Solo usuarios logueados pueden entrar aquí
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
            // 1. Buscamos al usuario logueado en la BD
            var userId = _currentUser.Id.GetValueOrDefault();
            var user = await _userManager.GetByIdAsync(userId);

            // 2. Convertimos la Entidad a DTO manualmente
            return new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                PhoneNumber = user.PhoneNumber,

                // 3. Recuperamos los datos extra (si no existen, devuelve null)
                PhotoUrl = user.GetProperty<string>("PhotoUrl"),
                Preferences = user.GetProperty<string>("Preferences")
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