using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildManager : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    private void SetAndroidKeystore()
    {
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = Application.dataPath + "/1317+oSRZunity.keystore";
        Debug.Log(PlayerSettings.Android.keystoreName);
        PlayerSettings.Android.keystorePass = "1317+oSRZunity";
        PlayerSettings.Android.keyaliasName = "release";
        PlayerSettings.Android.keyaliasPass = "1317+oSRZunity";
    }
    
    
    public int callbackOrder { get; }
    public void OnPreprocessBuild(BuildReport report)
    {
        SetAndroidKeystore();
        Debug.LogWarning("Build Started");
    }
    public void OnPostprocessBuild(BuildReport report)
    {
        string oldPath = report.summary.outputPath;
        string directory = Path.GetDirectoryName(oldPath);
        string extension = Path.GetExtension(oldPath);
        
        
        
        string newPath = directory + "/" + Application.productName + " " + Application.version + " (" + PlayerSettings.Android.bundleVersionCode + ")" + extension; 
        
        System.IO.File.Copy(oldPath, newPath);
        
        
        
        PlayerSettings.Android.bundleVersionCode++;
    }
}
