using UnityEngine;
public class ButtonWallBehaviour : MonoBehaviour
{
    
    void OnCollisionEnter(Collision col)
    {
        // When target is hit
        if (col.gameObject.name == "PlayerUpper")
        {
            Debug.Log("Button wall collided");
            GameObject studyWizard = GameObject.Find("Study Wizard");
            //studyWizard.GetComponent<TaskSequence>().ButtonWallPressed(gameObject);

        }
    }
}
