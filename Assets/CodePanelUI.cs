using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections;

public class CodePanelUI : MonoBehaviour
{
    public event Action OnCodeCorrect;

    [Header("UI Elements")]
    public GameObject codePanel;
    public GameObject dialogWindow;
    public GameObject notePanelButton;
    public TextMeshProUGUI inputText;

    [Header("Note")]
    public Button noteButton;
    public CollectibleViewUI collectibleViewUI;
    public CollectibleTextItem noteItem;

    private bool wasCodePanelVisibleBeforeNote = false;

    [Header("Settings")]
    public string correctCode = "118";
    public float feedbackDisplayTime = 3f;

    private string currentInput = "";

    void Start()
    {
        DialogueManager.Instance.HideDialoguePanel();

        if (noteButton != null)
            noteButton.onClick.AddListener(ShowNote);
    }

    public void ShowCodePanel()
    {
        DialogueManager.Instance.HideDialoguePanel(() =>
        {
            currentInput = "";
            inputText.text = "";
            inputText.color = Color.white;
            codePanel.SetActive(true);
        });
    }

    public void OnDigitPress(string digit)
    {
        if (currentInput.Length >= 3) return;

        currentInput += digit;
        inputText.text = currentInput;
        inputText.color = Color.white;
    }

    public void OnClear()
    {
        currentInput = "";
        inputText.text = "";
    }

    public void OnSubmit()
    {
        bool correct = currentInput == correctCode;

        StartCoroutine(ShowFeedback(
            correct ? "CORRECT" : "ERROR",
            correct ? new Color(0.03f, 0.53f, 0.06f) : new Color(0.51f, 0.08f, 0.03f),
            correct
        ));
    }

    IEnumerator ShowFeedback(string message, Color color, bool isCorrect)
    {
        inputText.text = message;
        inputText.color = color;

        yield return new WaitForSeconds(feedbackDisplayTime);

        inputText.text = "";

        if (isCorrect)
        {
            codePanel.SetActive(false);
            DialogueManager.Instance.ShowDialoguePanel();
            OnCodeCorrect?.Invoke();
        }
        else
        {
            OnClear();
        }
    }

    private void ShowNote()
    {
        if (noteItem == null)
        {
            Debug.LogError("CodePanelUI: noteItem не назначен");
            return;
        }

        wasCodePanelVisibleBeforeNote = codePanel.activeSelf;
        codePanel.SetActive(false);

        collectibleViewUI.Show(noteItem);
    }

    public void CloseNote()
    {
        collectibleViewUI.Hide();

        if (wasCodePanelVisibleBeforeNote)
            codePanel.SetActive(true);
    }

    public void ToggleNotePanel()
    {
        if (collectibleViewUI.gameObject.activeSelf)
            CloseNote();
        else
            ShowNote();
    }

}