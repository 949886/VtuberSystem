using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class AutoSave : EditorWindow
{

    private bool autoSaveScene = true;
    private bool showMessage = false;
    private bool isStarted = false;
    private int intervalScene = 1;
    private DateTime lastSaveTimeScene;

    private string projectPath;
    private string scenePath;

    [MenuItem("Window/AutoSave")]
    static void Init()
    {
        AutoSave saveWindow = (AutoSave)GetWindow(typeof(AutoSave));
        saveWindow.Show();
    }
    
    void OnEnable()
    {
        lastSaveTimeScene = DateTime.Now;
        projectPath = Application.dataPath;
        scenePath = EditorSceneManager.GetActiveScene().path;
    }

    void OnGUI()
    {
        GUILayout.Label("Info:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Saving to:", "" + projectPath);
        EditorGUILayout.LabelField("Saving scene:", "" + scenePath);
        GUILayout.Label("Options:", EditorStyles.boldLabel);
        autoSaveScene = EditorGUILayout.BeginToggleGroup("Auto save", autoSaveScene);
        intervalScene = EditorGUILayout.IntSlider("Interval (minutes)", intervalScene, 1, 10);
        if (isStarted)
            EditorGUILayout.LabelField("Last save:", "" + lastSaveTimeScene);
        EditorGUILayout.EndToggleGroup();
        showMessage = EditorGUILayout.BeginToggleGroup("Show LiveMessage", showMessage);
        EditorGUILayout.EndToggleGroup();
    }


    void Update()
    {
        if (autoSaveScene)
        {
            if (DateTime.Now.Minute >= (lastSaveTimeScene.Minute + intervalScene) || 
                DateTime.Now.Minute == 59 && DateTime.Now.Second == 59)
                saveScene();
        }
        else  isStarted = false;
    }

    void saveScene()
    {
        if (EditorApplication.isPlaying)
            return;

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.SaveScene(activeScene);
        lastSaveTimeScene = DateTime.Now;
        isStarted = true;
        AutoSave repaintSaveWindow = (AutoSave)GetWindow(typeof(AutoSave));
        repaintSaveWindow.Repaint();
        if (showMessage)
            Debug.Log("AutoSave saved: " + scenePath + " on " + lastSaveTimeScene);
    }
}
