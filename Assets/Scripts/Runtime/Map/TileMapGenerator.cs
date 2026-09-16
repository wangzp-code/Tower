using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class TileMapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapWidth = 13;
    public int mapHeight = 13;
    public int tileSize = 40;

    [Header("Tile Rules")]
    public float wallRatio = 0.3f;
    public int roomCount = 5;
    public int minRoomSize = 3;
    public int maxRoomSize = 5;

    [Header("Prefabs")]
    public TileBase floorTile;
    public TileBase wallTile;
    public GameObject exitPrefab;
    public GameObject[] monsterPrefabs;
    public GameObject[] itemPrefabs;

    [Header("References")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    private int currentFloor;
    private readonly HashSet<Vector3Int> floorTiles = new HashSet<Vector3Int>();
    private readonly HashSet<Vector3Int> wallTiles = new HashSet<Vector3Int>();
    private readonly List<Room> rooms = new List<Room>();

    [System.Serializable]
    private class Room
    {
        public Vector3Int center;
        public int width, height;
        public List<Vector3Int> tiles;
    }

    public void GenerateMap(int floor)
    {
        currentFloor = floor;
        ClearMap();

        int seed = floor * 1337 + 17;
        Random.InitState(seed);

        GenerateRooms();
        ConnectRooms();
        PlaceWalls();
        AddDecorations();
        SpawnEntities();
        CreateExit();
    }

    private void GenerateRooms()
    {
        rooms.Clear();
        for (int i = 0; i < roomCount; i++)
        {
            int roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);
            int x = Random.Range(1, mapWidth - roomWidth - 1);
            int y = Random.Range(1, mapHeight - roomHeight - 1);

            Room room = new Room
            {
                center = new Vector3Int(x + roomWidth / 2, y + roomHeight / 2, 0),
                width = roomWidth,
                height = roomHeight,
                tiles = new List<Vector3Int>()
            };

            for (int rx = 0; rx < roomWidth; rx++)
            {
                for (int ry = 0; ry < roomHeight; ry++)
                {
                    var tile = new Vector3Int(x + rx, y + ry, 0);
                    room.tiles.Add(tile);
                    floorTiles.Add(tile);
                    if (floorTilemap != null && floorTile != null)
                    {
                        floorTilemap.SetTile(tile, floorTile);
                    }
                }
            }

            rooms.Add(room);
        }
    }

    private void ConnectRooms()
    {
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            var start = rooms[i].center;
            var end = rooms[i + 1].center;
            CreateCorridor(start, end);
        }
    }

    private void CreateCorridor(Vector3Int start, Vector3Int end)
    {
        int x = start.x;
        int y = start.y;

        while (x != end.x)
        {
            AddFloorTile(x, y);
            x += x < end.x ? 1 : -1;
        }

        while (y != end.y)
        {
            AddFloorTile(x, y);
            y += y < end.y ? 1 : -1;
        }
    }

    private void AddFloorTile(int x, int y)
    {
        var tile = new Vector3Int(x, y, 0);
        if (floorTiles.Add(tile) && floorTilemap != null && floorTile != null)
        {
            floorTilemap.SetTile(tile, floorTile);
        }
    }

    private void PlaceWalls()
    {
        wallTiles.Clear();
        HashSet<Vector3Int> checkedTiles = new HashSet<Vector3Int>();

        foreach (var floor in floorTiles)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var check = new Vector3Int(floor.x + dx, floor.y + dy, 0);
                    if (checkedTiles.Contains(check)) continue;
                    checkedTiles.Add(check);

                    if (!floorTiles.Contains(check) && IsInBounds(check))
                    {
                        wallTiles.Add(check);
                        if (wallTilemap != null && wallTile != null)
                        {
                            wallTilemap.SetTile(check, wallTile);
                        }
                    }
                }
            }
        }
    }

    private bool IsInBounds(Vector3Int pos)
    {
        return pos.x >= 0 && pos.x < mapWidth && pos.y >= 0 && pos.y < mapHeight;
    }

    private void AddDecorations()
    {
    }

    private void SpawnEntities()
    {
        int monsterCount = GetMonsterCountForFloor(currentFloor);
        for (int i = 0; i < monsterCount; i++)
        {
            SpawnMonster();
        }
    }

    private void SpawnMonster()
    {
        if (floorTiles.Count == 0 || monsterPrefabs == null || monsterPrefabs.Length == 0 || floorTilemap == null)
        {
            return;
        }

        var tiles = new List<Vector3Int>(floorTiles);
        var spawnTile = tiles[Random.Range(0, tiles.Count)];
        var prefab = monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];
        Vector3 worldPos = floorTilemap.CellToWorld(spawnTile) + new Vector3(tileSize / 2f, tileSize / 2f, 0);
        Instantiate(prefab, worldPos, Quaternion.identity);
    }

    private int GetMonsterCountForFloor(int floor)
    {
        if (GameManager.Instance == null) return 4;
        return FloorManager.Instance != null ? FloorManager.Instance.GetMonsterCount(floor) : 4;
    }

    private void CreateExit()
    {
        if (rooms.Count > 0 && exitPrefab != null && floorTilemap != null)
        {
            var lastRoom = rooms[rooms.Count - 1];
            Vector3 worldPos = floorTilemap.CellToWorld(lastRoom.center) + new Vector3(tileSize / 2f, tileSize / 2f, 0);
            Instantiate(exitPrefab, worldPos, Quaternion.identity);
        }
    }

    private void ClearMap()
    {
        if (floorTilemap != null) floorTilemap.ClearAllTiles();
        if (wallTilemap != null) wallTilemap.ClearAllTiles();
        floorTiles.Clear();
        wallTiles.Clear();
        rooms.Clear();
    }

    public bool IsWalkable(Vector3 worldPos)
    {
        if (floorTilemap == null) return false;
        var cell = floorTilemap.WorldToCell(worldPos);
        return floorTiles.Contains(cell);
    }

    public List<Vector3Int> GetFloorTiles() => new List<Vector3Int>(floorTiles);
}
