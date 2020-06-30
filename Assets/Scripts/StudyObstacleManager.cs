using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class StudyObstacleManager : MonoBehaviour
{
    private GameObject target;
    GameObject[] rails;
    async void Start()
    {
        target = GameObject.Find("Target");
        rails = GameObject.FindGameObjectsWithTag("Rail");
        // if we register obstacles too early, the device will not work any longer (only sync debug logs will be printed
        // I am working on fixing this, but for now just add a wait
        await Task.Delay(1000);
        //PantoCollider[] pantoColliders = GameObject.FindObjectsOfType<PantoCollider>();
        /*foreach (PantoCollider collider in pantoColliders)
        {
            EnableObstacle(collider);
        }*/
    }

    private void EnableObstacle(PantoCollider collider)
    {
        collider.CreateObstacle();
        collider.Enable();
    }


    public void DisableAll()
    {
        if (target)
        {

            target.gameObject.GetComponent<PantoCircularCollider>().Disable();
        }
        if (rails.Length!=0)
        {

            for (int i = 0; i < rails.Length; i++)
            {
                //Debug.Log("Disabling rails");
                rails[i].GetComponent<PantoBoxCollider>().Disable();
                rails[i].SetActive(false);
            }
        }
    }

    public GameObject ReEnableTarget()
    {
        GameObject newTarget = Instantiate(target);
        Destroy(target);
        target = newTarget;
        EnableObstacle(target.gameObject.GetComponent<PantoCollider>());
        return target;
    }

    public void ReEnableRails(Vector3 targetPos, float width, int angle)
    {
        Debug.Log(rails.Length);
        ArrayList newRails = new ArrayList();
        for (int i = 0; i < rails.Length; i++)
        {
            GameObject rail = Instantiate(rails[i]);
            rail.SetActive(true);
            int rotationAngle = angle + i * 90;
            rail.transform.localScale = new Vector3(15, 1, width);
            rail.transform.eulerAngles = new Vector3(0, rotationAngle, 0);
            rail.transform.position = targetPos;
            newRails.Add(rail);
            EnableObstacle(rail.gameObject.GetComponent<PantoCollider>());
            Debug.Log("enabled rail");
        }
        for (int i = 0; i < rails.Length; i++)
        {
            Destroy(rails[i]);
        }
        rails = newRails.ToArray(typeof(GameObject)) as GameObject[];

    }
}
