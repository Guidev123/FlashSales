namespace Modules.Launches.Endpoints
{
    public static class LaunchesPermissions
    {
        public static class Launches
        {
            public const string Create = "launches:create";
            public const string Schedule = "launches:schedule:own";
            public const string Cancel = "launches:cancel:own";
        }
    }
}