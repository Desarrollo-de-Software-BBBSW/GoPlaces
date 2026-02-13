using GoPlaces.Users;
using Microsoft.AspNetCore.Identity;
using Shouldly;
using System.Threading.Tasks;
using Volo.Abp.Guids;
using Volo.Abp.Uow;
using Xunit;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using IdentityUserManager = Volo.Abp.Identity.IdentityUserManager;

namespace GoPlaces.Tests.Users
{
    public class LoginAppService_Tests : GoPlacesApplicationTestBase<GoPlacesApplicationTestModule>
    {
        private readonly IdentityUserManager _userManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IGuidGenerator _guidGenerator;

        public LoginAppService_Tests()
        {
            _userManager = GetRequiredService<IdentityUserManager>();
            _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
            _guidGenerator = GetRequiredService<IGuidGenerator>();
        }

        [Fact]
        public async Task Should_Login_With_Valid_Credentials()
        {
            var username = "login_master";
            var password = "1q2w3E*Password";

            await WithUnitOfWorkAsync(async () =>
            {
                var user = new IdentityUser(_guidGenerator.Create(), username, "master@test.com");
                user.SetEmailConfirmed(true);

                await _userManager.UpdateSecurityStampAsync(user);
                (await _userManager.CreateAsync(user, password)).CheckErrors();

                await _unitOfWorkManager.Current.SaveChangesAsync();

                var loginService = GetRequiredService<IMyLoginAppService>();
                var result = await loginService.LoginAsync(new LoginInputDto
                {
                    UserNameOrEmail = username,
                    Password = password
                });

                result.ShouldBeTrue();
            });
        }
    }
}