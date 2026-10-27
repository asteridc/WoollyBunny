using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIRaycastDebug : MonoBehaviour
{
    private readonly List<RaycastResult> results =
        new List<RaycastResult>();

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Debug.Log("[UI Debug] ЛКМ: " + Input.mousePosition);

        if (EventSystem.current == null)
        {
            Debug.LogError("[UI Debug] EventSystem не найден!");
            return;
        }

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position = Input.mousePosition;

        results.Clear();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count == 0)
        {
            Debug.LogWarning(
                "[UI Debug] Ни один UI-объект не получил Raycast.");
            return;
        }

        foreach (RaycastResult result in results)
        {
            Debug.Log(
                "[UI Debug] Объект под курсором: " +
                result.gameObject.name,
                result.gameObject);
        }
    }
}