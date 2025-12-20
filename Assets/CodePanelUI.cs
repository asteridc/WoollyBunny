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

    [Header("Note Access")]
    public Button noteButton;
    public GameObject collectibleViewUI;
    public TextMeshProUGUI collectibleTitle;
    public TextMeshProUGUI collectibleContent;
    public Image collectibleIcon;

    private bool wasCodePanelVisibleBeforeNote = false;

    [Header("Note Content")]
    public string noteTitle;
    [TextArea(3, 10)] public string noteText;
    public Sprite noteSprite;

    [Header("Settings")]
    public string correctCode = "118";
    public float feedbackDisplayTime = 3f;

    private string currentInput = "";

    void Start()
    {
        DialogueManager.Instance.HideDialoguePanel();

        if (noteButton != null)
        {
            noteButton.onClick.AddListener(ShowNote);
        }
    }

    public void ShowCodePanel()
    {
        DialogueManager.Instance.HideDialoguePanel(() =>
        {
            Debug.Log("🔓 CodePanelUI: Открываем панель");
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
        if (currentInput == correctCode)
        {
            StartCoroutine(ShowFeedback("CORRECT", new Color(0.0323f, 0.5283f, 0.0653f), true));
        }
        else
        {
            StartCoroutine(ShowFeedback("ERROR", new Color(0.5137f, 0.0852f, 0.0352f), false));
        }
    }

    IEnumerator ShowFeedback(string message, Color color, bool isCorrect)
    {
        Debug.Log("💬 CodePanelUI: Введённый код — " + currentInput);
        inputText.text = message;
        inputText.color = color;

        yield return new WaitForSeconds(feedbackDisplayTime);

        inputText.text = "";

        if (isCorrect)
        {
            Debug.Log("✅ CodePanelUI: Код верен, закрываем панель и продолжаем");
            codePanel.SetActive(false);
            DialogueManager.Instance.ShowDialoguePanel();
            OnCodeCorrect?.Invoke();
        }
        else
        {
            Debug.Log("❌ CodePanelUI: Код неверен");
            OnClear();
        }
    }

    public void ToggleNotePanel()
    {
        if (notePanelButton != null)
        {
            notePanelButton.SetActive(!notePanelButton.activeSelf);
        }
    }

    private void ShowNote()
    {
        wasCodePanelVisibleBeforeNote = codePanel.activeSelf;

        codePanel.SetActive(false);

        collectibleViewUI.SetActive(true);
        collectibleTitle.text = noteTitle;
        collectibleContent.text = noteText;
        collectibleIcon.sprite = noteSprite;
    }

    public void CloseNote()
    {    
        collectibleViewUI.SetActive(false);
        if (wasCodePanelVisibleBeforeNote)
        {
            codePanel.SetActive(true);
        }
    }
}