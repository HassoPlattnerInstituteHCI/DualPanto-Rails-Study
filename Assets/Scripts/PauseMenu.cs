// To use this example, attach this script to an empty GameObject.
// Create three buttons (Create>UI>Button). Next, select your
// empty GameObject in the Hierarchy and click and drag each of your
// Buttons from the Hierarchy to the Your First Button, Your Second Button
// and Your Third Button fields in the Inspector.
// Click each Button in Play Mode to output their message to the console.
// Note that click means press down and then release.

using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public Button continueButton;

    void Start()
    {
        continueButton.onClick.AddListener(Continue);
    }

    void Continue()
    {
        GameObject uiManager = GameObject.Find("UI");
        uiManager.GetComponent<StudyUIManager>().ContinueStudy();
    }

}