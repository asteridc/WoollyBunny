using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

public class CircuitPuzzleManager : MonoBehaviour
{
    public int maxAttempts = 3;
    private int currentAttempts;
    private int currentLevel;

    public List<CircuitLevelData> levels;
    private CircuitLevel currentCircuit;
    public Transform parent;

    public GameObject deathPanel;
    public GameObject circuitUIPanel;

    public static CircuitPuzzleManager Instance { get; private set; }

    private void Awake()
    {
        Debug.Log("CircuitPuzzleManager.Instance установлен");
        Instance = this;
        circuitUIPanel.SetActive(false);
    }

    public void StartPuzzle()
    {
        currentAttempts = maxAttempts;
        currentLevel = 0;
        circuitUIPanel.SetActive(true);
        deathPanel.SetActive(false);
        LoadLevel(currentLevel);
    }

    private void LoadLevel(int index)
    {
        ClearOldLevel();
        GameObject levelObj = Instantiate(levels[index].levelPrefab, parent);
        currentCircuit = levelObj.GetComponent<CircuitLevel>();
        CircuitLevelBuilding builder = levelObj.GetComponent<CircuitLevelBuilding>();

        if (currentCircuit.levelBuilder != null)
        {
            currentCircuit.levelBuilder.Init(OnLevelSuccess, OnLevelFail);
        }

        currentCircuit.levelBuilder = builder;
        currentCircuit.Init(OnLevelSuccess, OnLevelFail);
    }

    private void OnLevelSuccess()
    {
        currentLevel++;
        if (currentLevel >= levels.Count)
            EndPuzzle(true); // победа
        else
            LoadLevel(currentLevel);
    }

    private void OnLevelFail()
    {
        currentAttempts--;
        if (currentAttempts <= 0)
            StartCoroutine(ShowDeath());
        else
            LoadLevel(currentLevel); // рестарт уровн€
    }

    private IEnumerator ShowDeath()
    {
        // анимаци€ искрени€, задержка 5 секунд
        yield return new WaitForSeconds(5f);
        deathPanel.SetActive(true);
        // можно отключить управление и ждать клика
    }

    private void EndPuzzle(bool success)
    {
        circuitUIPanel.SetActive(false);
        // ¬озвращаемс€ в диалог
        // DialogueManager.Instance.ContinueDialogueAfterPuzzle(success);
    }

    private void ClearOldLevel()
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }
}