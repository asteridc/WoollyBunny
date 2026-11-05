using System;
using System.Collections.Generic;
using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EliDiaryController : MonoBehaviour
{
    [Serializable]
    private class Chapter
    {
        public string titleRu;
        public string titleEn;
        public GameObject contentRoot;
        public EliDiaryNoteSlot[] notes;
    }

    [SerializeField] private Chapter[] chapters;
    [SerializeField] private TMP_Text chapterNumberText;
    [SerializeField] private TMP_Text chapterTitleText;
    [SerializeField] private TMP_Text noteContentTitleText;
    [SerializeField] private TMP_Text noteContentText;
    [SerializeField] private Image noteContentBackgroundImage;
    [SerializeField] private float chapterFadeDuration = 0.2f;

    private int currentChapterIndex;
    private Tween chapterTween;
    private EliDiaryNoteSlot selectedNote;

    private void Awake()
    {
        if (chapters == null)
            return;

        for (int i = 0; i < chapters.Length; i++)
        {
            Chapter chapter = chapters[i];
            if (chapter == null)
                continue;

            CacheChapterNotes(chapter);
            foreach (EliDiaryNoteSlot note in chapter.notes)
            {
                if (note == null)
                    continue;

                note.Selected -= OnNoteSelected;
                note.Selected += OnNoteSelected;
            }
        }

        for (int i = 1; i < chapters.Length; i++)
        {
            if (chapters[i] != null && chapters[i].contentRoot != null)
                chapters[i].contentRoot.SetActive(false);
        }

        ShowChapter(0, false);
        ClearCurrentSelection();
    }

    private void OnEnable()
    {
        EliDiaryNoteSlot.NoteUnlocked -= OnNoteUnlocked;
        EliDiaryNoteSlot.NoteUnlocked += OnNoteUnlocked;
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
        RefreshAllNotes();
        UpdateChapterLabels();
    }

    private void OnDisable()
    {
        EliDiaryNoteSlot.NoteUnlocked -= OnNoteUnlocked;
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    public void NextChapter()
    {
        ShowChapter(currentChapterIndex + 1, true);
    }

    public void PreviousChapter()
    {
        ShowChapter(currentChapterIndex - 1, true);
    }

    public void SelectChapter(int index)
    {
        ShowChapter(index, true);
    }

    private void ShowChapter(int index, bool animate)
    {
        if (chapters == null || chapters.Length == 0)
            return;

        index = Mathf.Clamp(index, 0, chapters.Length - 1);
        if (chapters[index] == null)
            return;

        if (index == currentChapterIndex && chapters[index].contentRoot != null &&
            chapters[index].contentRoot.activeSelf)
        {
            UpdateChapterLabels();
            return;
        }

        chapterTween?.Kill();
        GameObject previousRoot = chapters[currentChapterIndex] != null
            ? chapters[currentChapterIndex].contentRoot
            : null;
        GameObject nextRoot = chapters[index].contentRoot;

        Action showNext = () =>
        {
            if (previousRoot != null && previousRoot != nextRoot)
                previousRoot.SetActive(false);

            currentChapterIndex = index;
            if (nextRoot != null)
            {
                nextRoot.SetActive(true);
                CanvasGroup nextGroup = GetOrAddCanvasGroup(nextRoot);
                nextGroup.alpha = 0f;
                chapterTween = nextGroup.DOFade(1f, animate ? chapterFadeDuration : 0f)
                    .SetUpdate(true);
            }

            UpdateChapterLabels();
            RefreshCurrentChapterNotes();
            ClearCurrentSelection();
        };

        if (animate && previousRoot != null && previousRoot.activeSelf && previousRoot != nextRoot)
        {
            CanvasGroup previousGroup = GetOrAddCanvasGroup(previousRoot);
            chapterTween = previousGroup.DOFade(0f, chapterFadeDuration)
                .SetUpdate(true)
                .OnComplete(() => showNext());
        }
        else
        {
            showNext();
        }
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        return group != null ? group : target.AddComponent<CanvasGroup>();
    }

    private void UpdateChapterLabels()
    {
        if (chapters == null || chapters.Length == 0 || currentChapterIndex >= chapters.Length)
            return;

        Chapter chapter = chapters[currentChapterIndex];
        if (chapterNumberText != null)
        {
            chapterNumberText.text = LanguageManager.CurrentLanguage == Language.English
                ? $"Chapter {currentChapterIndex}"
                : $"Глава {currentChapterIndex}";
        }

        if (chapterTitleText != null && chapter != null)
        {
            chapterTitleText.text = LanguageManager.CurrentLanguage == Language.English
                ? chapter.titleEn
                : chapter.titleRu;
        }
    }

    private void CacheChapterNotes(Chapter chapter)
    {
        List<EliDiaryNoteSlot> notes = new List<EliDiaryNoteSlot>();
        if (chapter.notes != null)
        {
            foreach (EliDiaryNoteSlot note in chapter.notes)
            {
                if (note != null && !notes.Contains(note))
                    notes.Add(note);
            }
        }

        if (chapter.contentRoot != null)
        {
            EliDiaryNoteSlot[] childNotes =
                chapter.contentRoot.GetComponentsInChildren<EliDiaryNoteSlot>(true);
            foreach (EliDiaryNoteSlot note in childNotes)
            {
                if (note != null && !notes.Contains(note))
                    notes.Add(note);
            }
        }

        chapter.notes = notes.ToArray();
    }

    private void RefreshAllNotes()
    {
        if (chapters == null)
            return;

        foreach (Chapter chapter in chapters)
        {
            if (chapter == null || chapter.notes == null)
                continue;

            foreach (EliDiaryNoteSlot note in chapter.notes)
            {
                if (note != null)
                    note.RefreshAvailability();
            }
        }
    }

    private void RefreshCurrentChapterNotes()
    {
        if (chapters == null || currentChapterIndex >= chapters.Length)
            return;

        Chapter chapter = chapters[currentChapterIndex];
        if (chapter == null || chapter.notes == null)
            return;

        foreach (EliDiaryNoteSlot note in chapter.notes)
        {
            if (note != null)
                note.RefreshAvailability();
        }
    }

    private void OnNoteUnlocked(string noteId)
    {
        RefreshAllNotes();
    }

    private void OnLanguageChanged(Language language)
    {
        UpdateChapterLabels();
        RefreshCurrentChapterNotes();
        UpdateSelectedNoteContent();
    }

    private void OnNoteSelected(EliDiaryNoteSlot selected)
    {
        if (chapters == null || currentChapterIndex >= chapters.Length || chapters[currentChapterIndex] == null)
            return;

        EliDiaryNoteSlot[] notes = chapters[currentChapterIndex].notes;
        if (notes == null)
            return;

        foreach (EliDiaryNoteSlot note in notes)
        {
            if (note != null)
                note.SetSelected(note == selected);
        }

        selectedNote = selected;
        UpdateSelectedNoteContent();
    }

    private void ClearCurrentSelection()
    {
        selectedNote = null;
        if (chapters != null)
        {
            foreach (Chapter chapter in chapters)
            {
                if (chapter == null || chapter.notes == null)
                    continue;

                foreach (EliDiaryNoteSlot note in chapter.notes)
                {
                    if (note != null)
                        note.SetSelected(false, false);
                }
            }
        }

        if (noteContentTitleText != null)
            noteContentTitleText.text = string.Empty;
        if (noteContentText != null)
            noteContentText.text = string.Empty;
        if (noteContentBackgroundImage != null)
        {
            noteContentBackgroundImage.sprite = null;
            noteContentBackgroundImage.enabled = false;
        }
    }

    private void UpdateSelectedNoteContent()
    {
        if (selectedNote == null)
            return;

        if (noteContentTitleText != null)
            noteContentTitleText.text = selectedNote.Title;
        if (noteContentText != null)
            noteContentText.text = selectedNote.Content;
        if (noteContentBackgroundImage != null)
        {
            noteContentBackgroundImage.sprite = selectedNote.ContentBackground;
            noteContentBackgroundImage.enabled = selectedNote.ContentBackground != null;
        }
    }
}
