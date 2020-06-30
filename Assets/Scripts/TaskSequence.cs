using UnityEngine;
using System.Collections;
using System;
using SpeechIO;
using System.Threading.Tasks;

public class TaskSequence : MonoBehaviour
{


    private int numberOfTasks;
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

    public TextAsset csvFile;
    public int taskChunkSize;

    // Use this for initialization
    async void Start()
    {
        speech = new SpeechOut();
        target = GameObject.Find("Target");
        manager = GameObject.Find("Manager");
        ReadProtocol();
        await Task.Delay(1000);
        NextTask();
    }

    void ReadProtocol()
    {
        string[] records = csvFile.text.Split('\n');
        foreach (string record in records)
        {
            string[] fields = record.Split(',');
            StudyTask t = new StudyTask(GenerateSpawnPosition(), GenerateSpawnPosition(), 10, 0.2, true, 45, 1);
            tasks.Add(t);
        }
    }

    async void NextTask()
    {
        if (currentTaskId < tasks.Count)
        {
            StudyObstacleManager om = manager.gameObject.GetComponent<StudyObstacleManager>();
            om.DisableAll();
            StudyTask t = (StudyTask)tasks[currentTaskId];
            PantoHandle handle = (PantoHandle)GameObject.Find("Panto").GetComponent<LowerHandle>();
            await handle.MoveToPosition(t.startPos, 0.3f, true);
            target = om.ReEnableTarget();
            target.transform.position = t.targetPos;
            target.transform.localScale = new Vector3(t.targetSize, t.targetSize, t.targetSize);
            if (t.guidesEnabled)
            {
                om.ReEnableRails();
            }
            //enable obstacles and rails
            await speech.Speak("3, 2, 1, Go", 0.5f);
            startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            isRunning = true;
        } else
        {
            Debug.Log("Finished study - continue with questionnaire");
        }
    }

    private Vector3 GenerateSpawnPosition()
    {
        float randomPosX = UnityEngine.Random.Range(-spawnRange, spawnRange);
        float randomPosZ = -10 + UnityEngine.Random.Range(-spawnRange, spawnRange);
        Vector3 randomPos = new Vector3(randomPosX, 1, randomPosZ);
        return randomPos;
    }

    public void StopTask()
    {
        //if Target detected collision ->
        if (isRunning)
        {
            StudyTask t = (StudyTask)tasks[currentTaskId];
            //disable obstacles and rails
            isRunning = false;
            t.time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startTime;
            Debug.Log("Task finished in " + t.time);
            currentTaskId++;
            NextTask();
        }

    }

}

public class StudyTask
{
    public Vector3 targetPos;
    public Vector3 startPos;
    public int guideLength;
    public double guideWidth;
    public bool guidesEnabled;
    public int guideAngle;
    public int targetSize;
    public long time;

    public StudyTask(Vector3 targetPos, Vector3 startPos, int guideLength, double guideWidth, bool guidesEnabled, int guideAngle, int targetSize)
    {
        this.targetPos = targetPos;
        this.startPos = startPos;
        this.guideLength = guideLength;
        this.guideWidth = guideWidth;
        this.guidesEnabled = guidesEnabled;
        this.guideAngle = guideAngle;
        this.targetSize = targetSize;
        this.time = -1;
    }

    public override string ToString()
    {
        return "......";
    }
}