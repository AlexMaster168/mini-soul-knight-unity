using UnityEngine;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator Instance;
    public const int MaxFloors = 3;

    public int gridWidth = 5;
    public int gridHeight = 5;
    public int roomInnerW = 14;
    public int roomInnerH = 10;

    private int[,] grid;
    public Dictionary<Vector2Int, RoomData> rooms = new Dictionary<Vector2Int, RoomData>();
    private RoomData currentRoom;
    private int floor = 1;
    private bool transitioning;

    public bool IsTransitioning { get { return transitioning; } }

    public class RoomData
    {
        public Vector2Int gridPos;
        public Vector3 worldCenter;
        public bool cleared;
        public bool visited;
        public bool revealed;
        public bool isBossRoom;
        public bool isStartRoom;
        public GameObject roomObject;
        public RoomManager roomManager;
        public List<Vector2Int> connections = new List<Vector2Int>();
        public List<RoomDoor> doors = new List<RoomDoor>();

        public void SetDoors(bool closed)
        {
            foreach (RoomDoor d in doors)
                if (d != null) d.SetClosed(closed);
        }
    }

    struct Theme
    {
        public Color floor, line, crack, wall, background, doorGlow, flame;
        public string name;
    }

    Theme GetTheme(int f)
    {
        switch (f)
        {
            case 2:
                return new Theme
                {
                    name = "Frozen Catacombs",
                    floor = new Color(0.28f, 0.36f, 0.42f), line = new Color(0.22f, 0.29f, 0.35f), crack = new Color(0.2f, 0.26f, 0.32f),
                    wall = new Color(0.45f, 0.6f, 0.72f), background = new Color(0.06f, 0.09f, 0.13f),
                    doorGlow = new Color(0.4f, 0.8f, 1f), flame = new Color(0.4f, 0.8f, 1f, 0.9f)
                };
            case 3:
                return new Theme
                {
                    name = "Infernal Citadel",
                    floor = new Color(0.34f, 0.2f, 0.2f), line = new Color(0.26f, 0.14f, 0.15f), crack = new Color(0.4f, 0.12f, 0.08f),
                    wall = new Color(0.6f, 0.32f, 0.28f), background = new Color(0.12f, 0.04f, 0.05f),
                    doorGlow = new Color(1f, 0.35f, 0.1f), flame = new Color(1f, 0.25f, 0.2f, 0.9f)
                };
            default:
                return new Theme
                {
                    name = "Forgotten Crypt",
                    floor = new Color(0.35f, 0.33f, 0.3f), line = new Color(0.28f, 0.26f, 0.24f), crack = new Color(0.25f, 0.23f, 0.2f),
                    wall = new Color(0.6f, 0.5f, 0.4f), background = new Color(0.15f, 0.13f, 0.1f),
                    doorGlow = new Color(0.9f, 0.2f, 0.2f), flame = new Color(1f, 0.6f, 0.1f, 0.9f)
                };
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateFloor();
        FloorTransition.Get().Banner("FLOOR 1", GetTheme(1).name, 2.8f);
    }

    // ================= ГЕНЕРАЦИЯ =================

    void GenerateFloor()
    {
        rooms.Clear();
        grid = new int[gridWidth, gridHeight];

        Theme theme = GetTheme(floor);
        if (Camera.main != null) Camera.main.backgroundColor = theme.background;

        Vector2Int start = new Vector2Int(gridWidth / 2, gridHeight / 2);

        GenerateLayout(start);
        ConnectRooms();

        // start room = (0,0)
        Vector3 startCenter = rooms[start].worldCenter;
        foreach (var kvp in rooms)
            kvp.Value.worldCenter -= startCenter;

        RoomData startRoom = rooms[start];
        startRoom.isStartRoom = true;

        foreach (var kvp in rooms)
            CreateRoomVisual(kvp.Value, theme);

        startRoom.visited = true;
        startRoom.cleared = true; // на старте безопасно
        currentRoom = startRoom;

        foreach (var kvp in rooms)
            kvp.Value.revealed = false;
        Reveal(startRoom);
        UpdateVisibility();
        FocusCamera(startRoom, true);
    }

    void GenerateLayout(Vector2Int start)
    {
        grid[start.x, start.y] = 1;
        rooms[start] = CreateRoomData(start);

        int roomCount = 6 + floor * 2 + Random.Range(0, 3);
        Vector2Int current = start;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        for (int i = 0; i < roomCount; i++)
        {
            Shuffle(dirs);

            bool placed = false;
            foreach (Vector2Int d in dirs)
            {
                Vector2Int next = current + d;
                if (IsValid(next) && grid[next.x, next.y] == 0)
                {
                    grid[next.x, next.y] = 1;
                    rooms[next] = CreateRoomData(next);
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

        // босс - самая дальняя комната
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

    RoomData CreateRoomData(Vector2Int pos)
    {
        float wx = pos.x * (roomInnerW + 2);
        float wy = pos.y * (roomInnerH + 2);
        return new RoomData { gridPos = pos, worldCenter = new Vector3(wx, wy, 0) };
    }

    void CreateRoomVisual(RoomData room, Theme theme)
    {
        GameObject roomObj = new GameObject("Room_" + room.gridPos.x + "_" + room.gridPos.y);
        roomObj.transform.position = room.worldCenter;
        room.roomObject = roomObj;

        RoomManager rm = roomObj.AddComponent<RoomManager>();
        rm.roomData = room;
        room.roomManager = rm;

        CreateFloor(roomObj.transform, room.worldCenter, theme);
        CreateWalls(roomObj.transform, room.worldCenter, room, theme);
        CreateTorches(roomObj.transform, room.worldCenter, theme);

        if (!room.isStartRoom && !room.isBossRoom && TrapManager.Instance != null)
            TrapManager.Instance.SpawnRoomTraps(room.worldCenter, roomInnerW, roomInnerH, room.isBossRoom, roomObj.transform);
    }

    void CreateFloor(Transform parent, Vector3 center, Theme theme)
    {
        Sprite floorA = SpriteGenerator.CreateTile(32, theme.floor, theme.line);
        Sprite floorB = SpriteGenerator.CreateTile(32, theme.floor * 0.95f, theme.line);
        Sprite floorC = SpriteGenerator.CreateTile(32, theme.floor * 1.05f, theme.crack);

        float seed = Random.Range(0f, 100f);
        for (int x = 0; x < roomInnerW; x++)
        {
            for (int y = 0; y < roomInnerH; y++)
            {
                GameObject tile = new GameObject("Tile_" + x + "_" + y);
                tile.transform.SetParent(parent);
                tile.transform.position = center + new Vector3(x - roomInnerW / 2f + 0.5f, y - roomInnerH / 2f + 0.5f, 0);
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();

                float rand = Mathf.PerlinNoise(seed + x * 0.3f, seed + y * 0.3f);
                if (rand > 0.7f)
                    sr.sprite = floorC;
                else if (rand < 0.3f)
                    sr.sprite = floorB;
                else
                    sr.sprite = floorA;

                sr.sortingOrder = 0;
            }
        }

        for (int i = 0; i < 6; i++)
        {
            float dx = Random.Range(-roomInnerW / 2f + 1, roomInnerW / 2f - 1);
            float dy = Random.Range(-roomInnerH / 2f + 1, roomInnerH / 2f - 1);
            SpawnFloorDetail(parent, center + new Vector3(dx, dy, 0));
        }
    }

    void SpawnFloorDetail(Transform parent, Vector3 pos)
    {
        GameObject detail = new GameObject("FloorDetail");
        detail.transform.SetParent(parent);
        detail.transform.position = pos;
        SpriteRenderer sr = detail.AddComponent<SpriteRenderer>();

        int type = Random.Range(0, 3);
        if (type == 0)
            sr.sprite = SpriteGenerator.CreateCircle(6, new Color(0.3f, 0.28f, 0.25f, 0.5f));
        else if (type == 1)
            sr.sprite = SpriteGenerator.CreateSquare(6, new Color(0.28f, 0.25f, 0.22f, 0.4f));
        else
            sr.sprite = SpriteGenerator.CreateCircle(4, new Color(0.4f, 0.35f, 0.2f, 0.3f));

        sr.sortingOrder = 0;
        detail.transform.localScale = Vector3.one * Random.Range(0.1f, 0.2f);
    }

    // Проёмы: N/S - 4 тайла шириной, E/W - 2 тайла высотой (симметрично относительно центра комнаты)
    bool IsGapX(int x) { return Mathf.Abs(x - (roomInnerW - 1) / 2f) <= 1.6f; }
    bool IsGapY(int y) { return Mathf.Abs(y - (roomInnerH - 1) / 2f) <= 0.6f; }

    void CreateWalls(Transform parent, Vector3 center, RoomData room, Theme theme)
    {
        Sprite wallSprite = SpriteGenerator.CreateWall(32, theme.wall);
        Color wallColor = Color.Lerp(theme.wall, Color.white, 0.15f);
        Sprite doorSprite = PixelArt.DoorBars(theme.doorGlow);
        float hw = roomInnerW / 2f;
        float hh = roomInnerH / 2f;

        bool hasNorth = room.connections.Contains(room.gridPos + Vector2Int.up);
        bool hasSouth = room.connections.Contains(room.gridPos + Vector2Int.down);
        bool hasEast = room.connections.Contains(room.gridPos + Vector2Int.right);
        bool hasWest = room.connections.Contains(room.gridPos + Vector2Int.left);

        for (int x = -1; x <= roomInnerW; x++)
        {
            float wx = center.x + x - hw + 0.5f;
            bool gap = x >= 0 && x < roomInnerW && IsGapX(x);

            Vector3 top = new Vector3(wx, center.y + hh + 0.5f, 0);
            if (hasNorth && gap) CreateDoor(parent, room, top, doorSprite, false);
            else CreateWallTile(parent, top, wallSprite, wallColor);

            Vector3 bottom = new Vector3(wx, center.y - hh - 0.5f, 0);
            if (hasSouth && gap) CreateDoor(parent, room, bottom, doorSprite, false);
            else CreateWallTile(parent, bottom, wallSprite, wallColor);
        }

        for (int y = 0; y < roomInnerH; y++)
        {
            float wy = center.y + y - hh + 0.5f;
            bool gap = IsGapY(y);

            Vector3 left = new Vector3(center.x - hw - 0.5f, wy, 0);
            if (hasWest && gap) CreateDoor(parent, room, left, doorSprite, true);
            else CreateWallTile(parent, left, wallSprite, wallColor);

            Vector3 right = new Vector3(center.x + hw + 0.5f, wy, 0);
            if (hasEast && gap) CreateDoor(parent, room, right, doorSprite, true);
            else CreateWallTile(parent, right, wallSprite, wallColor);
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
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
    }

    void CreateDoor(Transform parent, RoomData room, Vector3 pos, Sprite sprite, bool vertical)
    {
        GameObject door = new GameObject("Door");
        door.tag = "Wall";
        door.transform.SetParent(parent);
        door.transform.position = pos;
        if (vertical) door.transform.rotation = Quaternion.Euler(0, 0, 90f);
        SpriteRenderer sr = door.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;
        door.AddComponent<BoxCollider2D>().size = Vector2.one;
        room.doors.Add(door.AddComponent<RoomDoor>());
    }

    void CreateTorches(Transform parent, Vector3 center, Theme theme)
    {
        float hw = roomInnerW / 2f - 1;
        float hh = roomInnerH / 2f - 1;
        CreateTorch(parent, center + new Vector3(-hw, hh, 0), theme);
        CreateTorch(parent, center + new Vector3(hw, hh, 0), theme);
        CreateTorch(parent, center + new Vector3(-hw, -hh, 0), theme);
        CreateTorch(parent, center + new Vector3(hw, -hh, 0), theme);
    }

    void CreateTorch(Transform parent, Vector3 pos, Theme theme)
    {
        GameObject torch = new GameObject("Torch");
        torch.transform.SetParent(parent);
        torch.transform.position = pos;

        SpriteRenderer sr = torch.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateSquare(8, new Color(0.4f, 0.25f, 0.1f));
        sr.sortingOrder = 3;
        torch.transform.localScale = Vector3.one * 0.3f;

        GameObject flame = new GameObject("Flame");
        flame.transform.SetParent(torch.transform);
        flame.transform.localPosition = new Vector3(0, 0.4f, 0);
        SpriteRenderer flameSr = flame.AddComponent<SpriteRenderer>();
        flameSr.sprite = SpriteGenerator.CreateCircle(12, theme.flame);
        flameSr.sortingOrder = 4;
        flame.transform.localScale = Vector3.one * 0.5f;
        TorchFlicker flicker = flame.AddComponent<TorchFlicker>();
        flicker.tint = theme.flame;

        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(torch.transform);
        glow.transform.localPosition = new Vector3(0, 0.3f, 0);
        glow.transform.localScale = Vector3.one * 4f;
        SpriteRenderer glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = SpriteGenerator.CreateCircle(32, new Color(theme.flame.r, theme.flame.g * 0.8f, theme.flame.b, 0.2f));
        glowSr.sortingOrder = 1;
    }

    // ================= ПЕРЕХОД МЕЖДУ КОМНАТАМИ =================

    void Update()
    {
        if (transitioning || currentRoom == null) return;
        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        Vector2 p = player.transform.position;
        float hw = roomInnerW / 2f;
        float hh = roomInnerH / 2f;

        foreach (var kvp in rooms)
        {
            RoomData room = kvp.Value;
            Vector2 d = p - (Vector2)room.worldCenter;

            if (!room.revealed && Mathf.Abs(d.x) < hw + 1.5f && Mathf.Abs(d.y) < hh + 1.5f)
            {
                Reveal(room);
                UpdateVisibility();
            }

            if (room != currentRoom && Mathf.Abs(d.x) < hw - 0.9f && Mathf.Abs(d.y) < hh - 0.9f)
            {
                EnterRoomTo(room);
                break;
            }
        }
    }

    void Reveal(RoomData room)
    {
        room.revealed = true;
    }

    public void EnterRoom(Vector2Int direction)
    {
        Vector2Int nextPos = currentRoom.gridPos + direction;
        if (rooms.ContainsKey(nextPos)) EnterRoomTo(rooms[nextPos]);
    }

    public void EnterRoomTo(RoomData targetRoom)
    {
        if (targetRoom == currentRoom) return;

        currentRoom = targetRoom;
        currentRoom.visited = true;
        Reveal(currentRoom);
        UpdateVisibility();
        FocusCamera(currentRoom, false);

        if (!currentRoom.cleared)
            currentRoom.roomManager.BeginEncounter(floor);

        if (ProceduralMusic.Instance != null)
            ProceduralMusic.Instance.SetBossMode(currentRoom.isBossRoom && !currentRoom.cleared);
    }

    void UpdateVisibility()
    {
        foreach (var kvp in rooms)
        {
            RoomData r = kvp.Value;
            float alpha = !r.revealed ? 0f : (r == currentRoom ? 1f : 0.35f);
            SetRoomAlpha(r.roomObject, alpha);
        }
    }

    void SetRoomAlpha(GameObject roomObj, float alpha)
    {
        foreach (SpriteRenderer sr in roomObj.GetComponentsInChildren<SpriteRenderer>())
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
        foreach (TorchFlicker tf in roomObj.GetComponentsInChildren<TorchFlicker>())
            tf.roomAlpha = alpha;
    }

    void FocusCamera(RoomData room, bool snap)
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        CameraFollow cf = cam.GetComponent<CameraFollow>();
        if (cf != null)
        {
            cf.useFocus = true;
            cf.focus = room.worldCenter;
        }
        if (snap)
            cam.transform.position = new Vector3(room.worldCenter.x, room.worldCenter.y, -10);
    }

    // ================= ЗАЧИСТКА, ЛУТ, ПОРТАЛ =================

    public void RoomCleared(RoomData room)
    {
        SpawnRoomLoot(room);

        if (!room.isBossRoom)
            SpawnShopKeeper(room);

        if (IsAdjacentToBoss(room))
            SpawnBossPrep(room);

        if (!room.isBossRoom && ProceduralMusic.Instance != null)
            ProceduralMusic.Instance.SetBossMode(false);

        // вспышка на дверях - проходы открыты
        foreach (RoomDoor d in room.doors)
            if (d != null && EffectsManager.Instance != null)
                EffectsManager.Instance.SpawnPickupEffect(d.transform.position, new Color(0.4f, 1f, 0.5f));
    }

    bool IsAdjacentToBoss(RoomData room)
    {
        foreach (var kvp in rooms)
        {
            if (kvp.Value.isBossRoom && room.connections.Contains(kvp.Key))
                return true;
        }
        return false;
    }

    public void SpawnPortal(Vector3 pos)
    {
        GameObject portal = new GameObject("Portal");
        portal.transform.position = pos;
        SpriteRenderer sr = portal.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArt.Portal();
        sr.sortingOrder = 6;
        portal.AddComponent<Portal>();

        FloorTransition.Get().Banner("FLOOR CLEARED", "Step into the portal", 3.2f);
    }

    public void StartNextFloor()
    {
        if (transitioning) return;
        transitioning = true;
        floor++;
        FloorTransition.Get().Play("FLOOR " + floor, GetTheme(floor).name, BuildNextFloor, () => transitioning = false);
    }

    void BuildNextFloor()
    {
        foreach (var kvp in rooms)
            if (kvp.Value.roomObject != null) Destroy(kvp.Value.roomObject);

        DestroyAll<WeaponPickup>();
        DestroyAll<PowerUp>();
        DestroyAll<ArmorPickup>();
        DestroyAll<ShopKeeper>();
        DestroyAll<Portal>();
        DestroyAll<Bullet>();
        DestroyAll<Enemy>();

        GenerateFloor();

        PlayerController pc = PlayerController.Instance;
        if (pc != null)
        {
            pc.transform.position = Vector3.zero;
            pc.StopDash();
            // небольшая награда за этаж
            pc.currentHealth = Mathf.Min(pc.maxHealth, pc.currentHealth + pc.maxHealth / 3);
            pc.currentEnergy = pc.maxEnergy;
            // золото с прошлого этажа притягивается к игроку
            foreach (GoldPickup g in FindObjectsByType<GoldPickup>(FindObjectsSortMode.None))
                g.transform.position = pc.transform.position + (Vector3)(Random.insideUnitCircle * 2f);
        }

        if (Camera.main != null)
            Camera.main.transform.position = new Vector3(0, 0, -10);
        if (ProceduralMusic.Instance != null)
            ProceduralMusic.Instance.SetBossMode(false);
    }

    void DestroyAll<T>() where T : Component
    {
        foreach (T t in FindObjectsByType<T>(FindObjectsSortMode.None))
            Destroy(t.gameObject);
    }

    void SpawnBossPrep(RoomData room)
    {
        Vector2 center = room.worldCenter;

        Vector2 wpos = center + new Vector2(-2f, 0);
        GameObject weaponObj = new GameObject("BossWeapon");
        weaponObj.transform.position = wpos;
        SpriteRenderer wsr = weaponObj.AddComponent<SpriteRenderer>();
        wsr.sprite = SpriteGenerator.CreateDropWeapon(16);
        wsr.sortingOrder = 8;
        WeaponPickup wp = weaponObj.AddComponent<WeaponPickup>();
        wp.weaponName = "HolyGrenade";

        Vector2 apos = center + new Vector2(2f, 0);
        GameObject armorObj = new GameObject("ArmorPickup");
        armorObj.transform.position = apos;
        SpriteRenderer asr = armorObj.AddComponent<SpriteRenderer>();
        asr.sprite = SpriteGenerator.CreateDropArmor(16);
        asr.sortingOrder = 8;
        ArmorPickup ap = armorObj.AddComponent<ArmorPickup>();
        ap.armorAmount = 50;
    }

    void SpawnShopKeeper(RoomData room)
    {
        Vector3 pos = room.worldCenter + new Vector3(3.5f, 0, 0);
        GameObject shopkeeper = new GameObject("ShopKeeper");
        shopkeeper.transform.position = pos;
        SpriteRenderer sr = shopkeeper.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateMerchant(32);
        sr.sortingOrder = 10;
        shopkeeper.AddComponent<BoxCollider2D>().isTrigger = true;
        shopkeeper.AddComponent<ShopKeeper>();
    }

    void SpawnRoomLoot(RoomData room)
    {
        float hw = roomInnerW / 2f - 1;
        float hh = roomInnerH / 2f - 1;

        if (room.isBossRoom)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = (Vector2)room.worldCenter + new Vector2(Random.Range(-3f, 3f), Random.Range(-2f, 2f));
                SpawnPickup(pos, i < 3 ? "weapon" : "health");
            }
            SpawnPickup((Vector2)room.worldCenter + new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f)), "energy");
        }
        else
        {
            int lootCount = Random.Range(1, 3);
            string[] lootTypes = { "health", "health", "energy", "energy", "weapon", "damage", "speed" };

            for (int i = 0; i < lootCount; i++)
            {
                Vector2 pos = (Vector2)room.worldCenter + new Vector2(Random.Range(-hw, hw), Random.Range(-hh, hh));
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
                sr.sprite = SpriteGenerator.CreateDropWeapon(16);
                obj.AddComponent<WeaponPickup>();
                break;
            case "health":
                sr.sprite = SpriteGenerator.CreateDropHealth(16);
                PowerUp pu = obj.AddComponent<PowerUp>();
                pu.type = PowerUp.PowerUpType.HealthRestore;
                break;
            case "energy":
                sr.sprite = SpriteGenerator.CreateDropEnergy(16);
                PowerUp pe = obj.AddComponent<PowerUp>();
                pe.type = PowerUp.PowerUpType.EnergyRestore;
                break;
            case "damage":
                sr.sprite = SpriteGenerator.CreateDropDamage(16);
                PowerUp pd = obj.AddComponent<PowerUp>();
                pd.type = PowerUp.PowerUpType.DamageBoost;
                break;
            case "speed":
                sr.sprite = SpriteGenerator.CreateDropSpeed(16);
                PowerUp ps = obj.AddComponent<PowerUp>();
                ps.type = PowerUp.PowerUpType.SpeedBoost;
                break;
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
