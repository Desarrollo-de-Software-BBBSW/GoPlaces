using GoPlaces.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace GoPlaces.Permissions;

public class GoPlacesPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(GoPlacesPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(GoPlacesPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<GoPlacesResource>(name);
    }
}
