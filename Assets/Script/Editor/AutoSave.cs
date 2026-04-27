using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class AutoSave
{
    private static double nextSaveTime;

    static AutoSave()
    {
        nextSaveTime = EditorApplication.timeSinceStartup + 60;
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        // ADDED: Don't run if the game is playing or switching modes
        if (EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (EditorApplication.timeSinceStartup > nextSaveTime)
        {
            SaveProject();
            nextSaveTime = EditorApplication.timeSinceStartup + 60;
        }
    }

    private static void SaveProject()
    {
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("Auto-saved scene and assets at: " + System.DateTime.Now.ToString("HH:mm:ss"));
    }
}