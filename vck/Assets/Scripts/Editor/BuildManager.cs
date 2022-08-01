using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

public static class BuildManager 
{
    public const string DesktopSymbol = "IS_DESKTOP";
    public const string WebglSymbol = "IS_WEGBL";
    public const string SetEditorToDesktop = "Product/Set Editor to Webgl Build";

    [MenuItem("Build/OSX")]
    public static void BuildOSX()
    {
        DoBuild(BuildTarget.StandaloneOSX);
    }

    [MenuItem("Build/Windows")]
    public static void BuildWindows()
    {
        DoBuild(BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Build/WebGL")]
    public static void BuildWebGL()
    {
        DoBuild(BuildTarget.WebGL);
    }

    private static void DoBuild(BuildTarget platform)
    {
        string path = EditorUtility.SaveFolderPanel("Choose Build Location", "", "");
        if (path.Length == 0) return;
        string[] scenes = new string[] {
            "Assets/Scenes/title.unity",
            "Assets/Scenes/testing.unity"
            };

        BuildPlayerOptions bpo = new BuildPlayerOptions();
        bpo.scenes = scenes;
        bpo.target = platform;
        bpo.locationPathName = path + "/VCK";
        PlayerSettings.productName = "Vancouver Child Kicker: The Game";
        if (platform != BuildTarget.WebGL)
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "IS_DESKTOP");
        else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, "IS_WEBGL");
        BuildReport report = BuildPipeline.BuildPlayer(bpo);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build Successful - " + summary.outputPath);
        }
        else
        {
            Debug.LogError("Build failed - " + summary.totalErrors);
        }
    }

    [MenuItem(SetEditorToDesktop)]
    public static void EditorSelectWebGL()
    {
        ToggleEditorBuildStatus(SetEditorToDesktop, false);
    }

    [MenuItem(SetEditorToDesktop, true)]
    public static bool EditorSelectWebGLValidate()
    {
        ToggleEditorBuildStatus(SetEditorToDesktop, true);
        return true;
    }

    public static void ToggleEditorBuildStatus(string menuPath, bool validateOnly)
    {
        bool desktopDefined = false;
        bool webglDefined = false;
        string newDefineSymbols = validateOnly ? null : string.Empty;
        string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        string[] defineSymbolsArray = defineSymbols.Split(';');
        foreach (string defineSymbol in defineSymbolsArray)
        {
            string trimmedDefineSymbol = defineSymbol.Trim();
            if (trimmedDefineSymbol == DesktopSymbol)
            {
                desktopDefined = true;
                continue;
            }
            else if (trimmedDefineSymbol == WebglSymbol)
            {
                webglDefined = true;
                continue;
            }
            if (!validateOnly)
            {
                newDefineSymbols += string.Format("{0};", trimmedDefineSymbol);
            }
        }
        if ((desktopDefined && webglDefined) || (!desktopDefined && !webglDefined))
        {
            // Oops, either both defines were true, or both were false. Default to RSET
            webglDefined = true; // Set freestyle as active so it toggles to RSET below
            desktopDefined = false;
            Debug.LogError("PLATFORM DEFINES ERROR: Both IS_DESKTOP and IS_WEBGL are defined!");
        }
        if (!validateOnly)
        {
            if (webglDefined)
                newDefineSymbols += string.Format("{0};", DesktopSymbol);
            else
                newDefineSymbols += string.Format("{0};", WebglSymbol);
            webglDefined = !webglDefined;
            Debug.LogWarning("New defines: " + newDefineSymbols);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefineSymbols);
        }
        Menu.SetChecked(menuPath, webglDefined);
    }
}
