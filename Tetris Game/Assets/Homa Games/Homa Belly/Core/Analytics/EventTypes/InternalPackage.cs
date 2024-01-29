namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class InternalPackage : AnalyticsEvent
    {
        public string PackageName { get; }
        public string Version { get; }
        public string Status { get; }
        public InternalPackage(string packageName, string version, InternalPackageStatus status) : base(HomaGames.HomaBelly.EventCategory.internal_package.ToString())
        {
            PackageName = packageName;
            Version = version;
            Status = ToUnderscoreCase(status.ToString());
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"InternalPackage:{PackageName}:{Version}:{Status}");
        }
    }
}