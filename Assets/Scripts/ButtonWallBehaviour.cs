using UnityEngine;
public class ButtonWallBehaviour : MonoBehaviour
{
    
    void OnCollisionEnter(Collision col)
    {
        Debug.Log("Button wall collided");
        // When target is hit
        if (col.gameObject.name == "PlayerUpper" || col.gameObject.name == "PlayerLower")
        {
            GameObject studyWizard = GameObject.Find("Study Wizard");
            studyWizard.GetComponent<TaskSequence>().ButtonWallPressed(gameObject);

        }
    }
}
