using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomaGames.Geryon.Editor.CodeGen;
using HomaGames.HomaBelly;
using UnityEditor;

namespace HomaGames.Geryon.Editor
{
    internal static class DvrScriptManager
    {
        public static readonly DvrScriptVersion DefaultDvrScriptVersion = DvrScriptVersion.Observables;
        public static int UpdateProcessProgressId { get; private set; }

        public static DvrScriptVersion GetDvrScriptVersionForAutomaticUpdate()
        {
            // No DVR file => Default 
            if (!Database.TryGetDvrAssetPath(out var dvrFilePath))
                return DefaultDvrScriptVersion;

            // Update existing DVR Script using the same version
            if (Database.TryGetDvrScriptVersion(dvrFilePath, out var ver))
                return ver;

            return DvrScriptVersion.Unknown;
        }

        public static async Task CreateOrUpdateDvrFileAsync(bool showNoRemoteConfigDialog,
            CancellationToken cancellationToken = default)
        {
            var version = GetDvrScriptVersionForAutomaticUpdate();
            await CreateOrUpdateDvrFileAsync(version, showNoRemoteConfigDialog, cancellationToken);
        }

        public static async Task CreateOrUpdateDvrFileAsync(DvrScriptVersion scriptVersion,
            bool showNoRemoteConfigDialog,
            CancellationToken cancellationToken = default)
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
            UpdateProcessProgressId = Progress.Start("Updating DVR File");
            Progress.RegisterCancelCallback(UpdateProcessProgressId, () =>
            {
                timeoutCts.Cancel();
                return false;
            });
            try
            {
                Progress.Report(UpdateProcessProgressId, .3f, "Fetch remote config");
                var dvrDefinition = await RemoteDatabase.GetDvrCodeGenModelAsync(linkedCts.Token);
                Progress.Report(UpdateProcessProgressId, .8f, "Write file to disk");

                var generator = GetFileGenerator(scriptVersion);

                var dvrFileContent = generator.GetDvrFileContent(dvrDefinition);
                Database.WriteDvrFile(dvrFileContent, scriptVersion);
            }
            catch (AggregateException aggregateException)
            {
                var itemNoMatchException = (DvrItemNoMatchException)
                    aggregateException.InnerExceptions.FirstOrDefault(e => e is DvrItemNoMatchException);

                if (itemNoMatchException != null && showNoRemoteConfigDialog)
                    DisplayNoRemoteConfigDialog();
                foreach (var innerException in aggregateException.InnerExceptions)
                    HomaGamesLog.Error("[N-Testing] Exception when trying to fetch DVR values:" + innerException);
            }
            catch (OperationCanceledException)
            {
                HomaGamesLog.Debug("DVR Script update cancelled.");
            }
            catch (DvrItemNoMatchException)
            {
                if (showNoRemoteConfigDialog)
                    DisplayNoRemoteConfigDialog();
            }
            catch (Exception e)
            {
                HomaGamesLog.Error("[N-Testing] Exception when trying to fetch DVR values:" + e);
            }
            finally
            {
                Progress.UnregisterCancelCallback(UpdateProcessProgressId);
                Progress.Remove(UpdateProcessProgressId);
            }
        }

        private static IDvrScriptGenerator GetFileGenerator(DvrScriptVersion version)
        {
            return version switch
            {
                DvrScriptVersion.Unknown => throw new Exception("DVR script version is none"),
                DvrScriptVersion.DynamicVariable => new DynamicVariableDvrScriptGenerator(),
                DvrScriptVersion.Observables => new ObservableDvrScriptGenerator(),
                _ => throw new NotImplementedException($"Code generator {version} is not implemented.")
            };
        }

        private static void DisplayNoRemoteConfigDialog()
        {
            EditorUtility.DisplayDialog("Error while fetching N-Testing values",
                "No remote configuration found for N-Testing for your game. " +
                "If you are using N-Testing, please contact your Publish Manager. " +
                "Otherwise you can safely ignore this error.", "OK");
        }
    }
}