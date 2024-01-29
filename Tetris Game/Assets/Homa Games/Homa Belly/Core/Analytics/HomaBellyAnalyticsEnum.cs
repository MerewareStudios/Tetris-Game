namespace HomaGames.HomaBelly
{
    public enum ErrorSeverity
    {
        Undefined = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Critical = 5
    }

    public enum ProgressionStatus
    {
        /// <summary>Undefined progression</summary>
        Undefined = 0,
        /// <summary>User started progression</summary>
        Start = 1,
        /// <summary>User successfully ended a progression</summary>
        Complete = 2,
        /// <summary>User failed a progression</summary>
        Fail = 3
    }

    public enum ResourceFlowType
    {
        /// <summary>Undefined flow type</summary>
        Undefined = 0,
        /// <summary>Source: Used when adding resource to a user</summary>
        Source = 1,
        /// <summary>Sink: Used when removing a resource from a user</summary>
        Sink = 2
    }

    public enum ItemUpgradeType
    {
        /// <summary>Item level upgrade</summary>
        Item,
        /// <summary>General, non-item level upgrade</summary>
        Upgrade
    }
    
    /// <summary>Enum representing the different reasons that can fire an item flow</summary>
    public enum ItemFlowReason
    {
        /// <summary>A rewarded video completed</summary>
        RewardedVideoAd,
        /// <summary>An interstitial video completed</summary>
        InterstitialAd,
        /// <summary>The user purchased something</summary>
        InAppPurchase,
        /// <summary>The user reached a level or other progression event</summary>
        Progression,
        /// <summary>An obsolete flowResource event</summary>
        Obsolete,
        /// <summary>Something else</summary>
        Other
    }
    
    /// <summary>Enum representing the different reasons that can fire a resource flow</summary>
    public enum ResourceFlowReason
    {
        /// <summary>A rewarded video completed</summary>
        RewardedVideoAd,
        /// <summary>An interstitial video completed</summary>
        InterstitialAd,
        /// <summary>The user purchased something</summary>
        InAppPurchase,
        /// <summary>The user reached a level or other progression event</summary>
        Progression,
        /// <summary>An obsolete flowResource event</summary>
        Obsolete,
        /// <summary>Something else</summary>
        Other
    }

    public enum AdAction
    {
        Undefined = 0,
        Clicked = 1,
        Show = 2,
        FailedShow = 3,
        RewardReceived = 4,
        Request = 5,
        Loaded = 6
    }

    public enum AdType
    {
        Undefined = 0,
        Video = 1,
        RewardedVideo = 2,
        Playable = 3,
        Interstitial = 4,
        OfferWall = 5,
        Banner = 6
    }

    public enum AdError
    {
        Undefined = 0,
        Unknown = 1,
        Offline = 2,
        NoFill = 3,
        InternalError = 4,
        InvalidRequest = 5,
        UnableToPrecache = 6
    }
    
    public enum EventCategory
    {
        progression_event,
        session_event,
        system_event,
        design_event,
        custom_event,
        ad_event,
        iap_event,
        item_event,
        resource_event,
        internal_package,
        remote_configuration_fetch_event,
    }

    /// <summary>The type of the bonus object opened</summary>
    public enum BonusObjectType
    {
        /// <summary>Chestroom</summary>
        Chestroom,
        /// <summary>Randomized bonus (roulette or similar)</summary>
        RandomBonus,
        /// <summary>A special bonus level</summary>
        BonusLevel,
        /// <summary>Something different or game-specific</summary>
        CustomBonus
    }

    /// <summary>Enum representing the different status messages sent by the InternalPackage event</summary>
    public enum InternalPackageStatus
    {
        /// <summary>When a package is installed in a game</summary>
        Installed,
        /// <summary>When a package is installed in a game, and enabled</summary>
        Enabled,
        /// <summary>When the package is shown to a player</summary>
        Suggested,
        /// <summary>When the player interacts right after sending suggested event</summary>
        Triggered,
        /// <summary>When the player interacts with the package (click any button)</summary>
        Interacted,
    }

    public enum ProductCategory
    {
        Subscription,
        SeasonPass,
        Currency,
        Bundle,
        NoAds,
        Cosmetics,
        Other,
    }

    public enum CurrencyType
    {
        Soft,
        Hard,
        Tickets,
        Other,
    }
}