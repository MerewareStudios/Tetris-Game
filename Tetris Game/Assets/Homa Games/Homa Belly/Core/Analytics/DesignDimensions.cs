namespace HomaGames.HomaBelly
{
    public class DesignDimensions
    {
        public string Key1 { get; }
        public string Key2 { get; }
        public string Key3 { get; }
        public string Key4 { get; }
        public string Key5 { get; }
        public float? Score { get; }

        public DesignDimensions(string key1 = null, string key2 = null, string key3 = null, string key4 = null, string key5 = null, float? score = null)
        {
            Key1 = key1;
            Key2 = key2;
            Key3 = key3;
            Key4 = key4;
            Key5 = key5;
            Score = score;
        }
    }
}