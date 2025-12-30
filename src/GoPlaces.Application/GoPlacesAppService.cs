using GoPlaces.Localization;
using Volo.Abp.Application.Services;

namespace GoPlaces;

/* Inherit your application services from this class.
 */
public abstract class GoPlacesAppService : ApplicationService
{
    protected GoPlacesAppService()
    {
        LocalizationResource = typeof(GoPlacesResource);
    }
}
