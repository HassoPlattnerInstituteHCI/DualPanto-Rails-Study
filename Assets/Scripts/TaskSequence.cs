using UnityEngine;
using System.Collections;
using System;
using SpeechIO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

public class TaskSequence : MonoBehaviour
{

    private GameObject target;
    private GameObject manager;
    private long startTime;
    private Dictionary<int, List<StudyTask>> tasks = new Dictionary<int, List<StudyTask>>();
    //private ArrayList tasks = new ArrayList(); // TODO: put the tasks into chunks
    private Dictionary<int, List<string>> questionnaireAnswers = new Dictionary<int, List<string>>(); // ["agency","easiness"] 
    private int currentTaskId = 0;
    private int currentChunkId = 0;
    private int currentTaskInChunk = 0;
    private bool isRunning = false;
    private SpeechOut speech;
    GameObject[] startObjects;
    GameObject[] pauseObjects;
    GameObject[] finishObjects;
    private int userId = -1;
    private int taskCount = 0;

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
            if(fields.Length <= 1)
            {
                continue;
            }
            if (userId == -1)
            {
                userId = int.Parse(fields[0]);
            } 
            int blockId = int.Parse(fields[1]);
            int taskId = int.Parse(fields[2]);
            Vector3 targetPos = new Vector3(
                float.Parse(fields[5],System.Globalization.CultureInfo.InvariantCulture),
                0,
                float.Parse(fields[6], System.Globalization.CultureInfo.InvariantCulture));
            Vector3 startPos = new Vector3(
                float.Parse(fields[3], System.Globalization.CultureInfo.InvariantCulture),
                0,
                float.Parse(fields[4], System.Globalization.CultureInfo.InvariantCulture));
            StudyTask t = new StudyTask(userId, taskId, blockId, targetPos, startPos, 0.2f, true, Int32.Parse(fields[9]), 1);
            //Debug.Log(blockId);
            if (tasks.ContainsKey(blockId))
            {
                tasks[blockId].Add(t);
            } else
            {
                tasks.Add(blockId, new List<StudyTask> { t });
            }
            //Debug.Log(tasks);
            taskCount++;
        }
    }

    async void NextTask()
    {
        if(currentTaskId == taskCount)
        {
            Debug.Log("Finished study");
            Debug.Log(taskCount);
            speech.Speak("Well done, you've completed the user study. Thanks for participating!", 1);
            FinishStudy();
        } else
        {
            StudyObstacleManager om = manager.gameObject.GetComponent<StudyObstacleManager>();
            om.DisableAll();
            StudyTask t = tasks[currentChunkId][currentTaskInChunk];
            PantoHandle handle = GameObject.Find("Panto").GetComponent<LowerHandle>();
            await Task.Delay(1000);
            Vector3 s = new Vector3(
                2,
                0,
                -5);
            await handle.MoveToPosition(t.startPos, 0.005f, true);
            //await handle.MoveToPosition(s, 0.005f, true);
            
            await speech.Speak("3, 2, 1, Go", 0.5f);
            target = om.ReEnableTarget(t.targetPos, new Vector3(t.targetSize, t.targetSize, t.targetSize));

            if (t.guidesEnabled)
            {
                om.ReEnableRails(t.targetPos, t.guideWidth, t.guideLength);
            }
            
            startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            isRunning = true;
        }
        
    }

    public void StopTask()
    {
        //if Target detected collision ->
        if (isRunning)
        {
            audioSource.Play();
            isRunning = false;
            StudyTask t = tasks[currentChunkId][currentTaskInChunk];
            t.time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startTime;
            Debug.Log("Task finished in " + t.time);
            currentTaskId++;
            currentTaskInChunk++;
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

    public void FinishQuestionnaire()
    {
        string agency = GameObject.Find("AgencySlider").GetComponent<Slider>().value.ToString();
        string easiness = GameObject.Find("EasinessSlider").GetComponent<Slider>().value.ToString();
        questionnaireAnswers[currentChunkId] = new List<string> { agency, easiness };
        ExportResults();
        currentChunkId++;
        currentTaskInChunk = 0;
    }

    //hides objects with ShowOnPause tag
    public void ContinueStudy()
    {
        // except of the first run always evaluate the questionnaire after a chunk
        if (currentTaskId > 0)
        {
            FinishQuestionnaire();
        }
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

    private void ExportResults()
    {
        string path = "studyResults_" + userId + ".csv";
        if (!File.Exists(path))
        {
            // add header to csv file (watch the order of attributes and questionnaire answers
            string header = "UserId, TaskId, BlockId, TargetX, TargetY, StartX, StartY, GuideLength, Time, Agency, Easiness";
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(header);
            }
        }

        using (StreamWriter writer = File.AppendText(path))
        {
            Debug.Log("Current chunk id");
            Debug.Log(currentChunkId);
            foreach (StudyTask t in tasks[currentChunkId])
            {
                // append the results from the questionnaire
                List<string> answers = questionnaireAnswers[t.blockId];
                string res = t.ToString(answers);
                Debug.Log(res);
                writer.WriteLine(res + "\n");
            }
        }
        Debug.Log("Wrote to file");
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
        time = -1;
    }

    public string ToString(List<string> answers)
    {
        // order of attributes is the same as in the object
        // order is userId, taskId, blockId, targetX, targetY, startX, startY, guideLength, time
        ArrayList attrs = new ArrayList{
            userId.ToString(),
            taskId.ToString(),
            blockId.ToString(),
            targetPos.x.ToString(),
            targetPos.z.ToString(),
            startPos.x.ToString(),
            startPos.z.ToString(),
            guideLength.ToString(),
            time.ToString()
        };
        // at the end append the answers to the questions in the order of appearance
        attrs.AddRange(answers);
        return string.Join(",", attrs.ToArray());
    }
}