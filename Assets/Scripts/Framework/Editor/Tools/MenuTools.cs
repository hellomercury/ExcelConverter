using UnityEditor;
using UnityEngine;

public class MenuTools
{
    #region Open folder
    [MenuItem("Framework/Open Files/PersistentData Folder", priority = 50)]
    static void OpenPersistentData()
    {
        System.Diagnostics.Process.Start(Application.persistentDataPath);
    }

    [MenuItem("Framework/Open Files/Assets Folder", priority = 53)]
    static void OpenAssets()
    {
        System.Diagnostics.Process.Start(Application.dataPath);
    }

    [MenuItem("Framework/Open Files/StreamingAssets Folder", priority = 55)]
    static void OpenStreamingAssets()
    {
        System.Diagnostics.Process.Start(Application.streamingAssetsPath);
    }
    #endregion

    #region Local data
    [MenuItem("Framework/Local Player Data/Clear PlayerPrefs", priority = 25)]
    static void ClearPlayPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
    #endregion
}