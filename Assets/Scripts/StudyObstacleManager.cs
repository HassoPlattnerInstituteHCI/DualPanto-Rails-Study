using System.Collections;
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
        Debug.Log("Starting obstacle manager");
        await Task.Delay(1000);
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
        collider.onUpper = false;
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
            int rotationAngle = 45 + i * 90;
            rail.transform.localScale = new Vector3(length, 1, width);
            rail.transform.eulerAngles = new Vector3(0, rotationAngle, 0);
            rail.transform.position = targetPos;
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
