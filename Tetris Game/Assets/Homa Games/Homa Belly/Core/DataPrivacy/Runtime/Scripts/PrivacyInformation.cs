using System;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public static class PrivacyInformation
    {
        public static event Action<AttAuthorizationStatus> AttAuthorizationStatusChanged
        {
            add => UserCentricsApiWrapper.AttAuthorizationStatusChanged += value;
            remove => UserCentricsApiWrapper.AttAuthorizationStatusChanged -= value;
        }

        public static AttAuthorizationStatus AttAuthorizationStatus => UserCentricsApiWrapper
            .AttAuthorizationStatus;

        public static string TcString => UserCentricsApiWrapper.TcString;
    }
}