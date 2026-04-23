using UnityEngine;

[System.Serializable]
public class Choice
{
    public string choiceText;
    public int nextLineIndex;

    public HeroPath pathToAdd;
    public int points;

    public bool isSideChoice; // если это побочный выбор
}
