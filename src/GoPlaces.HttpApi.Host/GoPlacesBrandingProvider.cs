using Microsoft.Extensions.Localization;
using GoPlaces.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace GoPlaces;

[Dependency(ReplaceServices = true)]
public class GoPlacesBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<GoPlacesResource> _localizer;

    public GoPlacesBrandingProvider(IStringLocalizer<GoPlacesResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
