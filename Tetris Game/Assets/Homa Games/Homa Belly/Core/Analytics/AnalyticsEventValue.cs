namespace HomaGames.HomaBelly.Internal.Analytics
{
    public struct AnalyticsEventValue
    {
        public string Name { get; }
        public float Value { get; }
        public AnalyticsEventValue(string name, float value = 0)
        {
            Name = name;
            Value = value;
        }
    }
}