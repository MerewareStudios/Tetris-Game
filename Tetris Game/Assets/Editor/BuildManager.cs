using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildManager : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder { get; }
    public void OnPreprocessBuild(BuildReport report)
    {
        Const @const = (Const)AssetDatabase.LoadAssetAtPath("Assets/Resources/Game Constants.asset", typeof(ScriptableObject));
        @const.bundleVersionCode = PlayerSettings.Android.bundleVersionCode;
        Debug.LogWarning("Build Started");
    }
    public void OnPostprocessBuild(BuildReport report)
    {
        Const @const = (Const)AssetDatabase.LoadAssetAtPath("Assets/Resources/Game Constants.asset", typeof(ScriptableObject));
        @const.bundleVersionCode = PlayerSettings.Android.bundleVersionCode;
        
        
        
        string oldPath = report.summary.outputPath;
        string directory = Path.GetDirectoryName(oldPath);
        string extension = Path.GetExtension(oldPath);
        
        
        
        string newPath = directory + "/" + Application.productName + " " + Application.version + " (" + @const.bundleVersionCode + ")" + extension; 
        
        System.IO.File.Copy(oldPath, newPath);
        
        
        
        PlayerSettings.Android.bundleVersionCode++;
        @const.bundleVersionCode = PlayerSettings.Android.bundleVersionCode;
    }
}
