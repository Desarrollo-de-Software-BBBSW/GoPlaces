using GoPlaces.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace GoPlaces.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class GoPlacesController : AbpControllerBase
{
    protected GoPlacesController()
    {
        LocalizationResource = typeof(GoPlacesResource);
    }
}
