using UnityEngine;
using TMPro;

public class UniqueMaterialHandler : MonoBehaviour
{
    private TMP_Text textComponent;

    void Start()
    {
        // Получаем TextMeshPro компонент
        textComponent = GetComponent<TMP_Text>();

        if (textComponent != null)
        {
            // Создаем копию материала
            Material uniqueMaterial = new Material(textComponent.fontSharedMaterial);
            textComponent.fontSharedMaterial = uniqueMaterial;

            // Пример изменения только для этого объекта
            uniqueMaterial.SetColor("_FaceColor", Color.white);
        }
    }
}