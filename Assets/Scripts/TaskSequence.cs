using UnityEngine;
using System.Collections;
using System;
using SpeechIO;
using System.Threading.Tasks;

public class TaskSequence : MonoBehaviour
{

    //TODO: spawn using protocol positions and angles
    private float spawnRange = 2f;
    private GameObject target;
    private GameObject manager;
    private ArrayList durations = new ArrayList();
    private long startTime;
    private ArrayList tasks = new ArrayList();
    private int currentTaskId;
    private bool isRunning = false;
    private SpeechOut speech;
    GameObject[] startObjects;
    GameObject[] pauseObjects;
    GameObject[] finishObjects;

    AudioSource audioSource;
    public TextAsset csvFile;
    public int taskChunkSize;

    // Use this for initialization
    void Start()
    {
        speech = new SpeechOut();
        target = GameObject.Find("Target");
        manager = GameObject.Find("Manager");
        startObjects = GameObject.FindGameObjectsWithTag("ShowOnStart");
        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        finishObjects = GameObject.FindGameObjectsWithTag("ShowOnFinish");
        audioSource = GetComponent<AudioSource>();
        Time.timeScale = 1;
        ReadProtocol();
        ShowStartMenu();
    }

    void ReadProtocol()
    {
        string[] records = csvFile.text.Split('\n');
        for(int i= 1;i < records.Length;i++)
        {
            string record = records[i];
            string[] fields = record.Split(',');
            
            Vector3 targetPos = new Vector3(
                float.Parse(fields[5],System.Globalization.CultureInfo.InvariantCulture),
                0,
                float.Parse(fields[6], System.Globalization.CultureInfo.InvariantCulture));
            Vector3 startPos = new Vector3(
                float.Parse(fields[3], System.Globalization.CultureInfo.InvariantCulture),
                0,
                float.Parse(fields[4], System.Globalization.CultureInfo.InvariantCulture));
            //int railAngle = UnityEngine.Random.Range(0, 6) * 15;
            float railWidth = UnityEngine.Random.Range(1, 4) * 0.1f;
            //float targetSize = UnityEngine.Random.Range(0, 3) * 0.5f + 1;
            StudyTask t = new StudyTask(0, 1, 0, targetPos, startPos, 0.2f, true, Int32.Parse(fields[9]), 1);
            tasks.Add(t);
        }
    }

    async void NextTask()
    {
        if(currentTaskId == tasks.Count)
        {
            Debug.Log("Finished study");
            Debug.Log(tasks.Count);
            speech.Speak("Well done, you've completed the user study. Thanks for participating!", 1);
            FinishStudy();
        } else
        {
            StudyObstacleManager om = manager.gameObject.GetComponent<StudyObstacleManager>();
            om.DisableAll();
            StudyTask t = (StudyTask)tasks[currentTaskId];
            PantoHandle handle = (PantoHandle)GameObject.Find("Panto").GetComponent<LowerHandle>();
            await Task.Delay(1000);
            Vector3 s = new Vector3(
                5,
                0,
                -5);
            //await handle.MoveToPosition(t.startPos, 0.005f, true);
            await handle.MoveToPosition(s, 0.005f, true);
            target = om.ReEnableTarget(t.targetPos, new Vector3(t.targetSize, t.targetSize, t.targetSize));
            

            if (t.guidesEnabled)
            {
                om.ReEnableRails(t.targetPos, t.guideWidth, t.guideLength);
            }
            Debug.Log("re enabled objects");
            await Task.Delay(1000);
            Debug.Log("waited for a second");
            //enable obstacles and rails
            await speech.Speak("3, 2, 1, Go", 0.5f);
            startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            isRunning = true;
        }
        
    }

    private Vector3 GenerateSpawnPosition()
    {
        float randomPosX = UnityEngine.Random.Range(-spawnRange, spawnRange);
        float randomPosZ = -6 + UnityEngine.Random.Range(-spawnRange, spawnRange);
        Vector3 randomPos = new Vector3(randomPosX, 1, randomPosZ);
        return randomPos;
    }

    public void StopTask()
    {
        //if Target detected collision ->
        if (isRunning)
        {
            audioSource.Play();
            isRunning = false;
            StudyTask t = (StudyTask)tasks[currentTaskId];
            //disable obstacles and rails
            t.time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startTime;
            Debug.Log("Task finished in " + t.time);
            currentTaskId++;
            if (currentTaskId % taskChunkSize != 0)
            {
                NextTask();
            }
            else
            {
                speech.Speak("Chunk completed", 1);
                Debug.Log("Finished chunk - continue with questionnaire");
                ShowQuestionnaire();
            }
        }

    }

    private void ShowStartMenu()
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

    //hides objects with ShowOnPause tag
    public void ContinueStudy()
    {
        Time.timeScale = 1;
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
        NextTask();
    }

}

public class StudyTask
{
    public int userId;
    public int taskId;
    public int blockId;
    public Vector3 targetPos;
    public Vector3 startPos;
    public float guideWidth;
    public bool guidesEnabled;
    public int guideLength;
    public float targetSize;
    public long time;

    public StudyTask(int userId, int taskId, int blockId, Vector3 targetPos, Vector3 startPos, float guideWidth, bool guidesEnabled, int guideLength, float targetSize)
    {
        this.userId = userId;
        this.taskId = taskId;
        this.blockId = blockId;
        this.targetPos = targetPos;
        this.startPos = startPos;
        this.guideWidth = guideWidth;
        this.guidesEnabled = guidesEnabled;
        this.guideLength = guideLength;
        this.targetSize = targetSize;
        this.time = -1;
    }

    public override string ToString()
    {
        return "......";
    }
}