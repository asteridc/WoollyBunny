using UnityEngine;

public class nextdialogue : MonoBehaviour
{
    public DialogueManager dialogueManager;

    public void OnClick()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnClickNext();
            Debug.Log("Кнопка нажата");
        }
    }
}