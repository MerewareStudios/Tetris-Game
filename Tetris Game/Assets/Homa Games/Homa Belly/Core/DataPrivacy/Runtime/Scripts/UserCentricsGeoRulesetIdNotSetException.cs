namespace HomaGames.HomaBelly.DataPrivacy
{
    public class UserCentricsGeoRulesetIdNotSetException : UserCentricsWrapperException
    {
        public UserCentricsGeoRulesetIdNotSetException() : base("UserCentrics ruleset ID not set in the settings, initialisation aborted.")
        {
        }
    }
}