using GoPlaces.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace GoPlaces.Follows;

public class EfCoreFollowListRepository
    : EfCoreRepository<GoPlacesDbContext, FollowList, Guid>, IFollowListRepository
{
    public EfCoreFollowListRepository(IDbContextProvider<GoPlacesDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<FollowList?> FindDefaultByOwnerAsync(Guid ownerUserId)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.FollowLists
            .Include(x => x.Items) // eager load if needed
            .FirstOrDefaultAsync(x => x.OwnerUserId == ownerUserId && x.IsDefault);
    }
}
