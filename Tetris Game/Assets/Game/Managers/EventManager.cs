using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : Singleton<EventManager>
{
#if UNITY_EDITOR
    [SerializeField] public bool CanTrack = true;
    [SerializeField] public bool CanLog = true;
#endif
    [SerializeField] public float playTimeEventInterval = 30.0f;
    [SerializeField] public string playtimeTag = "PLAYTIME:";
    [SerializeField] public string onboardingTag = "ONBOARDING:";
    [SerializeField] public string upgradeTag = "UPGRADE:";
    [SerializeField] public string buildTag = "BUILD:";
    //void Start()
    //{
    //    TickManager.THIS += new TickManager.CustomTickable(playTimeEventInterval, () =>
    //    {
    //        TrackPlaytime();

    //    });
    //}
    public void TrackOnboardingCompleted(ONBOARDING step)
    {
        string eventMessage = onboardingTag + step.ToString();
        eventMessage.LogEvent();
        SendTrackDesignEvent(eventMessage, PlayTime());
    }
    public void TrackUpgrade(string upgradeName, int upgradeIndex)
    {
        string eventMessage = upgradeTag + upgradeName + ":" + upgradeIndex;
        eventMessage.LogEvent();
        SendTrackDesignEvent(eventMessage, PlayTime());
    }
    public void TrackBuild(string buildName)
    {
        string eventMessage = upgradeTag + buildName;
        eventMessage.LogEvent();
        SendTrackDesignEvent(eventMessage, PlayTime());
    }
    public void TrackPlaytime()
    {
        int playTime = PlayTime();
        string eventMessage = playtimeTag + playTime;
        eventMessage.LogEvent();
        SendTrackDesignEvent(eventMessage, playTime);
    }

    private int PlayTime()
    {
        return ((int)SaveManager.THIS.saveData.playTime);
    }
    private void SendTrackDesignEvent(string eventMessage, int eventID)
    {
#if UNITY_EDITOR
        if (CanTrack)
#endif
        {
            //HomaGames.HomaBelly.HomaBelly.Instance.TrackDesignEvent(eventMessage, eventID);
        }
    }
}
public static class EventManagerExtensions
{
    public static void LogEvent(this object obj)
    {
#if UNITY_EDITOR
        if (EventManager.THIS.CanLog)
        {
            obj.LogW();
        }
#endif
    }
    public static void Track(this ONBOARDING step)
    {
        EventManager.THIS.TrackOnboardingCompleted(step);
    }
    public static void TrackIndexAsUpgrade(this int index, string name)
    {
        EventManager.THIS.TrackUpgrade(name, index);
    }
    public static void TrackBuild(this string name)
    {
        EventManager.THIS.TrackBuild(name);
    }
}
