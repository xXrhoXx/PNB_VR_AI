using System.Diagnostics;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class AutoSave
{
    private static double nextSaveTime;
    private const double SaveInterval = 300; // 5 minutes in seconds

    static AutoSave()
    {
        nextSaveTime = EditorApplication.timeSinceStartup + SaveInterval;
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
            //SaveProject();
            SaveProjectAndCommit();
            nextSaveTime = EditorApplication.timeSinceStartup + SaveInterval;
        }
    }

    private static void SaveProject()
    {
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        UnityEngine.Debug.Log("Auto-saved scene and assets at: " + System.DateTime.Now.ToString("HH:mm:ss"));
    }

    private static void SaveProjectAndCommit()
    {
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        UnityEngine.Debug.Log($"<color=cyan>AutoSave:</color> Project saved. Starting background commit...");

        // We chain the commands using && (only commit if add succeeds)
        // The "quotation" marks around the message are escaped for the shell
        string chainedArgs = $"/c git add . && git commit -m \"Auto-save commit at {timestamp}\"";

        RunGit(chainedArgs);
    }

    private static void RunGit(string args)
    {
        try
        {
            // We use "cmd.exe" (Windows) or "/bin/sh" (Mac/Linux) to handle the command chaining
            string shell = Application.platform == RuntimePlatform.WindowsEditor ? "cmd.exe" : "/bin/sh";
            string prefix = Application.platform == RuntimePlatform.WindowsEditor ? "" : "-c ";

            ProcessStartInfo startInfo = new ProcessStartInfo(shell)
            {
                Arguments = prefix + args,
                WorkingDirectory = Application.dataPath + "/..",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = false
            };

            Process.Start(startInfo);
            // We DON'T use WaitForExit() here so Unity won't hang
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning("Git Background Process failed: " + e.Message);
        }
    }
}

/*
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
*/