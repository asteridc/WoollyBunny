using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LampSequenceController : MonoBehaviour
{
    [Header("Room Images")]
    [SerializeField] private GameObject normalRoom;
    [SerializeField] private GameObject lampOffRoom;

    [Header("Lamp Interaction")]
    [SerializeField] private Button lampButton;

    [Header("Manual Interactions")]
    [SerializeField] private int requiredClicks = 5;

    [Header("Flicker")]
    [SerializeField] private int flickerCycles = 9;
    [SerializeField] private float firstCycleDuration = 0.45f;
    [SerializeField] private float speedMultiplier = 0.72f;

    [Header("Audio")]
    [SerializeField] private AudioSource addictScreamSource;
    [SerializeField] private AudioSource childScreamSource;
    [SerializeField] private AudioSource squishAudioSource;
    [SerializeField] private AudioSource switchAudioSource;

    [Header("Final Event Timing")]
    [SerializeField] private float finalEventDelay = 0.5f;
    [SerializeField] private float childScreamDelay = 1f;

    [SerializeField] private float zombieFadeDelay = 0.8f;
    [SerializeField] private float zombieFadeDuration = 1.2f;

    [SerializeField] private float squishDelay = 1.0f;
    [SerializeField] private float squishFadeDelay = 0.8f;
    [SerializeField] private float squishFadeDuration = 1.0f;
    [SerializeField] private float bloodDelayAfterSquish = 0.2f;

    [Header("Blood")]
    [SerializeField] private float bloodDelayAfterSquishFade = 0.3f;
    [SerializeField] private GameObject bloodOverlay;
    [SerializeField] private float bloodFadeDuration = 0.6f;

    private bool isLampOn = true;
    private bool isFlickering;
    private int interactionCount;

    private Coroutine flickerCoroutine;

    public void OnLampClicked()
    {
        if (switchAudioSource != null)
        {
            switchAudioSource.PlayOneShot(switchAudioSource.clip);
        }
        
        if (isFlickering)
            return;

        interactionCount++;

        if (interactionCount < requiredClicks)
        {
            SetLampState(!isLampOn);
            return;
        }

        StartFlicker();
    }

    private void StartFlicker()
    {
        if (isFlickering)
            return;

        isFlickering = true;

        if (flickerCoroutine != null)
            StopCoroutine(flickerCoroutine);

        flickerCoroutine = StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        float duration = firstCycleDuration;

        for (int i = 0; i < flickerCycles; i++)
        {
            SetLampState(!isLampOn);

            yield return new WaitForSecondsRealtime(duration);

            duration *= speedMultiplier;
        }

        // Финальное состояние.
        SetLampState(false);

        yield return StartCoroutine(FinalEventRoutine());

        flickerCoroutine = null;
    }

    private IEnumerator FinalEventRoutine()
    {
        // Лампа окончательно погасла.
        yield return new WaitForSecondsRealtime(finalEventDelay);

        // Крик зомби.
        if (addictScreamSource != null)
        {
            addictScreamSource.Play();
        }

        // Пауза перед ребёнком.
        yield return new WaitForSecondsRealtime(childScreamDelay);

        // Крик ребёнка.
        if (childScreamSource != null)
        {
            childScreamSource.Play();
        }

        // Через некоторое время начинаем затихание зомби.
        yield return new WaitForSecondsRealtime(zombieFadeDelay);

        if (addictScreamSource != null)
        {
            StartCoroutine(
                FadeOutAudio(
                    addictScreamSource,
                    zombieFadeDuration));
        }

        // Задержка перед Slimy Squish Stabs.
        yield return new WaitForSecondsRealtime(squishDelay);

        // Проигрываем Slimy целиком,
        // затухание происходит только в конце файла.
        yield return StartCoroutine(
            PlayAudioWithEndFade(
                squishAudioSource,
                squishFadeDuration));

        // После окончания Slimy появляется кровь.
        yield return new WaitForSecondsRealtime(
            bloodDelayAfterSquish);

        yield return StartCoroutine(ShowBlood());
    }

    private IEnumerator FadeOutAudio(AudioSource source, float duration)
    {
        if (source == null)
            yield break;

        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            source.volume = Mathf.Lerp(startVolume, 0f, t);

            yield return null;
        }

        source.volume = 0f;
        source.Stop();
        source.volume = startVolume;
    }

    private IEnumerator PlayAudioWithEndFade(AudioSource source, float fadeDuration)
    {
        if (source == null || source.clip == null)
            yield break;

        source.volume = 1f;
        source.Play();

        float clipLength = source.clip.length;

        // Оставляем время на нормальное проигрывание файла.
        float waitTime = Mathf.Max(0f, clipLength - fadeDuration);

        yield return new WaitForSecondsRealtime(waitTime);

        // Плавно затухаем только в конце.
        yield return StartCoroutine(
            FadeOutAudio(source, fadeDuration));
    }

    private IEnumerator ShowBlood()
    {
        if (bloodOverlay == null)
            yield break;

        bloodOverlay.SetActive(true);

        Image image = bloodOverlay.GetComponent<Image>();

        if (image == null)
            yield break;

        Color color = image.color;
        color.a = 0f;
        image.color = color;

        float elapsed = 0f;

        while (elapsed < bloodFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / bloodFadeDuration);

            color.a = t;
            image.color = color;

            yield return null;
        }

        color.a = 1f;
        image.color = color;
    }

    private void SetLampState(bool lampOn)
    {
        isLampOn = lampOn;

        if (normalRoom != null)
            normalRoom.SetActive(lampOn);

        if (lampOffRoom != null)
            lampOffRoom.SetActive(!lampOn);
    }
}