using GoPlaces.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace GoPlaces.Permissions;

public class GoPlacesPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(GoPlacesPermissions.GroupName, L("Permission:GoPlaces"));
        var destinations = group.AddPermission(GoPlacesPermissions.Destinations.Default, L("Permission:Destinations"));
        destinations.AddChild(GoPlacesPermissions.Destinations.Save, L("Permission:Destinations.Save"));
    }
    private static LocalizableString L(string name) => LocalizableString.Create<GoPlacesResource>(name);
}
