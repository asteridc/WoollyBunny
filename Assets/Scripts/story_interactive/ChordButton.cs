using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class ChordButton : MonoBehaviour
{
    [Header("Аудио")]
    public AudioClip chordClip;       // короткий звук аккорда
    public AudioSource audioSource;   // отдельный источник на каждой кнопке
    [HideInInspector] public bool isActive;
    [HideInInspector] public bool wasPlayed;

    [Header("Визуализация")]
    public Image highlight;

    private Button btn;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        btn = GetComponent<Button>();
        canvasGroup = GetComponent<CanvasGroup>();

        btn.onClick.AddListener(PlayChord);

        if (highlight != null)
            highlight.enabled = false;

        // обязательно включаем AudioSource
        if (audioSource != null)
            audioSource.playOnAwake = false;
    }

    public void PlayChord()
    {
        if (!isActive || wasPlayed) return;

        wasPlayed = true;

        // 🔊 проигрываем аккорд
        if (audioSource != null && chordClip != null)
        {
            audioSource.clip = chordClip;
            audioSource.Play();
        }

        // 💡 визуальный отклик
        if (highlight != null)
            StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        highlight.enabled = true;
        for (float i = 1f; i >= 0; i -= Time.deltaTime * 3f)
        {
            highlight.color = new Color(1f, 1f, 1f, i);
            yield return null;
        }
        highlight.enabled = false;
    }
}