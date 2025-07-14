using UnityEngine;
using System;
using static CircuitLevel;

public class CircuitLevelBuilding : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject emptyPrefab;
    public GameObject startPrefab;
    public GameObject endPrefab;
    public RectTransform gridParent;
    public Vector2Int startPos;
    public Vector2Int endPos;

    public string[] layout = {
        "XXXXXXX",
        "X.....X",
        "X.X.X.X",
        "X.....X",
        "XXXXXXX"
    };

    public GridCell[,] grid;

    public void Init(Action onSuccess, Action onFail)
    {
        grid = new GridCell[layout[0].Length, layout.Length];

        for (int y = 0; y < layout.Length; y++)
        {
            string row = layout[y];
            for (int x = 0; x < row.Length; x++)
            {
                char c = row[x];
                GameObject go = null;

                if (x == startPos.x && y == startPos.y)
                {
                    go = Instantiate(startPrefab, gridParent);
                    grid[x, y] = new GridCell { isWall = false };
                }
                else if (x == endPos.x && y == endPos.y)
                {
                    go = Instantiate(endPrefab, gridParent);
                    grid[x, y] = new GridCell { isWall = false };
                }
                else if (c == 'X')
                {
                    go = Instantiate(wallPrefab, gridParent);
                    grid[x, y] = new GridCell { isWall = true };
                }
                else
                {
                    go = Instantiate(emptyPrefab, gridParent);
                    grid[x, y] = new GridCell { isWall = false };
                }

                go.GetComponent<RectTransform>().anchoredPosition = new Vector2(x * 64, -y * 64);
            }
        }
        
        // После построения всей сетки подвинуть gridParent для центрирования
        float width = layout[0].Length * 64;   // ширина в пикселях
        float height = layout.Length * 64;     // высота в пикселях

        // Сдвигаем так, чтобы сетка была по центру родителя (установить pivot у gridParent должен быть (0.5, 0.5))
        gridParent.anchoredPosition = new Vector2(-width / 2, height / 2);
    }
}