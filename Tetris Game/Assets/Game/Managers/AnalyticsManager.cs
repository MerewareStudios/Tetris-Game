using UnityEngine;
//ANALYTICS_ENABLED
public static class AnalyticsManager
{
    private const string AnalyticsEnabled = "ANALYTICS_ENABLED";
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void OnboardingStepComplete(string stepName)
    {
        string trace = stepName;
        Log(trace);
    }


    private static void Log(string trace)
    {
        Debug.LogWarning("<color=yellow>" + trace + "</color>");
    }
}
