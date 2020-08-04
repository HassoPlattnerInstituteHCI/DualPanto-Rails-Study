using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class StudyObstacleManager : MonoBehaviour
{
    private GameObject target;
    GameObject[] rails;
    private float roomHeight;
    private float roomWidth;
    private float verticalRoomCenter;
    async void Start()
    {
        target = GameObject.Find("Target");
        rails = GameObject.FindGameObjectsWithTag("Rail");
        Debug.Log("Starting obstacle manager");
        await Task.Delay(1000);
        roomHeight = Math.Abs(GameObject.Find("Wall top").transform.position.z - GameObject.Find("Wall bottom").transform.position.z);
        roomWidth = GameObject.Find("Wall right").transform.position.x - GameObject.Find("Wall left").transform.position.x;
        verticalRoomCenter = GameObject.Find("Wall top").transform.position.z - roomHeight / 2;
        // if we register obstacles too early, the device will not work any longer (only sync debug logs will be printed
        // I am working on fixing this, but for now just add a wait
        //await Task.Delay(1000);
        //PantoCollider[] pantoColliders = GameObject.FindObjectsOfType<PantoCollider>();
        /*foreach (PantoCollider collider in pantoColliders)
        {
            EnableObstacle(collider);
        }*/
    }


    private void EnableObstacle(PantoCollider collider)
    {
        collider.onLower = false;
        collider.onUpper = true;
        collider.CreateObstacle();
        collider.Enable();
        
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


    public GameObject ReEnableTarget(Vector3 position, Vector3 scale)
    {
        GameObject newTarget = Instantiate(target);
        Destroy(target);
        target = newTarget;
        target.transform.position = position;
        target.transform.localScale = scale;
        Debug.Log("enabling target " + target.GetInstanceID());
        //target.gameObject.AddComponents<PantoCollider>());
        EnableObstacle(target.gameObject.GetComponent<PantoCollider>());
        Debug.Log("enabled target");
        return target;
    }

    public void ReEnableRails(Vector3 targetPos, float width, int length)
    {
        ArrayList newRails = new ArrayList();
        for (int i = 0; i < rails.Length; i++)
        {
            GameObject rail = Instantiate(rails[i]);
            rail.SetActive(true);
            int rotationAngle = i * 90;
            // TODO: calculate max length of the rails (where they collide with outer walls of the playing area)
            rail.transform.position = targetPos;
            if (length > roomHeight)
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

                rail.transform.localScale = new Vector3(length, 1, width);
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
