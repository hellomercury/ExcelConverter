using UnityEditor;
using UnityEngine;

namespace  Szn.Editor.Menu
{
    public class FolderMenu
    {
        #region Open folder
        [MenuItem("Tools/Open Files/PersistentData Folder", priority = 50)]
        static void OpenPersistentData()
        {
            System.Diagnostics.Process.Start(Application.persistentDataPath);
        }

        [MenuItem("Tools/Open Files/Assets Folder", priority = 53)]
        static void OpenAssets()
        {
            System.Diagnostics.Process.Start(Application.dataPath);
        }

        [MenuItem("Tools/Open Files/StreamingAssets Folder", priority = 55)]
        static void OpenStreamingAssets()
        {
            System.Diagnostics.Process.Start(Application.streamingAssetsPath);
        }
        #endregion
    }
}
