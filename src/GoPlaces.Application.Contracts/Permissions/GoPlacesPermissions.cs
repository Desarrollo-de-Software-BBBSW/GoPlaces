namespace GoPlaces.Permissions;

public static class GoPlacesPermissions
{
    public const string GroupName = "GoPlaces";
    public static class Destinations
    {
        public const string Default = GroupName + ".Destinations";
        public const string Save = Default + ".Save";
    }
}
