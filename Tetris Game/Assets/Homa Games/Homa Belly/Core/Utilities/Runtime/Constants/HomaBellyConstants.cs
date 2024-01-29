namespace HomaGames.HomaBelly.Utilities
{
    public static class HomaBellyConstants
    {
        public static string PRODUCT_NAME = "Homa Belly";
        public static string PRODUCT_VERSION = "1.11.4";
#if !HOMA_BELLY_DEV_ENV
        public const string API_HOST = "https://damysus-engine.homagames.com";
#else
        public const string API_HOST = "https://damysus-engine.homastage.com";
#endif
        
        public static string API_VERSION = "V2";
    }
}
