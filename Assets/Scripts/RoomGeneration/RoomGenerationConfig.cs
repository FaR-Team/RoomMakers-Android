using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Room Generation Config", menuName = "Room Generation/Generation Config")]
public class RoomGenerationConfig : ScriptableObject
{
    [Header("Room Types")]
    public RoomType centerRooms;
    public RoomType topRooms;
    public RoomType bottomRooms;
    public RoomType leftRooms;
    public RoomType rightRooms;
    public RoomType topLeftCornerRooms;
    public RoomType topRightCornerRooms;
    public RoomType bottomLeftCornerRooms;
    public RoomType bottomRightCornerRooms;

    [Header("Shop Configuration")]
    public RoomType shopRooms;

    [Header("Grid Configuration")]
    public Vector2Int gridBounds = new Vector2Int(16, 16);
    public Vector2Int roomSize = new Vector2Int(10, 9);
    public Vector2Int gridOffset = new Vector2Int(-7, -15);

    private Dictionary<RoomPosition, RoomType> roomTypeMap;

    private void OnEnable()
    {
        InitializeRoomTypeMap();
    }

    private void InitializeRoomTypeMap()
    {
        roomTypeMap = new Dictionary<RoomPosition, RoomType>
        {
            { RoomPosition.Center, centerRooms },
            { RoomPosition.Top, topRooms },
            { RoomPosition.Bottom, bottomRooms },
            { RoomPosition.Left, leftRooms },
            { RoomPosition.Right, rightRooms },
            { RoomPosition.TopLeftCorner, topLeftCornerRooms },
            { RoomPosition.TopRightCorner, topRightCornerRooms },
            { RoomPosition.BottomLeftCorner, bottomLeftCornerRooms },
            { RoomPosition.BottomRightCorner, bottomRightCornerRooms }
        };
    }

    public RoomPosition GetRoomPosition(Vector2Int gridCoords)
    {
        int x = gridCoords.x;
        int y = gridCoords.y;

        bool isLeftEdge = x == gridOffset.x;
        bool isRightEdge = x == gridOffset.x + gridBounds.x - 1;
        bool isTopEdge = y == gridOffset.y + gridBounds.y - 1;
        bool isBottomEdge = y == gridOffset.y;

        if (isLeftEdge && isTopEdge) return RoomPosition.TopLeftCorner;
        if (isRightEdge && isTopEdge) return RoomPosition.TopRightCorner;
        if (isLeftEdge && isBottomEdge) return RoomPosition.BottomLeftCorner;
        if (isRightEdge && isBottomEdge) return RoomPosition.BottomRightCorner;

        if (isLeftEdge) return RoomPosition.Left;
        if (isRightEdge) return RoomPosition.Right;
        if (isTopEdge) return RoomPosition.Top;
        if (isBottomEdge) return RoomPosition.Bottom;

        return RoomPosition.Center;
    }

    public RoomType GetRoomType(RoomPosition position)
    {
        if (roomTypeMap == null) InitializeRoomTypeMap();

        return roomTypeMap.TryGetValue(position, out RoomType roomType) ? roomType : null;
    }

    public Vector2Int WorldToGridCoords(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / roomSize.x),
            Mathf.RoundToInt(worldPosition.y / roomSize.y)
        );
    }

    public Vector3 GridToWorldPosition(Vector2Int gridCoords)
    {
        return new Vector3(
            gridCoords.x * roomSize.x,
            gridCoords.y * roomSize.y,
            0
        );
    }
}