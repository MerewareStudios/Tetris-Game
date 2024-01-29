namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Deprecated, use UnityEditor.SettingsProvider instead.
    /// </summary>
    public interface ISettingsProvider
    {
        int Order { get; }
        string Name { get; }
        string Version { get; }
        void Draw();
    }
}