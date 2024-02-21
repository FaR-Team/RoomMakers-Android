using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Grid grid;
    static GridManager instance;

    public static Grid _grid => instance.grid;

    private void Awake()
    {
        instance = this;
    }

    public static Vector2 PositionToCellCenter(Vector2 pos)
    {
        var cell = _grid.WorldToCell(pos);
        return _grid.GetCellCenterWorld(cell);
    }
}