using UnityEngine;
public class TargetBehaviour : MonoBehaviour
{
    
    void OnCollisionEnter(Collision col)
    {
        // When target is hit
        if (col.gameObject.name == "PlayerUpper" || col.gameObject.name == "PlayerLower")
        {
            GameObject studyWizard = GameObject.Find("Study Wizard");
            studyWizard.GetComponent<TaskSequence>().StopTask();

        }
    }
}
