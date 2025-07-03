using UnityEngine;
using UnityEngine.UI;

public class LanguageManager : MonoBehaviour
{
    public void ChangeLanguage(int languageIndex)
    {
        switch (languageIndex)
        {
            case 0: // –усский
                // ѕример: установить €зык через локализацию
                Debug.Log("язык сменен на –усский");
                break;
            case 1: // јнглийский
                Debug.Log("язык сменен на јнглийский");
                break;
            default:
                Debug.Log("язык не поддерживаетс€");
                break;
        }
    }
}