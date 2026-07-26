using DG.Tweening;
using UnityEngine;


public class UICursor : MonoBehaviour
{
    [SerializeField] private RectTransform cursor;
    [SerializeField] private Vector2 offset;

    void Update()
    {
        cursor.position = (Vector2)Input.mousePosition + offset;

        if (Input.GetMouseButtonDown(0))
        {
            cursor.DOKill();

            cursor.DOScale(0.65f, 0.05f)
                .OnComplete(() =>
                    cursor.DOScale(0.75f, 0.08f));
        }

    }


    public void HoverEnter()
    {
        cursor.DOScale(1.15f, 0.15f);
    }

    public void HoverExit()
    {
        cursor.DOScale(1f, 0.15f);
    }
}