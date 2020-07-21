﻿using UnityEngine;
using System.Collections;
using System;
using SpeechIO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

public class TaskSequence : MonoBehaviour
{

    private StudyObstacleManager obstacleManager;
    private StudyUIManager studyUIManager;
    private long startTime;
    private Dictionary<int, List<StudyTask>> tasks = new Dictionary<int, List<StudyTask>>();
    //private ArrayList tasks = new ArrayList(); // TODO: put the tasks into chunks
    private Dictionary<int, List<string>> questionnaireAnswers = new Dictionary<int, List<string>>(); // ["agency","easiness"] 
    private int currentTaskInChunk = 0;
    private bool isRunning = false;
    private SpeechOut speech;
    private int userId = -1;
    private int taskCount = 0;
    
    AudioSource audioSource;
    public TextAsset csvFile;
    public int currentTaskId = 0;
    public int taskChunkSize;
    public int currentChunkId = -1; // allow to start at a later block if the apparatus crashes during execution
    // start with chunk -1 (test runs)
    // Use this for initialization
    void Start()
    {
        speech = new SpeechOut();
        obstacleManager = GameObject.Find("Manager").GetComponent<StudyObstacleManager>();
        studyUIManager = GameObject.Find("UI").GetComponent<StudyUIManager>();
        audioSource = GetComponent<AudioSource>();
        Time.timeScale = 1;
        ReadProtocol();
        studyUIManager.ShowIntroMenu();
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
            if (tasks.ContainsKey(blockId))
            {
                tasks[blockId].Add(t);
            } else
            {
                tasks.Add(blockId, new List<StudyTask> { t });
            }
            taskCount++;
        }
    }

    async public void NextTask()
    {
        if(currentTaskId == taskCount)
        {
            Debug.Log("Finished study");
            Debug.Log(taskCount);
            speech.Speak("Well done, you've completed the user study. Thanks for participating!", 1);
            studyUIManager.FinishStudy();
        } else
        {
            StudyTask t = tasks[currentChunkId][currentTaskInChunk];
            Debug.Log("New task " + t.ToString());
            obstacleManager.DisableAll();
            PantoHandle handle = GameObject.Find("Panto").GetComponent<LowerHandle>();
            await Task.Delay(1000);
            Vector3 s = new Vector3(
                2,
                0,
                -5);
            //Debug.Log("Move to position");
            await handle.MoveToPosition(t.startPos, 0.005f, false);
            //await handle.MoveToPosition(s, 0.005f, true);

            await speech.Speak("3, 2, 1, Go", 0.5f);
            handle.Free();
            obstacleManager.ReEnableTarget(t.targetPos, new Vector3(t.targetSize, t.targetSize, t.targetSize));

            if (t.guidesEnabled)
            {
                obstacleManager.ReEnableRails(t.targetPos, t.guideWidth, t.guideLength);
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
            obstacleManager.TriggerWallsForButton();
        }
    }

    
    public async void ButtonWallPressed(GameObject buttonWall)
    {

        Debug.Log("Walls active " + obstacleManager.buttonWallsActive);
        if (obstacleManager.buttonWallsActive)
        {
            StudyTask t = tasks[currentChunkId][currentTaskInChunk];
            // if the right wall is touched then 
            t.foundTargtDeliberately = buttonWall.name == "Button wall right";
            obstacleManager.TriggerWallsForButton();
            // move me handle back to the middle
            PantoHandle handle = GameObject.Find("Panto").GetComponent<LowerHandle>();
            Vector3 s = new Vector3(
                0,
                0,
                -5);
            await handle.MoveToPosition(s, 0.005f, false);

            if (currentTaskId % taskChunkSize != 0)
            {
                NextTask();
            }
            else
            {
                // after training phase don't show questionnaire
                if (currentChunkId == -1)
                {
                    speech.Speak("Training completed.", 1);
                    studyUIManager.ShowStartMenu();
                }
                else
                {
                    speech.Speak("Sequence completed. Please rate the statements on the screen.", 1);
                    studyUIManager.ShowQuestionnaire();
                }
            }
        }
    }

    public void LogTimeToRail()
    {
        if (isRunning)
        {
            StudyTask t = tasks[currentChunkId][currentTaskInChunk];
            long timeToRail = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - startTime;
            // make sure to have a min time for the time to rail, otherwise the collision might be detected right when the rail is spawned
            if (t.timeToRail == -1 && timeToRail > 30)
            {
                t.timeToRail = timeToRail;
            }
        }
    }


    public void ExportResults(List<string> answers)
    {
        string path = "studyResults_" + userId + ".csv";
        if (!File.Exists(path))
        {
            // add header to csv file (watch the order of attributes and questionnaire answers
            string header = "UserId, TaskId, BlockId, TargetX, TargetY, StartX, StartY, GuideLength, Time, TimeToRail, Agency, Easiness, Found Target";
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(header);
            }
        }

        using (StreamWriter writer = File.AppendText(path))
        {
            foreach (StudyTask t in tasks[currentChunkId])
            {
                // append the results from the questionnaire
                string res = t.ToString(answers);
                writer.WriteLine(res);
            }
        }
    }

    public void NextChunk()
    {
        currentChunkId++;
        currentTaskInChunk = 0;

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
    public long timeToRail;
    public bool foundTargtDeliberately;

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
        timeToRail = -1;
        foundTargtDeliberately = false;
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
            time.ToString(),
            timeToRail.ToString(),
            foundTargtDeliberately.ToString()
        };
        // at the end append the answers of the questionnaire in the order of appearance
        attrs.AddRange(answers);
        return string.Join(",", attrs.ToArray());
    }
}