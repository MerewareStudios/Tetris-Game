namespace HomaGames.HomaBelly.Internal.Analytics
{
    internal static class InternalAnalytics
    {
        /// <summary>
        /// Tracks the initiation of the firstTime fetch operation.
        /// </summary>
        internal static void FirstTimeFetchStarted() => new FirstTimeFetchStarted().TrackEvent();

        /// <summary>
        /// Tracks the completion of the firstTime fetch operation.
        /// </summary>
        internal static void FirstTimeFetchCompleted() => new FirstTimeFetchCompleted().TrackEvent();

        /// <summary>
        /// Tracks the failure of the firstTime fetch operation, with optional failure reason.
        /// </summary>
        internal static void FirstTimeFetchFailed(string reason = "") => new FirstTimeFetchFailed(reason).TrackEvent();

        /// <summary>
        /// Tracks the initiation of everyTime fetch operation.
        /// </summary>
        internal static void EveryTimeFetchStarted() => new EveryTimeFetchStarted().TrackEvent();

        /// <summary>
        /// Tracks the completion of everyTime fetch operation.
        /// </summary>
        internal static void EveryTimeFetchCompleted() => new EveryTimeFetchCompleted().TrackEvent();

        /// <summary>
        /// Tracks the failure of everyTime fetch operation, with optional failure reason.
        /// </summary>
        internal static void EveryTimeFetchFailed(string reason = "") => new EveryTimeFetchFailed(reason).TrackEvent();
    }
}