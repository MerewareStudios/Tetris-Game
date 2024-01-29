using System;
using UnityEditor;

namespace HomaGames.HomaBelly
{
    public abstract class ReporterBase
    {
        private string LAST_PACKAGE_REPORT_KEY => $"homa_last_{GetType().Name}_date";
        
        protected long LastTimeReported
        {
            get =>
                long.Parse(ProjectPrefs.TryGet(LAST_PACKAGE_REPORT_KEY, out string val) ? val : "0");
            set => ProjectPrefs.Set(LAST_PACKAGE_REPORT_KEY, value.ToString());
        }

        protected bool CanReport => DateTimeOffset.UtcNow.ToUnixTimeSeconds() - LastTimeReported >= MinTimeInSecondsBetweenReports;

        protected abstract long MinTimeInSecondsBetweenReports { get; }
    }
}