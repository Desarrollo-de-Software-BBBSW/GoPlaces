using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace GoPlaces.Follows;

public interface IFollowListRepository : IRepository<FollowList, Guid>
{
    Task<FollowList?> FindDefaultByOwnerAsync(Guid ownerUserId);
}
