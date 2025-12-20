using UnityEngine;

public class CombatTutorialManager : MonoBehaviour
{
    public bool tutorialShown = false;

    public void ShowTutorialIfNeeded()
    {
        if (!tutorialShown)
        {
            // show tutorial UI
            tutorialShown = true;
            Debug.Log("Show combat tutorial");
        }
    }
}
