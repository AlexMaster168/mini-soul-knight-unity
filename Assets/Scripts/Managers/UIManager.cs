using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private Text healthText;
    private Text energyText;
    private Text armorText;
    private Text levelText;
    private Text scoreText;
    private Text weaponText;
    private Image healthBar;
    private Image energyBar;
    private Image armorBar;
    private RawImage minimapImage;

    private Texture2D minimapTexture;
    private const int MINIMAP_SIZE = 120;

    private GameObject statsPanel;
    private Text statsText;
    private bool statsVisible;

    void Awake()
    {
        Instance = this;
        CreateUI();
    }

    void CreateUI()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // === HP BAR ===
        GameObject hpBg = CreatePanel(canvas.transform, "HpBg", new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -10), new Vector2(220, 28));
        Image hpBgImg = hpBg.AddComponent<Image>();
        hpBgImg.color = new Color(0.15f, 0.02f, 0.02f, 0.95f);

        GameObject hpFill = CreatePanel(hpBg.transform, "HpFill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        healthBar = hpFill.AddComponent<Image>();
        healthBar.color = new Color(0.85f, 0.1f, 0.1f);

        GameObject hpText = CreatePanel(hpBg.transform, "HpText", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        healthText = hpText.AddComponent<Text>();
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthText.fontSize = 14;
        healthText.color = Color.white;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.text = "150 / 150";

        // === ENERGY BAR ===
        GameObject enBg = CreatePanel(canvas.transform, "EnBg", new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -42), new Vector2(220, 20));
        Image enBgImg = enBg.AddComponent<Image>();
        enBgImg.color = new Color(0.02f, 0.02f, 0.15f, 0.95f);

        GameObject enFill = CreatePanel(enBg.transform, "EnFill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        energyBar = enFill.AddComponent<Image>();
        energyBar.color = new Color(0.2f, 0.5f, 1f);

        GameObject enText = CreatePanel(enBg.transform, "EnText", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        energyText = enText.AddComponent<Text>();
        energyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        energyText.fontSize = 11;
        energyText.color = Color.white;
        energyText.alignment = TextAnchor.MiddleCenter;
        energyText.text = "200 / 200";

        // === ARMOR BAR ===
        GameObject arBg = CreatePanel(canvas.transform, "ArBg", new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -66), new Vector2(220, 16));
        Image arBgImg = arBg.AddComponent<Image>();
        arBgImg.color = new Color(0.02f, 0.05f, 0.15f, 0.9f);

        GameObject arFill = CreatePanel(arBg.transform, "ArFill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        armorBar = arFill.AddComponent<Image>();
        armorBar.color = new Color(0.3f, 0.7f, 1f);

        GameObject arText = CreatePanel(arBg.transform, "ArText", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        armorText = arText.AddComponent<Text>();
        armorText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        armorText.fontSize = 10;
        armorText.color = Color.white;
        armorText.alignment = TextAnchor.MiddleCenter;
        armorText.text = "Armor: 0";

        // === FLOOR TEXT ===
        GameObject floorObj = CreatePanel(canvas.transform, "Floor", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(200, 35));
        levelText = floorObj.AddComponent<Text>();
        levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        levelText.fontSize = 24;
        levelText.color = new Color(1f, 0.85f, 0.3f);
        levelText.alignment = TextAnchor.MiddleCenter;
        levelText.fontStyle = FontStyle.Bold;
        levelText.text = "Floor 1";

        // === SCORE ===
        GameObject scoreObj = CreatePanel(canvas.transform, "Score", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10, -10), new Vector2(180, 30));
        scoreText = scoreObj.AddComponent<Text>();
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = 18;
        scoreText.color = Color.white;
        scoreText.alignment = TextAnchor.UpperRight;
        scoreText.text = "Score: 0";

        // === WEAPON ===
        GameObject weaponObj = CreatePanel(canvas.transform, "Weapon", new Vector2(0, 0), new Vector2(0, 0), new Vector2(10, 65), new Vector2(220, 30));
        weaponText = weaponObj.AddComponent<Text>();
        weaponText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        weaponText.fontSize = 16;
        weaponText.color = new Color(1f, 0.9f, 0.4f);
        weaponText.fontStyle = FontStyle.Bold;
        weaponText.text = "Pistol";

        // === CONTROLS HINT ===
        GameObject hintObj = CreatePanel(canvas.transform, "Hint", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 5), new Vector2(500, 25));
        Text hintText = hintObj.AddComponent<Text>();
        hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hintText.fontSize = 12;
        hintText.color = new Color(1f, 1f, 1f, 0.4f);
        hintText.alignment = TextAnchor.MiddleCenter;
        hintText.text = "WASD Move | Click Shoot | Space Dash | Q Switch | Tab Stats";

        // === MINIMAP ===
        GameObject mmObj = CreatePanel(canvas.transform, "Minimap", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-10, 35), new Vector2(MINIMAP_SIZE, MINIMAP_SIZE));
        minimapImage = mmObj.AddComponent<RawImage>();
        minimapImage.raycastTarget = false;

        minimapTexture = new Texture2D(MINIMAP_SIZE, MINIMAP_SIZE, TextureFormat.RGBA32, false);
        minimapTexture.filterMode = FilterMode.Point;
        minimapImage.texture = minimapTexture;

        // Border
        GameObject border = CreatePanel(mmObj.transform, "Border", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image borderImg = border.AddComponent<Image>();
        borderImg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        border.transform.SetAsFirstSibling();
        RectTransform borderRect = border.GetComponent<RectTransform>();
        borderRect.offsetMin = new Vector2(-2, -2);
        borderRect.offsetMax = new Vector2(2, 2);

        // === STATS PANEL (Tab) ===
        statsPanel = CreatePanel(canvas.transform, "StatsPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(350, 400));
        Image statsBg = statsPanel.AddComponent<Image>();
        statsBg.color = new Color(0.05f, 0.05f, 0.1f, 0.92f);
        statsPanel.SetActive(false);

        GameObject statsTitle = CreatePanel(statsPanel.transform, "Title", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -10), new Vector2(0, 35));
        Text titleText = statsTitle.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 22;
        titleText.color = new Color(1f, 0.85f, 0.3f);
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontStyle = FontStyle.Bold;
        titleText.text = "PLAYER STATS";

        GameObject statsContent = CreatePanel(statsPanel.transform, "Content", new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 10), new Vector2(-20, -55));
        statsText = statsContent.AddComponent<Text>();
        statsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statsText.fontSize = 15;
        statsText.color = Color.white;
        statsText.alignment = TextAnchor.UpperLeft;
        statsText.supportRichText = true;
    }

    GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return obj;
    }

    void UpdateMinimap()
    {
        if (DungeonGenerator.Instance == null || minimapTexture == null) return;

        Color[] clear = new Color[MINIMAP_SIZE * MINIMAP_SIZE];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = new Color(0.05f, 0.05f, 0.1f, 0.8f);
        minimapTexture.SetPixels(clear);

        DungeonGenerator.RoomData current = DungeonGenerator.Instance.GetCurrentRoom();
        if (current == null) return;

        int centerX = MINIMAP_SIZE / 2;
        int centerY = MINIMAP_SIZE / 2;
        int roomPx = 12;
        int gap = 3;

        foreach (var room in DungeonGenerator.Instance.rooms.Values)
        {
            if (!room.visited) continue;

            int rx = centerX + (room.gridPos.x - current.gridPos.x) * (roomPx + gap);
            int ry = centerY + (room.gridPos.y - current.gridPos.y) * (roomPx + gap);

            Color roomColor;
            if (room == current)
                roomColor = new Color(0.2f, 0.95f, 0.3f, 1f);
            else if (room.isBossRoom)
                roomColor = new Color(0.95f, 0.15f, 0.15f, 0.9f);
            else if (room.cleared)
                roomColor = new Color(0.4f, 0.4f, 0.5f, 0.8f);
            else
                roomColor = new Color(0.7f, 0.65f, 0.2f, 0.85f);

            DrawMinimapRoom(rx, ry, roomPx, roomColor);

            if (room.connections.Contains(Vector2Int.up))
                DrawMinimapCorridor(rx, ry + roomPx / 2, gap, roomPx / 3, roomColor * 0.7f);
            if (room.connections.Contains(Vector2Int.down))
                DrawMinimapCorridor(rx, ry - roomPx / 2, gap, roomPx / 3, roomColor * 0.7f);
            if (room.connections.Contains(Vector2Int.right))
                DrawMinimapCorridor(rx + roomPx / 2, ry, roomPx / 3, gap, roomColor * 0.7f);
            if (room.connections.Contains(Vector2Int.left))
                DrawMinimapCorridor(rx - roomPx / 2, ry, roomPx / 3, gap, roomColor * 0.7f);
        }

        minimapTexture.Apply();
    }

    void DrawMinimapRoom(int cx, int cy, int size, Color color)
    {
        int half = size / 2;
        for (int y = cy - half; y <= cy + half; y++)
            for (int x = cx - half; x <= cx + half; x++)
                if (x >= 0 && x < MINIMAP_SIZE && y >= 0 && y < MINIMAP_SIZE)
                    minimapTexture.SetPixel(x, y, color);
    }

    void DrawMinimapCorridor(int cx, int cy, int w, int h, Color color)
    {
        for (int y = cy - h / 2; y <= cy + h / 2; y++)
            for (int x = cx - w / 2; x <= cx + w / 2; x++)
                if (x >= 0 && x < MINIMAP_SIZE && y >= 0 && y < MINIMAP_SIZE)
                    minimapTexture.SetPixel(x, y, color);
    }

    void Update()
    {
        if (PlayerController.Instance != null)
        {
            float hpRatio = (float)PlayerController.Instance.currentHealth / PlayerController.Instance.maxHealth;
            healthBar.rectTransform.anchorMax = new Vector2(Mathf.Max(hpRatio, 0f), 1);
            healthText.text = PlayerController.Instance.currentHealth + " / " + PlayerController.Instance.maxHealth;

            float enRatio = (float)PlayerController.Instance.currentEnergy / PlayerController.Instance.maxEnergy;
            energyBar.rectTransform.anchorMax = new Vector2(Mathf.Max(enRatio, 0f), 1);
            energyText.text = Mathf.CeilToInt(PlayerController.Instance.currentEnergy) + " / " + PlayerController.Instance.maxEnergy;

            float arRatio = PlayerController.Instance.maxArmor > 0 ? (float)PlayerController.Instance.armor / PlayerController.Instance.maxArmor : 0f;
            armorBar.rectTransform.anchorMax = new Vector2(Mathf.Max(arRatio, 0f), 1);
            armorText.text = "Armor: " + PlayerController.Instance.armor;

            Inventory inv = PlayerController.Instance.GetComponent<Inventory>();
            if (inv != null && inv.weapons.Count > inv.currentWeaponIndex)
                weaponText.text = inv.weapons[inv.currentWeaponIndex];
        }

        if (DungeonGenerator.Instance != null)
            levelText.text = "Floor " + DungeonGenerator.Instance.GetFloor();

        if (ScoreManager.Instance != null)
            scoreText.text = "Score: " + ScoreManager.Instance.GetScore();

        if (statsVisible)
            UpdateStatsText();

        UpdateMinimap();
    }

    public void ToggleStatsPanel()
    {
        statsVisible = !statsVisible;
        statsPanel.SetActive(statsVisible);
        if (statsVisible)
            UpdateStatsText();
    }

    void UpdateStatsText()
    {
        if (PlayerController.Instance == null) return;
        var p = PlayerController.Instance;

        Inventory inv = p.GetComponent<Inventory>();
        string weaponName = "Pistol";
        int weaponCount = 0;
        if (inv != null && inv.weapons.Count > 0)
        {
            weaponName = inv.weapons[inv.currentWeaponIndex];
            weaponCount = inv.weapons.Count;
        }

        int score = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0;
        int floor = DungeonGenerator.Instance != null ? DungeonGenerator.Instance.GetFloor() : 1;

        statsText.text =
            "<color=#4CAF50>HP:</color>           " + p.currentHealth + " / " + p.maxHealth + "\n" +
            "<color=#2196F3>Energy:</color>      " + Mathf.CeilToInt(p.currentEnergy) + " / " + p.maxEnergy + "\n" +
            "<color=#64B5F6>Armor:</color>       " + p.armor + " / " + p.maxArmor + "\n" +
            "\n" +
            "<color=#FF9800>Damage:</color>      " + p.attackDamage + "\n" +
            "<color=#FFC107>Fire Rate:</color>   " + p.attackCooldown.ToString("F2") + "s\n" +
            "<color=#FFEB3B>Projectiles:</color> " + p.projectilesPerShot + "\n" +
            "<color=#FFF176>Energy Cost:</color> " + p.energyCost + "\n" +
            "<color=#FFF9C4>Speed:</color>        " + p.moveSpeed.ToString("F1") + "\n" +
            "\n" +
            "<color=#CE93D8>Weapon:</color>      " + weaponName + "\n" +
            "<color=#BA68C8>Arsenal:</color>     " + weaponCount + " weapons\n" +
            "\n" +
            "<color=#90A4AE>Floor:</color>       " + floor + "\n" +
            "<color=#B0BEC5>Score:</color>      " + score;
    }
}
