using UnityEngine;
using System;

public class CircuitLevel : MonoBehaviour
{
    public Vector2Int start;
    public Vector2Int end;
    public GridCell[,] grid;
    private Vector2Int current;
    public Transform energyVisual;

    public System.Action onSuccess;
    public System.Action onFail;

    public CircuitLevelBuilding levelBuilder;

    void Start()
    {
        current = start;
    }

    public void Init(Action onSuccess, Action onFail)
    {
        this.onSuccess = onSuccess;
        this.onFail = onFail;

        if (levelBuilder != null)
        {
            grid = levelBuilder.grid;
            start = levelBuilder.startPos;
            end = levelBuilder.endPos;
            current = start;
        }
    }

    public class GridCell
    {
        public bool isWall;
    }

    void Update()
    {
        Vector2Int dir = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W)) dir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S)) dir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A)) dir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D)) dir = Vector2Int.right;

        if (dir != Vector2Int.zero)
        {
            Vector2Int next = current + dir;
            if (IsWall(next))
            {
                onFail?.Invoke();
            }
            else
            {
                current = next;
                MoveVisual(current);
                if (current == end)
                    onSuccess?.Invoke();
            }
        }
    }

    bool IsWall(Vector2Int pos)
    {
        return grid[pos.x, pos.y].isWall;
    }

    void MoveVisual(Vector2Int pos)
    {
        Vector2 anchoredPos = new Vector2(pos.x * 64, -pos.y * 64);
        energyVisual.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
    }

}

[System.Serializable]
public class CircuitLevelData
{
    public GameObject levelPrefab;
}