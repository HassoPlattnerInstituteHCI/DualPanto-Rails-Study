using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StudyUIManager : MonoBehaviour
{
    GameObject[] introObjects;
    GameObject[] startObjects; // shown after the training phase, before start of the actual study
    GameObject[] pauseObjects;
    GameObject[] finishObjects;
    GameObject blackScreen;
    TaskSequence studyWizard;

    void Start()
    {
        introObjects = GameObject.FindGameObjectsWithTag("ShowOnIntro");
        startObjects = GameObject.FindGameObjectsWithTag("ShowOnStart");
        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        finishObjects = GameObject.FindGameObjectsWithTag("ShowOnFinish");
        blackScreen = GameObject.Find("Black Screen");
        studyWizard = GameObject.Find("Study Wizard").GetComponent<TaskSequence>();
    }

    public void ContinueStudy()
    {
        // this method is called after the intro, the start and the questionnaire menu
        // except of the intro, we always want to save the results in the csv file
        if (studyWizard.currentTaskId == 0)
        {
            HideUIAndResume();
            return;
        }
        if (studyWizard.currentChunkId != -1)
        {
            FinishQuestionnaire();
        }
        studyWizard.NextChunk();
        HideUIAndResume();
    }

    public void HideUIAndResume()
    {
        Time.timeScale = 1;
        foreach (GameObject g in introObjects)
        {
            g.SetActive(false);
        }
        foreach (GameObject g in startObjects)
        {
            g.SetActive(false);
        }
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(false);
        }
        foreach (GameObject g in finishObjects)
        {
            g.SetActive(false);
        }
        blackScreen.SetActive(true);
        studyWizard.NextTask();
    }


    public void FinishQuestionnaire()
    {
        Slider agencySlider = GameObject.Find("AgencySlider").GetComponent<Slider>();
        Slider easinessSlider = GameObject.Find("EasinessSlider").GetComponent<Slider>();

        string agency = agencySlider.value.ToString();
        string easiness = easinessSlider.value.ToString();
        studyWizard.ExportResults(new List<string> { agency, easiness });
        agencySlider.SetValueWithoutNotify(3);
        easinessSlider.SetValueWithoutNotify(3);
    }

    // the first menu that the user sees in which the study is explained
    public void ShowIntroMenu()
    {
        Time.timeScale = 0;
        foreach (GameObject g in introObjects)
        {
            g.SetActive(true);
        }
    }

    // the actual start menu that shows up after the training phase
    public void ShowStartMenu()
    {
        Time.timeScale = 0;
        foreach (GameObject g in startObjects)
        {
            g.SetActive(true);
        }
    }


    //shows objects with ShowOnPause tag
    public void ShowQuestionnaire()
    {
        Time.timeScale = 0;
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(true);
        }
    }


    public void FinishStudy()
    {
        Time.timeScale = 0;
        //export task data as csv
        foreach (GameObject g in finishObjects)
        {
            g.SetActive(true);
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            blackScreen.SetActive(false);
        }
        else if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
        {
            blackScreen.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            studyWizard.ArrowKeyPressed(false);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            studyWizard.ArrowKeyPressed(true);
        }
    }
}
