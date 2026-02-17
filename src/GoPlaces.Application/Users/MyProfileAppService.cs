using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace GoPlaces.Users;

[Authorize]
public class MyProfileAppService : GoPlacesAppService, IMyProfileAppService
{
    protected IdentityUserManager UserManager { get; }
    protected ICurrentUser CurrentUser { get; }

    public MyProfileAppService(IdentityUserManager userManager, ICurrentUser currentUser)
    {
        UserManager = userManager;
        CurrentUser = currentUser;
    }

    public virtual async Task<UserProfileDto> GetAsync()
    {
        var user = await UserManager.GetByIdAsync(CurrentUser.GetId());
        return new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Name = user.Name,
            Surname = user.Surname,
            PhotoUrl = user.GetProperty<string>("PhotoUrl")
        };
    }

    public virtual async Task UpdateAsync(UserProfileDto input)
    {
        var user = await UserManager.GetByIdAsync(CurrentUser.GetId());
        user.Name = input.Name;
        user.Surname = input.Surname;
        if (!input.Email.IsNullOrWhiteSpace()) await UserManager.SetEmailAsync(user, input.Email);
        user.SetProperty("PhotoUrl", input.PhotoUrl);
        (await UserManager.UpdateAsync(user)).CheckErrors();
    }

    public virtual async Task ChangePasswordAsync(ChangePasswordInputDto input)
    {
        var user = await UserManager.GetByIdAsync(CurrentUser.GetId());
        (await UserManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword)).CheckErrors();
    }

    public virtual async Task DeleteAsync()
    {
        var user = await UserManager.GetByIdAsync(CurrentUser.GetId());
        (await UserManager.DeleteAsync(user)).CheckErrors();
    }
}