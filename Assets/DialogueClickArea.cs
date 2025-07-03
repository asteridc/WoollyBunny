using UnityEngine;
using UnityEngine.UI;

public class DialogueClickArea : MonoBehaviour
{
    public DialogueManager dialogueManager; // присваиваю в инспекторе

    void Start()
    {
        Button btn = GetComponent<Button>();
        Debug.Log("DCA.CS работает");
        if (btn != null && dialogueManager != null)
        {
            btn.onClick.RemoveAllListeners(); // на всякий случай убираем старые
            btn.onClick.AddListener(() => dialogueManager.OnClickNext());
        }
    }
}