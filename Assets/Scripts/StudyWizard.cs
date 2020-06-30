using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StudyWizard : MonoBehaviour
{
    StudyUI window;
    async void Start()
    {
        window = ScriptableObject.CreateInstance<StudyUI>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
        window.ShowPopup();
    }


 }

class StudyUI : EditorWindow
{
    void OnGUI()
    {
        EditorGUILayout.LabelField("Welcome to the user study", EditorStyles.wordWrappedLabel);
        GUILayout.Space(40);
        if (GUILayout.Button("Start"))
        {
            
        }
        if (GUILayout.Button("End study"))
        {
            OnStopApp();
            Close();
            GUIUtility.ExitGUI();
        }
    }

    void OnStopApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

}