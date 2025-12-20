using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class GuitarSceneManager : MonoBehaviour
{
    [Header("Основное")]
    public Image backgroundImage;
    public Sprite apartmentBackground;
    public Sprite guitarBackground;
    public Image faderImage; // черный Image поверх всего экрана

    [Header("Аккорды")]
    public List<ChordButton> chordButtons;

    [Header("Пропуск сцены")]
    public Image skipProgressCircle;
    public float holdTimeToSkip = 2f;

    private float holdTimer;
    private bool isHolding;
    private bool miniGameFinished;
    private bool sceneActive;

    void Start()
    {
        faderImage.gameObject.SetActive(true);
        faderImage.color = new Color(0, 0, 0, 0); // полностью прозрачный

        foreach (var btn in chordButtons)
            btn.gameObject.SetActive(false);

        skipProgressCircle.fillAmount = 0;
        skipProgressCircle.gameObject.SetActive(false);
    }

    public void StartGuitarScene()
    {
        if (sceneActive) return;
        sceneActive = true;
        StartCoroutine(SceneSequence());
    }

    private IEnumerator SceneSequence()
    {
        yield return FadeOut(); // затемнение
        backgroundImage.sprite = guitarBackground;
        yield return FadeIn(); // появление фона

        // Запускаем мини-интерактив
        yield return StartCoroutine(GuitarMiniGame());
    }

    private IEnumerator GuitarMiniGame()
    {
        // Показываем кнопки с анимацией
        foreach (var btn in chordButtons)
        {
            btn.gameObject.SetActive(true);
            btn.transform.localScale = Vector3.zero;
            btn.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack); // плавное появление

            btn.isActive = true;
            btn.wasPlayed = false;
        }

        // Ждем, пока все кнопки будут сыграны
        foreach (var btn in chordButtons)
        {
            yield return new WaitUntil(() => btn.wasPlayed);
        }

        // Анимируем скрытие кнопок
        foreach (var btn in chordButtons)
        {
            btn.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack)
                .OnComplete(() => btn.gameObject.SetActive(false));
        }

        miniGameFinished = true;
    }

    void Update()
    {
        if (!miniGameFinished) return;

        // Удержание ЛКМ для пропуска
        if (Input.GetMouseButton(0))
        {
            isHolding = true;
            holdTimer += Time.deltaTime;
            skipProgressCircle.fillAmount = holdTimer / holdTimeToSkip;
            skipProgressCircle.gameObject.SetActive(true);

            if (holdTimer >= holdTimeToSkip)
            {
                StartCoroutine(EndScene());
            }
        }
        else if (isHolding)
        {
            isHolding = false;
            holdTimer = 0;
            skipProgressCircle.fillAmount = 0;
            skipProgressCircle.gameObject.SetActive(false);
        }
    }

    private IEnumerator EndScene()
    {
        miniGameFinished = false;

        yield return FadeOut();
        backgroundImage.sprite = apartmentBackground;
        yield return FadeIn();

        sceneActive = false;
        Debug.Log("Гитарная сцена завершена. Возврат к сюжету.");
    }

    // ===== Fade через Image + DOTween =====
    private Tween FadeOut()
    {
        return faderImage.DOFade(1f, 0.5f).SetEase(Ease.InOutSine);
    }

    private Tween FadeIn()
    {
        return faderImage.DOFade(0f, 0.5f).SetEase(Ease.InOutSine);
    }
}