using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace GoPlaces
{
    public class GoPlacesTestDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IdentityUserManager _userManager;
        private readonly IGuidGenerator _guidGenerator;

        public GoPlacesTestDataSeedContributor(
            IdentityUserManager userManager,
            IGuidGenerator guidGenerator)
        {
            _userManager = userManager;
            _guidGenerator = guidGenerator;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            /* DATOS OBLIGATORIOS PARA QUE FUNCIONEN LOS TESTS DE LOGIN */
            const string adminUserName = "admin";
            const string adminPassword = "1q2w3E*";
            const string adminEmail = "admin@abp.io";

            var adminUser = await _userManager.FindByNameAsync(adminUserName);

            if (adminUser == null)
            {
                adminUser = new IdentityUser(
                    _guidGenerator.Create(),
                    adminUserName,
                    adminEmail
                );

                (await _userManager.CreateAsync(adminUser, adminPassword)).CheckErrors();
            }
        }
    }
}