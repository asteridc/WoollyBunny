using UnityEngine;

public class CodePanelController : MonoBehaviour
{
    public CodePanelUI codePanelUI;
    public DialogueManager dialogueManager;
    public int triggerLineIndex = 5;

    private bool codePanelActive = false;

    public void OnLineChanged(int currentLine)
    {
        Debug.Log("🔵 CodePanelController: текущая строка = " + currentLine);

        if (codePanelActive)
        {
            Debug.Log("🛑 Панель уже активна, ничего не делаем");
            return;
        }

        if (currentLine == triggerLineIndex)
        {
            Debug.Log("Показываем кодовую панель");

            if (codePanelUI == null)
            {
                Debug.LogError("codePanelUI == null! Панель не может быть показана.");
                return;
            }

            codePanelActive = true;
            codePanelUI.ShowCodePanel();
            codePanelUI.OnCodeCorrect += HandleCodeCorrect;
        }
    }

    private void HandleCodeCorrect()
    {
        codePanelActive = false;
        codePanelUI.OnCodeCorrect -= HandleCodeCorrect;
        dialogueManager.ShowNextLine();
    }

    public bool IsCodePanelActive()
    {
        return codePanelActive;
    }
}