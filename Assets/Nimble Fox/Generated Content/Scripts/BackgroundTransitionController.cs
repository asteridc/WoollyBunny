using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundTransitionController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("CanvasGroup used for darkening the entire scene (fadepanel_ch1)")]
    public CanvasGroup fadePanel;
    [Tooltip("DialogueManager to detect and change backgrounds")]
    public DialogueManager dialogueManager; // holds public Image backgroundImage
    [Tooltip("DialogueClickArea to enable clicks after transition")]
    public DialogueClickArea dialogueClickArea; // has a Button component

    [Header("Animation Settings")]
    [Tooltip("Duration of the darkening animation")]
    public float darkenDuration = 0.5f;
    [Tooltip("Duration of the lightening animation")]
    public float lightenDuration = 0.5f;
    [Tooltip("Easing curve for darkening (t from 0→1)")]
    public AnimationCurve darkenEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Easing curve for lightening (t from 0→1)")]
    public AnimationCurve lightenEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Tracks the last applied background to detect changes
    private Sprite previousBackground;

    void Start()
    {
        // Auto-assign references if not set in Inspector
        if (fadePanel == null)
        {
            var fpGO = GameObject.Find("fadepanel_ch1");
            if (fpGO != null)
                fadePanel = fpGO.GetComponent<CanvasGroup>();
            else
                Debug.LogError("BackgroundTransitionController: fadepanel_ch1 not found in scene.");
        }
        if (dialogueManager == null)
            dialogueManager = GetComponent<DialogueManager>();
        if (dialogueClickArea == null)
            dialogueClickArea = GetComponent<DialogueClickArea>();

        Debug.Log($"BackgroundTransitionController: Start - fadePanel={(fadePanel != null)} dialogueManager={(dialogueManager != null)} dialogueClickArea={(dialogueClickArea != null)}");

        if (fadePanel == null || dialogueManager == null || dialogueClickArea == null)
        {
            Debug.LogError("BackgroundTransitionController: One or more references are missing after auto-assign.");
        }

        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
            fadePanel.interactable = false;
            Debug.Log($"BackgroundTransitionController: fadePanel initial alpha={fadePanel.alpha}, blocksRaycasts={fadePanel.blocksRaycasts}, interactable={fadePanel.interactable}");
        }

        // Initialize previousBackground to the current sprite
        if (dialogueManager != null && dialogueManager.backgroundImage != null)
        {
            previousBackground = dialogueManager.backgroundImage.sprite;
            Debug.Log($"BackgroundTransitionController: initial previousBackground={(previousBackground != null ? previousBackground.name : "null")}");
        }

        // Start polling for background changes
        StartCoroutine(BackgroundChangeDetection());
    }

    private IEnumerator BackgroundChangeDetection()
    {
        // Continuously check for sprite changes
        while (true)
        {
            if (dialogueManager != null && dialogueManager.backgroundImage != null)
            {
                var current = dialogueManager.backgroundImage.sprite;
                if (current != previousBackground)
                {
                    Debug.Log($"BackgroundTransitionController: Detected background change from {(previousBackground != null ? previousBackground.name : "null")} to {(current != null ? current.name : "null")}");
                    previousBackground = current;
                    // Trigger transition with the new background sprite
                    yield return StartCoroutine(TransitionCoroutine(current));
                }
            }
            yield return null;
        }
    }

    private IEnumerator TransitionCoroutine(Sprite newBackground)
    {
        Debug.Log($"BackgroundTransitionController: TransitionCoroutine start for newBackground={(newBackground != null ? newBackground.name : "null")}");

        // Block overlay input
        fadePanel.blocksRaycasts = true;
        fadePanel.interactable = true;
        // Disable click during transition
        Button clickButton = dialogueClickArea.GetComponent<Button>();
        if (clickButton != null)
            clickButton.interactable = false;

        // 1. Darken scene (alpha 0 → 1)
        Debug.Log("BackgroundTransitionController: Starting darken");
        yield return StartCoroutine(FadeCanvasGroup(fadePanel, 0f, 1f, darkenDuration, darkenEasing));
        Debug.Log($"BackgroundTransitionController: Darken complete, alpha={fadePanel.alpha}");

        // 2. Swap background sprite
        if (dialogueManager != null && dialogueManager.backgroundImage != null)
        {
            dialogueManager.backgroundImage.sprite = newBackground;
            Debug.Log($"BackgroundTransitionController: Swapped background sprite to {(newBackground != null ? newBackground.name : "null")}");
        }
        else
        {
            Debug.LogWarning("BackgroundTransitionController: dialogueManager or backgroundImage is null during sprite swap.");
        }

        // 3. Lighten scene (alpha 1 → 0)
        Debug.Log("BackgroundTransitionController: Starting lighten");
        yield return StartCoroutine(FadeCanvasGroup(fadePanel, 1f, 0f, lightenDuration, lightenEasing));
        Debug.Log($"BackgroundTransitionController: Lighten complete, alpha={fadePanel.alpha}");

        // 4. Unblock overlay input and re-enable clicks
        fadePanel.blocksRaycasts = false;
        fadePanel.interactable = false;
        if (clickButton != null)
            clickButton.interactable = true;

        Debug.Log("BackgroundTransitionController: TransitionCoroutine finished");
    }

    // Helper coroutine: fades a CanvasGroup from 'from' to 'to' over 'duration' using 'curve'
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration, AnimationCurve curve)
    {
        Debug.Log($"BackgroundTransitionController: FadeCanvasGroup called from {from} to {to} over {duration}s");
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = curve.Evaluate(t);
            cg.alpha = Mathf.Lerp(from, to, eased);
            yield return null;
        }
        cg.alpha = to;
        Debug.Log($"BackgroundTransitionController: FadeCanvasGroup complete, final alpha={cg.alpha}");
    }

    /// <summary>
    /// Public trigger to start a transition to a specified background sprite.
    /// newBg: Sprite to switch to during the transition.
    /// </summary>
    public void TriggerTransition(Sprite newBg)
    {
        Debug.Log($"BackgroundTransitionController: TriggerTransition called with newBg={(newBg != null ? newBg.name : "null")}");
        previousBackground = newBg;
        StartCoroutine(TransitionCoroutine(newBg));
    }
}