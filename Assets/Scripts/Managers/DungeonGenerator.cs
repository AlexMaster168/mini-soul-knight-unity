using UnityEngine;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator Instance;

    public int gridWidth = 5;
    public int gridHeight = 5;
    public int roomInnerW = 14;
    public int roomInnerH = 10;

    private int[,] grid;
    public Dictionary<Vector2Int, RoomData> rooms = new Dictionary<Vector2Int, RoomData>();
    private RoomData currentRoom;
    private int floor = 1;
    private int totalRooms;

    public class RoomData
    {
        public Vector2Int gridPos;
        public Vector3 worldCenter;
        public bool cleared;
        public bool visited;
        public bool isBossRoom;
        public bool isStartRoom;
        public GameObject roomObject;
        public RoomManager roomManager;
        public List<Vector2Int> connections = new List<Vector2Int>();
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateFloor();
    }

    void GenerateFloor()
    {
        rooms.Clear();
        grid = new int[gridWidth, gridHeight];

        int startX = gridWidth / 2;
        int startY = gridHeight / 2;
        Vector2Int start = new Vector2Int(startX, startY);

        GenerateLayout(start);
        ConnectRooms();

        // Offset all rooms so start room is at (0,0)
        Vector3 startCenter = rooms[start].worldCenter;
        foreach (var kvp in rooms)
            kvp.Value.worldCenter -= startCenter;

        foreach (var kvp in rooms)
            CreateRoomVisual(kvp.Value);

        totalRooms = rooms.Count;
        RoomData startRoom = rooms[start];
        startRoom.visited = true;
        startRoom.isStartRoom = true;
        currentRoom = startRoom;

        // Show all rooms, dim non-current
        foreach (var kvp in rooms)
        {
            kvp.Value.roomObject.SetActive(true);
            SetRoomDimmed(kvp.Value.roomObject, kvp.Key != start);
        }
        Camera.main.transform.position = new Vector3(startRoom.worldCenter.x, startRoom.worldCenter.y, -10);

        // Spawn enemies in start room
        startRoom.roomManager.SpawnEnemies(floor);
    }

    void GenerateLayout(Vector2Int start)
    {
        grid[start.x, start.y] = 1;
        rooms[start] = CreateRoomData(start, false);

        int roomCount = Random.Range(12, 18);
        Vector2Int current = start;

        for (int i = 0; i < roomCount; i++)
        {
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            Shuffle(dirs);

            bool placed = false;
            foreach (Vector2Int d in dirs)
            {
                Vector2Int next = current + d;
                if (IsValid(next) && grid[next.x, next.y] == 0)
                {
                    grid[next.x, next.y] = 1;
                    rooms[next] = CreateRoomData(next, false);
                    rooms[current].connections.Add(next);
                    rooms[next].connections.Add(current);
                    current = next;
                    placed = true;
                    break;
                }
            }
            if (!placed)
            {
                List<Vector2Int> keys = new List<Vector2Int>(rooms.Keys);
                current = keys[Random.Range(0, keys.Count)];
            }
        }

        float maxDist = 0;
        Vector2Int bossPos = start;
        foreach (var kvp in rooms)
        {
            float d = Vector2Int.Distance(kvp.Key, start);
            if (d > maxDist)
            {
                maxDist = d;
                bossPos = kvp.Key;
            }
        }
        rooms[bossPos].isBossRoom = true;
    }

    void ConnectRooms()
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var kvp in rooms)
        {
            foreach (Vector2Int d in dirs)
            {
                Vector2Int neighbor = kvp.Key + d;
                if (rooms.ContainsKey(neighbor) && !kvp.Value.connections.Contains(neighbor))
                {
                    kvp.Value.connections.Add(neighbor);
                    rooms[neighbor].connections.Add(kvp.Key);
                }
            }
        }
    }

    RoomData CreateRoomData(Vector2Int pos, bool cleared)
    {
        float wx = pos.x * (roomInnerW + 2);
        float wy = pos.y * (roomInnerH + 2);
        return new RoomData
        {
            gridPos = pos,
            worldCenter = new Vector3(wx, wy, 0),
            cleared = cleared,
            visited = false,
            isBossRoom = false,
            isStartRoom = false
        };
    }

    void CreateRoomVisual(RoomData room)
    {
        GameObject roomObj = new GameObject("Room_" + room.gridPos.x + "_" + room.gridPos.y);
        roomObj.transform.position = room.worldCenter;
        room.roomObject = roomObj;
        roomObj.SetActive(false);

        RoomManager rm = roomObj.AddComponent<RoomManager>();
        rm.roomData = room;
        room.roomManager = rm;

        CreateFloor(roomObj.transform, room.worldCenter);
        CreateWalls(roomObj.transform, room.worldCenter, room);
        CreateTorches(roomObj.transform, room.worldCenter);
    }

    void CreateFloor(Transform parent, Vector3 center)
    {
        Sprite floorSprite = SpriteGenerator.CreateTile(32, new Color(0.35f, 0.33f, 0.3f), new Color(0.28f, 0.26f, 0.24f));
        for (int x = 0; x < roomInnerW; x++)
        {
            for (int y = 0; y < roomInnerH; y++)
            {
                GameObject tile = new GameObject("Tile_" + x + "_" + y);
                tile.transform.SetParent(parent);
                tile.transform.position = center + new Vector3(x - roomInnerW / 2f + 0.5f, y - roomInnerH / 2f + 0.5f, 0);
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = floorSprite;
                sr.sortingOrder = 0;
            }
        }
    }

    void CreateWalls(Transform parent, Vector3 center, RoomData room)
    {
        Sprite wallSprite = SpriteGenerator.CreateWall(32, new Color(0.55f, 0.45f, 0.35f));
        Color wallColor = new Color(0.6f, 0.5f, 0.4f);
        float hw = roomInnerW / 2f;
        float hh = roomInnerH / 2f;

        bool hasNorth = room.connections.Contains(room.gridPos + Vector2Int.up);
        bool hasSouth = room.connections.Contains(room.gridPos + Vector2Int.down);
        bool hasEast = room.connections.Contains(room.gridPos + Vector2Int.right);
        bool hasWest = room.connections.Contains(room.gridPos + Vector2Int.left);

        // Top wall
        for (int x = -1; x <= roomInnerW; x++)
        {
            float wx = center.x + x - hw + 0.5f;
            float wy = center.y + hh + 0.5f;
            if (hasNorth && Mathf.Abs(x - roomInnerW / 2) <= 1) continue;
            CreateWallTile(parent, new Vector3(wx, wy, 0), wallSprite, wallColor);
        }

        // Bottom wall
        for (int x = -1; x <= roomInnerW; x++)
        {
            float wx = center.x + x - hw + 0.5f;
            float wy = center.y - hh - 0.5f;
            if (hasSouth && Mathf.Abs(x - roomInnerW / 2) <= 1) continue;
            CreateWallTile(parent, new Vector3(wx, wy, 0), wallSprite, wallColor);
        }

        // Left wall
        for (int y = 0; y < roomInnerH; y++)
        {
            float wx = center.x - hw - 0.5f;
            float wy = center.y + y - hh + 0.5f;
            if (hasWest && Mathf.Abs(y - roomInnerH / 2) <= 0) continue;
            CreateWallTile(parent, new Vector3(wx, wy, 0), wallSprite, wallColor);
        }

        // Right wall
        for (int y = 0; y < roomInnerH; y++)
        {
            float wx = center.x + hw + 0.5f;
            float wy = center.y + y - hh + 0.5f;
            if (hasEast && Mathf.Abs(y - roomInnerH / 2) <= 0) continue;
            CreateWallTile(parent, new Vector3(wx, wy, 0), wallSprite, wallColor);
        }

    }

    void CreateWallTile(Transform parent, Vector3 pos, Sprite sprite, Color color)
    {
        GameObject wall = new GameObject("Wall");
        wall.tag = "Wall";
        wall.transform.SetParent(parent);
        wall.transform.position = pos;
        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = 1;
    }

    void CreateTorches(Transform parent, Vector3 center)
    {
        float hw = roomInnerW / 2f - 1;
        float hh = roomInnerH / 2f - 1;
        CreateTorch(parent, center + new Vector3(-hw, hh, 0));
        CreateTorch(parent, center + new Vector3(hw, hh, 0));
        CreateTorch(parent, center + new Vector3(-hw, -hh, 0));
        CreateTorch(parent, center + new Vector3(hw, -hh, 0));
    }

    void CreateTorch(Transform parent, Vector3 pos)
    {
        GameObject torch = new GameObject("Torch");
        torch.transform.SetParent(parent);
        torch.transform.position = pos;
        SpriteRenderer sr = torch.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(16, new Color(1f, 0.6f, 0.1f, 0.8f));
        sr.sortingOrder = 2;

        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(torch.transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = Vector3.one * 3f;
        SpriteRenderer glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = SpriteGenerator.CreateCircle(16, new Color(1f, 0.5f, 0.1f, 0.3f));
        glowSr.sortingOrder = 1;
    }

    public void FindAndEnterNearestRoom(Vector3 playerPos)
    {
        RoomData nearest = null;
        float bestDist = float.MaxValue;

        foreach (var kvp in rooms)
        {
            if (kvp.Value == currentRoom) continue;
            float d = Vector2.Distance(playerPos, kvp.Value.worldCenter);
            if (d < bestDist)
            {
                bestDist = d;
                nearest = kvp.Value;
            }
        }

        if (nearest != null)
            EnterRoomTo(nearest);
    }

    public void EnterRoom(Vector2Int direction)
    {
        Vector2Int nextPos = currentRoom.gridPos + direction;
        if (!rooms.ContainsKey(nextPos)) return;
        EnterRoomTo(rooms[nextPos]);
    }

    public void EnterRoomTo(RoomData targetRoom)
    {
        if (targetRoom == currentRoom) return;

        currentRoom = targetRoom;
        currentRoom.visited = true;

        foreach (var kvp in rooms)
        {
            kvp.Value.roomObject.SetActive(true);
            bool isCurrent = kvp.Key == currentRoom.gridPos;
            SetRoomDimmed(kvp.Value.roomObject, !isCurrent);
        }

        PlayerController player = PlayerController.Instance;
        if (player != null)
        {
            player.transform.position = currentRoom.worldCenter;
            player.StopDash();
        }

        Camera.main.transform.position = new Vector3(currentRoom.worldCenter.x, currentRoom.worldCenter.y, -10);

        CameraFollow cf = Camera.main.GetComponent<CameraFollow>();
        if (cf != null) cf.enabled = false;
        StartCoroutine(EnableCameraFollow());

        if (!currentRoom.cleared)
            currentRoom.roomManager.SpawnEnemies(floor);
    }

    System.Collections.IEnumerator EnableCameraFollow()
    {
        yield return new WaitForSeconds(0.3f);
        CameraFollow cf = Camera.main.GetComponent<CameraFollow>();
        if (cf != null) cf.enabled = true;
    }

    public void RoomCleared()
    {
        currentRoom.cleared = true;
        SpawnRoomLoot();

        if (IsAdjacentToBoss())
            SpawnBossPrep();

        CheckFloorComplete();
    }

    bool IsAdjacentToBoss()
    {
        foreach (var kvp in rooms)
        {
            if (kvp.Value.isBossRoom && currentRoom.connections.Contains(kvp.Key))
                return true;
        }
        return false;
    }

    void SpawnBossPrep()
    {
        Vector2 center = currentRoom.worldCenter;

        Vector2 wpos = center + new Vector2(-2f, 0);
        GameObject weaponObj = new GameObject("BossWeapon");
        weaponObj.transform.position = wpos;
        SpriteRenderer wsr = weaponObj.AddComponent<SpriteRenderer>();
        wsr.sprite = SpriteGenerator.CreateSquare(16, new Color(1f, 0.4f, 0f));
        wsr.sortingOrder = 8;
        WeaponPickup wp = weaponObj.AddComponent<WeaponPickup>();
        wp.weaponName = "HolyGrenade";

        Vector2 apos = center + new Vector2(2f, 0);
        GameObject armorObj = new GameObject("ArmorPickup");
        armorObj.transform.position = apos;
        SpriteRenderer asr = armorObj.AddComponent<SpriteRenderer>();
        asr.sprite = SpriteGenerator.CreateSquare(16, new Color(0.3f, 0.7f, 1f));
        asr.sortingOrder = 8;
        ArmorPickup ap = armorObj.AddComponent<ArmorPickup>();
        ap.armorAmount = 50;
    }

    void SpawnRoomLoot()
    {
        float hw = roomInnerW / 2f - 1;
        float hh = roomInnerH / 2f - 1;

        if (currentRoom.isBossRoom)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = (Vector2)currentRoom.worldCenter + new Vector2(Random.Range(-3f, 3f), Random.Range(-2f, 2f));
                SpawnPickup(pos, i < 3 ? "weapon" : "health");
            }
            SpawnPickup((Vector2)currentRoom.worldCenter + new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f)), "energy");
        }
        else
        {
            int lootCount = Random.Range(2, 5);
            string[] lootTypes = { "health", "health", "energy", "energy", "weapon", "damage", "speed" };

            for (int i = 0; i < lootCount; i++)
            {
                Vector2 pos = (Vector2)currentRoom.worldCenter + new Vector2(Random.Range(-hw, hw), Random.Range(-hh, hh));
                string type = lootTypes[Random.Range(0, lootTypes.Length)];
                SpawnPickup(pos, type);
            }
        }
    }

    void SpawnPickup(Vector2 position, string type)
    {
        GameObject obj = new GameObject("Pickup_" + type);
        obj.transform.position = position;
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 8;

        switch (type)
        {
            case "weapon":
                sr.sprite = SpriteGenerator.CreateSquare(16, new Color(0.9f, 0.8f, 0.1f));
                obj.AddComponent<WeaponPickup>();
                break;
            case "health":
                sr.sprite = SpriteGenerator.CreateCircle(16, new Color(0.2f, 0.9f, 0.2f));
                PowerUp pu = obj.AddComponent<PowerUp>();
                pu.type = PowerUp.PowerUpType.HealthRestore;
                break;
            case "energy":
                sr.sprite = SpriteGenerator.CreateCircle(16, new Color(0.2f, 0.5f, 1f));
                PowerUp pe = obj.AddComponent<PowerUp>();
                pe.type = PowerUp.PowerUpType.EnergyRestore;
                break;
            case "damage":
                sr.sprite = SpriteGenerator.CreateCircle(16, new Color(0.9f, 0.2f, 0.2f));
                PowerUp pd = obj.AddComponent<PowerUp>();
                pd.type = PowerUp.PowerUpType.DamageBoost;
                break;
            case "speed":
                sr.sprite = SpriteGenerator.CreateCircle(16, new Color(0.2f, 0.9f, 0.9f));
                PowerUp ps = obj.AddComponent<PowerUp>();
                ps.type = PowerUp.PowerUpType.SpeedBoost;
                break;
        }
    }

    void CheckFloorComplete()
    {
        int visited = 0;
        foreach (var kvp in rooms)
            if (kvp.Value.visited) visited++;

        if (visited >= totalRooms)
        {
            floor++;
            foreach (var kvp in rooms)
                if (kvp.Value.roomObject != null) Destroy(kvp.Value.roomObject);
            GenerateFloor();
        }
    }

    void ActivateRoom(RoomData room)
    {
        foreach (var kvp in rooms)
        {
            kvp.Value.roomObject.SetActive(true);
            bool isCurrent = kvp.Key == room.gridPos;
            SetRoomDimmed(kvp.Value.roomObject, !isCurrent);
        }
        Camera.main.transform.position = new Vector3(room.worldCenter.x, room.worldCenter.y, -10);
    }

    void SetRoomDimmed(GameObject roomObj, bool dimmed)
    {
        float alpha = dimmed ? 0.3f : 1f;
        foreach (SpriteRenderer sr in roomObj.GetComponentsInChildren<SpriteRenderer>())
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
        foreach (Collider2D col in roomObj.GetComponentsInChildren<Collider2D>())
        {
            if (col.gameObject.CompareTag("Wall"))
                col.enabled = !dimmed;
        }
    }

    bool IsValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
    }

    public RoomData GetCurrentRoom() { return currentRoom; }
    public int GetFloor() { return floor; }

    void Shuffle(Vector2Int[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector2Int tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }
    }
}
