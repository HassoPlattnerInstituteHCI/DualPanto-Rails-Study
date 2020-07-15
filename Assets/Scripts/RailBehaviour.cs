using UnityEngine;
public class RailBehaviour : MonoBehaviour
{
    
    void OnCollisionEnter(Collision col)
    {
        // When rail is hit log time to rail
        if (col.gameObject.name == "PlayerUpper" || col.gameObject.name == "PlayerLower")
        {
            GameObject studyWizard = GameObject.Find("Study Wizard");
            studyWizard.GetComponent<TaskSequence>().LogTimeToRail();

        }
    }
}
