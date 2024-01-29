namespace HomaGames.HomaBelly.DataPrivacy
{
    /// <summary>
    /// The status values for app tracking authorization. (iOS 14+)
    /// <para>After a device receives an authorization request to approve access to app-related data that can be used for tracking the user or the device, the returned value is either:</para>
    /// <para>- <see cref="Authorized"/></para>
    /// <para>- <see cref="Denied"/></para>
    /// <para>Before a device receives an authorization request to approve access to app-related data that can be used for tracking the user or the device, the returned value is: <see cref="NotDetermined"/></para>
    /// <para>If authorization to use app tracking data is restricted, the value is: <see cref="Restricted"/></para>
    /// </summary>
    public enum AttAuthorizationStatus
    {
        /// <summary>
        /// The value that returns if the user authorizes access to app-related data for tracking the user or the device.
        /// </summary>
        Authorized,
        /// <summary>
        /// The value that returns if the user denies authorization to access app-related data for tracking the user or the device.
        /// </summary>
        Denied,
        /// <summary>
        /// The value that returns when the app can’t determine the user’s authorization status for access to app-related data for tracking the user or the device.
        /// </summary>
        NotDetermined,
        /// <summary>
        /// The value that returns if authorization to access app-related data for tracking the user or the device has a restricted status.
        /// </summary>
        Restricted
    }
}