using System;
using System.Collections;
using System.Threading.Tasks;
using DualPantoFramework;
using UnityEngine;


public class StudyObstacleManager : MonoBehaviour
{
    private GameObject target;
    GameObject[] rails;
    private GameObject forceField;
    GameObject leftWall;
    GameObject rightWall;
    GameObject topWall;
    GameObject bottomWall;
    private float roomHeight;
    private float roomWidth;
    private float verticalRoomCenter;
    async void Start()
    {
        target = GameObject.Find("Target");
        forceField = GameObject.Find("ForceField");
        DisableForceField();
        rails = GameObject.FindGameObjectsWithTag("Rail");
        Debug.Log("Starting obstacle manager");
        // if we register obstacles too early, the device will not work any longer (only sync debug logs will be printed
        // I am working on fixing this, but for now just add a wait
        await Task.Delay(1000);
        leftWall = GameObject.Find("Wall left");
        topWall = GameObject.Find("Wall top");
        rightWall = GameObject.Find("Wall right");
        bottomWall = GameObject.Find("Wall bottom");
        roomHeight = Math.Abs(topWall.transform.position.z - bottomWall.transform.position.z);
        roomWidth = rightWall.transform.position.x - leftWall.transform.position.x;
        verticalRoomCenter = topWall.transform.position.z - (roomHeight / 2);
    }


    private void EnableObstacle(PantoCollider collider)
    {
        collider.onLower = false;
        collider.onUpper = true;
        collider.CreateObstacle();
        collider.Enable();
        
    }

    // spawns a force field around the target position
    public void EnableForceField()
    {
        forceField.SetActive(true);
        forceField.transform.position = target.transform.position;

        // the forcefield needs to be double as big as the max dist to the wall
        float maxDistToWallX = Math.Max(
            Math.Abs(Vector3.Distance(target.transform.position, rightWall.transform.position)),
            Math.Abs(Vector3.Distance(target.transform.position, leftWall.transform.position))
        );
        float maxDistToWallY = Math.Max(
            Math.Abs(Vector3.Distance(target.transform.position, topWall.transform.position)),
            Math.Abs(Vector3.Distance(target.transform.position, bottomWall.transform.position))
        );

        forceField.transform.localScale = new Vector3(2*maxDistToWallX, 1, 2*maxDistToWallY);
    }

    public void DisableForceField()
    {
        forceField.SetActive(false);
        //CenterForceField ff = target.GetComponent<CenterForceField>();
        //if (ff != null)
        //{
        //}
    }

    async public void DisableAll()
    {
        if (target)
        {

            target.gameObject.GetComponent<PantoCircularCollider>().Remove();
            await Task.Delay(100);
        }
        if (rails.Length!=0)
        {

            for (int i = 0; i < rails.Length; i++)
            {
                Debug.Log("Disabling rail " + rails[i].GetInstanceID());
                rails[i].GetComponent<PantoBoxCollider>().Remove();
                rails[i].SetActive(false);
                await Task.Delay(100);
            }
        }
        DisableForceField();
        await Task.Delay(1000);
    }

    async public Task EnableWalls()
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        foreach(GameObject wall in walls)
        {
            EnableObstacle(wall.GetComponent<PantoCollider>());
            await Task.Delay(100);
        }
    }


    public void ReEnableTarget(Vector3 position, Vector3 scale)
    {
        GameObject newTarget = Instantiate(target);
        Destroy(target);
        target = newTarget;
        target.transform.position = position;
        target.transform.localScale = scale;
        Debug.Log("enabling target " + target.GetInstanceID());
        EnableObstacle(target.gameObject.GetComponent<PantoCollider>());

    }

    public void ReEnableRails(Vector3 targetPos, float width, string condition)
    {
        ArrayList newRails = new ArrayList();
        for (int i = 0; i < rails.Length; i++)
        {
            GameObject rail = Instantiate(rails[i]);
            rail.SetActive(true);
            int rotationAngle = i * 90;
            rail.transform.position = targetPos;
            if (condition == "through")
            {
                // if we have rails that go through the whole room
                if (rotationAngle == 0)
                {
                    //horizontal rail
                    rail.transform.localScale = new Vector3(roomWidth, 1, width);
                    // we have to horizontally center the rail to avoid out of
                    // bounds crashes on the firmware
                    Vector3 railPos = new Vector3(0, 0, targetPos.z);
                    rail.transform.position = railPos;
                } else
                {
                    //vertical rail
                    rail.transform.localScale = new Vector3(roomHeight, 1, width);
                    // we have to vertically center the rail to avoid out of
                    // bounds crashes on the firmware
                    Vector3 railPos = new Vector3(targetPos.x, 0, verticalRoomCenter);
                    rail.transform.position = railPos;
                }
            } else
            {

                rail.transform.localScale = new Vector3(int.Parse(condition), 1, width);
            }
            rail.transform.eulerAngles = new Vector3(0, rotationAngle, 0);
            newRails.Add(rail);
            rail.gameObject.AddComponent<PantoBoxCollider>();
            EnableObstacle(rail.gameObject.GetComponent<PantoBoxCollider>());
            Debug.Log("enabled rail " + rail.GetInstanceID());
        }
        for (int i = 0; i < rails.Length; i++)
        {
            Destroy(rails[i]);
        }
        rails = newRails.ToArray(typeof(GameObject)) as GameObject[];

    }

}
