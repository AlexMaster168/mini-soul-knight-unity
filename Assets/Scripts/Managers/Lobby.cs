using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ================= Мета-прогресс между забегами (PlayerPrefs) =================

public class CharacterDef
{
    public string id, description, startWeapon, startRobot;
    public string kind;      // силуэт (Knight / Rogue / Mage / Tank / Engineer)
    public bool female;
    public int price, maxHealth, maxEnergy, startArmor;
    public float moveSpeed, energyRegen, dashCooldown, damageMult, damageReduction;

    public CharacterDef(string id, int price, int hp, int energy, int armor, float speed, float regen, float dash,
                        float dmg, float reduction, string weapon, string robot, string description)
    {
        this.id = id; this.kind = id; this.price = price; maxHealth = hp; maxEnergy = energy; startArmor = armor; moveSpeed = speed;
        energyRegen = regen; dashCooldown = dash; damageMult = dmg; damageReduction = reduction;
        startWeapon = weapon; startRobot = robot; this.description = description;
    }
}

public class SkinDef
{
    public string id;
    public int price;
    public Color main, accent;

    public SkinDef(string id, int price, Color main, Color accent)
    {
        this.id = id; this.price = price; this.main = main; this.accent = accent;
    }
}

public static class MetaProgress
{
    const string KCrystals = "meta_crystals", KChars = "meta_chars", KSkins = "meta_skins",
                 KSelChar = "meta_char", KSelSkin = "meta_skin", KInit = "meta_init";

    public static readonly CharacterDef[] Characters =
    {
        new CharacterDef("Knight", 0, 150, 200, 0, 8f, 5f, 1f, 1f, 0f, "Pistol", null,
            "Balanced hero. Sturdy, reliable, no tricks."),
        new CharacterDef("Rogue", 400, 110, 180, 0, 10f, 5f, 0.6f, 1f, 0f, "Dagger", null,
            "Fast and slippery: quick dash, high speed, starts with a dagger."),
        new CharacterDef("Mage", 600, 100, 320, 0, 8f, 9f, 1f, 1.15f, 0f, "Star Wand", null,
            "Huge energy pool with fast regeneration and +15% damage. Starts with a Star Wand."),
        new CharacterDef("Tank", 700, 260, 200, 60, 6.6f, 5f, 1.2f, 1f, 0.1f, "Shotgun", null,
            "Slow but very tough: 260 HP, 60 armor, -10% damage taken. Starts with a Shotgun."),
        new CharacterDef("Engineer", 900, 130, 220, 0, 8f, 6f, 1f, 1f, 0f, "Laser", "Scout Bot",
            "Starts every run with a Scout Bot companion and a Laser."),

        // женские герои
        new CharacterDef("Valkyrie", 300, 140, 210, 0, 8.4f, 5f, 0.9f, 1.05f, 0f, "Spear", null,
            "Warrior maiden: a bit faster and starts with a long-reach Spear.") { kind = "Knight", female = true },
        new CharacterDef("Assassin", 500, 105, 190, 0, 10.5f, 5f, 0.55f, 1.1f, 0f, "Katana", null,
            "Deadly and swift: the fastest dash in the game, +10% damage, starts with a Katana.") { kind = "Rogue", female = true },
        new CharacterDef("Sorceress", 700, 95, 340, 0, 8f, 10f, 1f, 1.2f, 0f, "FairyGun", null,
            "Enormous energy pool, very fast regeneration, +20% damage. Starts with a FairyGun.") { kind = "Mage", female = true },
        new CharacterDef("Berserker", 800, 240, 200, 40, 7.2f, 5f, 1.1f, 1.1f, 0.08f, "Axe", null,
            "Tough fighter: 240 HP, 40 armor, -8% damage taken, +10% damage. Starts with an Axe.") { kind = "Tank", female = true },
        new CharacterDef("Mechanic", 1000, 125, 240, 0, 8f, 6f, 1f, 1f, 0f, "Plasma", "Gunner Bot",
            "Starts every run with a rapid-fire Gunner Bot and a Plasma gun.") { kind = "Engineer", female = true },
    };

    public static readonly SkinDef[] Skins =
    {
        new SkinDef("Classic", 0, new Color(0.2f, 0.4f, 0.9f), new Color(0.15f, 0.3f, 0.7f)),
        new SkinDef("Crimson", 80, new Color(0.85f, 0.2f, 0.2f), new Color(0.55f, 0.1f, 0.15f)),
        new SkinDef("Emerald", 100, new Color(0.2f, 0.7f, 0.35f), new Color(0.1f, 0.45f, 0.2f)),
        new SkinDef("Shadow", 150, new Color(0.25f, 0.2f, 0.35f), new Color(0.12f, 0.1f, 0.2f)),
        new SkinDef("Frost", 150, new Color(0.6f, 0.85f, 1f), new Color(0.35f, 0.6f, 0.85f)),
        new SkinDef("Sunset", 200, new Color(0.95f, 0.55f, 0.2f), new Color(0.75f, 0.25f, 0.35f)),
        new SkinDef("Neon", 250, new Color(0.2f, 0.95f, 0.6f), new Color(0.85f, 0.2f, 0.9f)),
        new SkinDef("Golden", 300, new Color(1f, 0.8f, 0.25f), new Color(0.75f, 0.55f, 0.1f)),
    };

    private static bool banked;

    public static CharacterDef GetChar(string id)
    {
        foreach (CharacterDef c in Characters) if (c.id == id) return c;
        return Characters[0];
    }

    public static SkinDef GetSkin(string id)
    {
        foreach (SkinDef s in Skins) if (s.id == id) return s;
        return Skins[0];
    }

    public static void Init()
    {
        banked = false;
        if (PlayerPrefs.GetInt(KInit, 0) == 0)
        {
            PlayerPrefs.SetInt(KInit, 1);
            PlayerPrefs.SetInt(KCrystals, 0);
        }
        // большой стартовый подарок: хватает почти на всех героев и скины (выдаётся один раз)
        if (PlayerPrefs.GetInt("meta_gift2", 0) == 0)
        {
            PlayerPrefs.SetInt("meta_gift2", 1);
            PlayerPrefs.SetInt(KCrystals, PlayerPrefs.GetInt(KCrystals, 0) + 10000);
            PlayerPrefs.Save();
        }
    }

    public static int Crystals { get { return PlayerPrefs.GetInt(KCrystals, 0); } }

    static HashSet<string> Load(string key, string first)
    {
        HashSet<string> set = new HashSet<string>(PlayerPrefs.GetString(key, "").Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries));
        set.Add(first);
        return set;
    }

    static void Save(string key, HashSet<string> set)
    {
        PlayerPrefs.SetString(key, string.Join(",", new List<string>(set).ToArray()));
        PlayerPrefs.Save();
    }

    public static bool OwnsChar(string id) { return Load(KChars, "Knight").Contains(id); }
    public static bool OwnsSkin(string id) { return Load(KSkins, "Classic").Contains(id); }

    public static string SelectedChar
    {
        get { string c = PlayerPrefs.GetString(KSelChar, "Knight"); return OwnsChar(c) ? c : "Knight"; }
        set { PlayerPrefs.SetString(KSelChar, value); PlayerPrefs.Save(); }
    }

    public static string SelectedSkin
    {
        get { string c = PlayerPrefs.GetString(KSelSkin, "Classic"); return OwnsSkin(c) ? c : "Classic"; }
        set { PlayerPrefs.SetString(KSelSkin, value); PlayerPrefs.Save(); }
    }

    static bool Spend(int amount)
    {
        if (Crystals < amount) return false;
        PlayerPrefs.SetInt(KCrystals, Crystals - amount);
        return true;
    }

    public static bool BuyChar(string id)
    {
        CharacterDef c = GetChar(id);
        if (OwnsChar(id) || !Spend(c.price)) return false;
        HashSet<string> set = Load(KChars, "Knight");
        set.Add(id);
        Save(KChars, set);
        return true;
    }

    public static bool BuySkin(string id)
    {
        SkinDef s = GetSkin(id);
        if (OwnsSkin(id) || !Spend(s.price)) return false;
        HashSet<string> set = Load(KSkins, "Classic");
        set.Add(id);
        Save(KSkins, set);
        return true;
    }

    // Награда за забег: кристаллы за глубину, остаток золота и победу
    public static int BankRun(bool win)
    {
        if (banked) return 0;
        banked = true;
        int floor = DungeonGenerator.Instance != null ? DungeonGenerator.Instance.GetFloor() : 1;
        int gold = ScoreManager.Instance != null ? ScoreManager.Instance.GetGold() : 0;
        int reward = floor * 40 + gold / 4 + (win ? 300 : 0);
        PlayerPrefs.SetInt(KCrystals, Crystals + reward);
        PlayerPrefs.Save();
        return reward;
    }

    // Внешний вид игрока: силуэт персонажа + цвета скина
    public static void ApplyLook(SpriteRenderer sr, string charId, string skinId)
    {
        if (sr == null) return;
        SkinDef skin = GetSkin(skinId);
        CharacterDef ch = GetChar(charId);
        sr.sprite = PixelArt.Hero(ch.kind, skin.main, skin.accent, ch.female);
    }

    public static void ApplyLook(SpriteRenderer sr)
    {
        ApplyLook(sr, SelectedChar, SelectedSkin);
    }

    // Статы выбранного персонажа - в начале забега
    public static void ApplyCharacter(PlayerController pc)
    {
        if (pc == null) return;
        CharacterDef c = GetChar(SelectedChar);
        pc.maxHealth = c.maxHealth;
        pc.currentHealth = c.maxHealth;
        pc.maxEnergy = c.maxEnergy;
        pc.currentEnergy = c.maxEnergy;
        pc.energyRegenRate = c.energyRegen;
        pc.moveSpeed = c.moveSpeed;
        pc.dashCooldown = c.dashCooldown;
        pc.damageMultiplier = c.damageMult;
        pc.damageReduction = c.damageReduction;
        pc.maxArmor = Mathf.Max(100, c.startArmor + 40);
        pc.armor = c.startArmor;

        Inventory inv = pc.GetComponent<Inventory>();
        if (inv != null && GameData.Weapons.ContainsKey(c.startWeapon))
        {
            inv.weapons[0] = c.startWeapon;
            inv.EquipWeapon(0);
        }
        if (c.startRobot != null && PlayerRobots.Instance != null)
            PlayerRobots.Instance.Add(c.startRobot);
    }
}

// ================= Лобби: комната перед забегом =================

public static class LobbyController
{
    public static GameObject root;
    public static bool Active { get { return root != null; } }

    public static void Build()
    {
        MetaProgress.Init();
        root = new GameObject("Lobby");

        const int W = 14, H = 10;
        Sprite floor = SpriteGenerator.CreateTile(32, new Color(0.3f, 0.28f, 0.38f), new Color(0.22f, 0.2f, 0.3f));
        Sprite wall = SpriteGenerator.CreateWall(32, new Color(0.5f, 0.42f, 0.62f));

        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                GameObject t = new GameObject("Tile");
                t.transform.SetParent(root.transform);
                t.transform.position = new Vector3(x - W / 2f + 0.5f, y - H / 2f + 0.5f, 0);
                t.AddComponent<SpriteRenderer>().sprite = floor;
            }

        for (int x = -1; x <= W; x++)
        {
            Wall(wall, new Vector3(x - W / 2f + 0.5f, H / 2f + 0.5f, 0));
            Wall(wall, new Vector3(x - W / 2f + 0.5f, -H / 2f - 0.5f, 0));
        }
        for (int y = 0; y < H; y++)
        {
            Wall(wall, new Vector3(-W / 2f - 0.5f, y - H / 2f + 0.5f, 0));
            Wall(wall, new Vector3(W / 2f + 0.5f, y - H / 2f + 0.5f, 0));
        }

        Torch(new Vector3(-6f, 4f, 0)); Torch(new Vector3(6f, 4f, 0));
        Torch(new Vector3(-6f, -4f, 0)); Torch(new Vector3(6f, -4f, 0));

        // Hero Master - выбор героев и скинов
        GameObject npc = new GameObject("HeroMaster");
        npc.transform.SetParent(root.transform);
        npc.transform.position = new Vector3(-3.5f, 2f, 0);
        npc.transform.localScale = Vector3.one * 1.3f;
        SpriteRenderer nsr = npc.AddComponent<SpriteRenderer>();
        nsr.sprite = PixelArt.Hero("Mage", new Color(0.55f, 0.25f, 0.8f), new Color(0.9f, 0.75f, 0.2f));
        nsr.sortingOrder = 10;
        npc.AddComponent<LobbyNpc>();
        Glow(npc.transform.position, new Color(0.8f, 0.5f, 1f, 0.22f), 4f);

        // портал на старт забега
        GameObject portal = new GameObject("RunPortal");
        portal.transform.SetParent(root.transform);
        portal.transform.position = new Vector3(4f, 2f, 0);
        SpriteRenderer psr = portal.AddComponent<SpriteRenderer>();
        psr.sprite = PixelArt.Portal();
        psr.sortingOrder = 6;
        portal.AddComponent<RunPortal>();

        PlayerController pc = PlayerController.Instance;
        if (pc != null)
        {
            pc.transform.position = new Vector3(0, -2f, 0);
            MetaProgress.ApplyLook(pc.GetComponent<SpriteRenderer>());
        }

        if (Camera.main != null)
        {
            Camera.main.backgroundColor = new Color(0.07f, 0.06f, 0.11f);
            Camera.main.transform.position = new Vector3(0, 0, -10);
            CameraFollow cf = Camera.main.GetComponent<CameraFollow>();
            if (cf != null) { cf.useFocus = true; cf.focus = Vector2.zero; }
        }

        FloorTransition.Get().Banner("LOBBY", "Pick your hero, then enter the portal", 3f);
    }

    static void Wall(Sprite sprite, Vector3 pos)
    {
        GameObject w = new GameObject("Wall");
        w.tag = "Wall";
        w.transform.SetParent(root.transform);
        w.transform.position = pos;
        SpriteRenderer sr = w.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.65f, 0.55f, 0.75f);
        sr.sortingOrder = 1;
        w.AddComponent<BoxCollider2D>().size = Vector2.one;
    }

    static void Glow(Vector3 pos, Color color, float size)
    {
        GameObject g = new GameObject("Glow");
        g.transform.SetParent(root.transform);
        g.transform.position = pos;
        g.transform.localScale = Vector3.one * size;
        SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(32, color);
        sr.sortingOrder = 1;
    }

    static void Torch(Vector3 pos)
    {
        GameObject f = new GameObject("Torch");
        f.transform.SetParent(root.transform);
        f.transform.position = pos;
        f.transform.localScale = Vector3.one * 0.5f;
        SpriteRenderer sr = f.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(12, new Color(0.7f, 0.5f, 1f, 0.9f));
        sr.sortingOrder = 4;
        TorchFlicker tf = f.AddComponent<TorchFlicker>();
        tf.tint = new Color(0.7f, 0.5f, 1f, 0.9f);
        Glow(pos, new Color(0.7f, 0.5f, 1f, 0.2f), 4f);
    }

    public static void Leave()
    {
        if (root != null) Object.Destroy(root);
        root = null;
    }
}

public class LobbyNpc : MonoBehaviour
{
    private Vector3 basePos;
    private float t;

    void Start() { basePos = transform.position; }

    void Update()
    {
        t += Time.deltaTime;
        transform.position = basePos + Vector3.up * Mathf.Sin(t * 2f) * 0.06f;

        PlayerController pc = PlayerController.Instance;
        if (pc == null || LobbyUI.Instance == null || LobbyUI.Instance.IsOpen()) return;

        if (Vector2.Distance(basePos, pc.transform.position) < 2.6f)
        {
            HudHint.Show("[E] Hero Master - heroes & skins");
            if (Input.GetKeyDown(KeyCode.E) && Time.frameCount != LobbyUI.Instance.ClosedFrame)
                LobbyUI.Instance.Open();
        }
    }
}

public class RunPortal : MonoBehaviour
{
    private bool used;
    private float t;

    void Update()
    {
        t += Time.deltaTime;
        transform.Rotate(0, 0, 60f * Time.deltaTime);
        transform.localScale = Vector3.one * (2.6f + Mathf.Sin(t * 3f) * 0.15f);

        PlayerController pc = PlayerController.Instance;
        if (pc == null || used) return;
        float d = Vector2.Distance(pc.transform.position, transform.position);
        if (d < 4f && (LobbyUI.Instance == null || !LobbyUI.Instance.IsOpen()))
            HudHint.Show("Step into the portal to START THE RUN");
        if (d < 1.3f)
        {
            used = true;
            DungeonGenerator.Instance.StartRun();
        }
    }
}

// Меню героев и скинов за кристаллы
public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance;

    private GameObject panel;
    private Text crystalHud, title, crystals, detailTitle, detailSub, detailBody, detailBuy, message, keys;
    private Image detailIcon, detailIcon2;
    private const int Rows = 10;
    private readonly Image[] rowBg = new Image[Rows];
    private readonly Image[] rowIcon = new Image[Rows];
    private readonly Image[] rowIcon2 = new Image[Rows];
    private readonly Text[] rowName = new Text[Rows];
    private readonly Text[] rowStatus = new Text[Rows];

    private bool isOpen;
    private int mode;   // 0 - герои, 1 - скины
    private int index;
    private int openedFrame;
    private float messageTimer;
    public int ClosedFrame { get; private set; }

    void Awake() { Instance = this; }
    public bool IsOpen() { return isOpen; }

    void Start()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 96;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        crystalHud = UiKit.MakeText(transform, "CrystalHud", 26, new Color(0.6f, 0.9f, 1f), TextAnchor.UpperLeft, 24, 20, 500, 40);
        crystalHud.fontStyle = FontStyle.Bold;

        panel = new GameObject("LobbyPanel");
        panel.transform.SetParent(transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.05f, 0.11f, 0.97f);
        RectTransform pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.pivot = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(1000, 640);

        title = UiKit.MakeText(panel.transform, "Title", 24, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleLeft, 25, 12, 700, 44);
        title.fontStyle = FontStyle.Bold;
        crystals = UiKit.MakeText(panel.transform, "Crystals", 24, new Color(0.6f, 0.9f, 1f), TextAnchor.MiddleRight, 700, 12, 275, 44);
        crystals.fontStyle = FontStyle.Bold;

        for (int i = 0; i < Rows; i++)
        {
            rowBg[i] = UiKit.MakeImage(panel.transform, "Row" + i, new Color(0.12f, 0.1f, 0.2f, 0.9f), 25, 64 + i * 50, 470, 46);
            rowIcon[i] = UiKit.MakeImage(rowBg[i].transform, "Icon", Color.white, 6, 0, 46, 46);
            rowIcon[i].preserveAspect = true;
            rowIcon2[i] = UiKit.MakeImage(rowBg[i].transform, "Icon2", Color.white, 52, 0, 46, 46);
            rowIcon2[i].preserveAspect = true;
            rowName[i] = UiKit.MakeText(rowBg[i].transform, "Name", 22, Color.white, TextAnchor.MiddleLeft, 62, 0, 260, 46);
            rowStatus[i] = UiKit.MakeText(rowBg[i].transform, "Status", 20, Color.white, TextAnchor.MiddleRight, 320, 0, 140, 46);
            rowStatus[i].fontStyle = FontStyle.Bold;
        }

        UiKit.MakeImage(panel.transform, "Divider", new Color(1f, 1f, 1f, 0.12f), 515, 64, 2, 500);
        detailIcon = UiKit.MakeImage(panel.transform, "Preview", Color.white, 640, 68, 220, 220);
        detailIcon.preserveAspect = true;
        detailIcon2 = UiKit.MakeImage(panel.transform, "Preview2", Color.white, 760, 68, 220, 220);
        detailIcon2.preserveAspect = true;
        detailTitle = UiKit.MakeText(panel.transform, "DTitle", 28, Color.white, TextAnchor.UpperLeft, 535, 296, 440, 36);
        detailTitle.fontStyle = FontStyle.Bold;
        detailSub = UiKit.MakeText(panel.transform, "DSub", 17, Color.gray, TextAnchor.UpperLeft, 535, 332, 440, 24);
        detailBody = UiKit.MakeText(panel.transform, "DBody", 18, Color.white, TextAnchor.UpperLeft, 535, 362, 445, 170);
        detailBuy = UiKit.MakeText(panel.transform, "DBuy", 22, new Color(1f, 0.9f, 0.4f), TextAnchor.MiddleLeft, 535, 500, 445, 46);
        detailBuy.fontStyle = FontStyle.Bold;

        message = UiKit.MakeText(panel.transform, "Msg", 20, new Color(1f, 0.5f, 0.4f), TextAnchor.MiddleCenter, 25, 570, 950, 30);
        message.fontStyle = FontStyle.Bold;
        keys = UiKit.MakeText(panel.transform, "Keys", 16, new Color(1f, 1f, 1f, 0.6f), TextAnchor.MiddleCenter, 25, 600, 950, 28);
        keys.text = "Up/Down select  |  T switch heroes / skins  |  Enter buy or equip  |  E close";
        panel.SetActive(false);
    }

    public void Open()
    {
        if (isOpen || panel == null) return;
        isOpen = true;
        openedFrame = Time.frameCount;
        panel.SetActive(true);
        message.text = "";
        index = 0;
        Refresh();
    }

    void Close()
    {
        isOpen = false;
        ClosedFrame = Time.frameCount;
        panel.SetActive(false);
        // вернуть внешний вид выбранного героя
        PlayerController pc = PlayerController.Instance;
        if (pc != null) MetaProgress.ApplyLook(pc.GetComponent<SpriteRenderer>());
    }

    int Count { get { return mode == 0 ? MetaProgress.Characters.Length : MetaProgress.Skins.Length; } }

    string IdAt(int i) { return mode == 0 ? MetaProgress.Characters[i].id : MetaProgress.Skins[i].id; }

    bool Owned(int i) { return mode == 0 ? MetaProgress.OwnsChar(IdAt(i)) : MetaProgress.OwnsSkin(IdAt(i)); }

    bool Equipped(int i) { return mode == 0 ? MetaProgress.SelectedChar == IdAt(i) : MetaProgress.SelectedSkin == IdAt(i); }

    int PriceAt(int i) { return mode == 0 ? MetaProgress.Characters[i].price : MetaProgress.Skins[i].price; }

    // спрайт-превью: для героя - его силуэт в выбранном скине, для скина - выбранный герой в этом скине
    Sprite Preview(int i)
    {
        if (mode == 0)
        {
            SkinDef sk = MetaProgress.GetSkin(MetaProgress.SelectedSkin);
            CharacterDef cd = MetaProgress.Characters[i];
            return PixelArt.Hero(cd.kind, sk.main, sk.accent, cd.female);
        }
        SkinDef s = MetaProgress.Skins[i];
        CharacterDef cur = MetaProgress.GetChar(MetaProgress.SelectedChar);
        return PixelArt.Hero(cur.kind, s.main, s.accent, cur.female);
    }

    // во вкладке скинов показываем сразу парня и девушку в этом скине
    Sprite SkinPreview(int i, bool girl)
    {
        SkinDef s = MetaProgress.Skins[i];
        CharacterDef cur = MetaProgress.GetChar(MetaProgress.SelectedChar);
        return PixelArt.Hero(cur.kind, s.main, s.accent, girl);
    }

    void Refresh()
    {
        bool skins = mode == 1;
        RectTransform d1 = detailIcon.rectTransform, d2 = detailIcon2.rectTransform;
        d1.anchoredPosition = new Vector2(skins ? 540 : 640, -68);
        d1.sizeDelta = skins ? new Vector2(200, 200) : new Vector2(220, 220);
        d2.anchoredPosition = new Vector2(760, -68);
        d2.sizeDelta = new Vector2(200, 200);
        detailIcon2.enabled = skins;
        title.text = "HERO MASTER     " + (mode == 0 ? "[ Heroes ]   Skins" : "Heroes   [ Skins ]") + "   (T)";
        crystals.text = "Crystals: " + MetaProgress.Crystals;

        for (int i = 0; i < Rows; i++)
        {
            bool exists = i < Count;
            rowBg[i].gameObject.SetActive(exists);
            if (!exists) continue;

            rowBg[i].color = i == index ? new Color(0.3f, 0.22f, 0.5f, 1f) : new Color(0.12f, 0.1f, 0.2f, 0.9f);
            rowIcon[i].sprite = skins ? SkinPreview(i, false) : Preview(i);
            rowIcon2[i].enabled = skins;
            if (skins) rowIcon2[i].sprite = SkinPreview(i, true);
            rowName[i].rectTransform.anchoredPosition = new Vector2(skins ? 104 : 62, 0);
            rowName[i].text = IdAt(i);
            bool owned = Owned(i);
            if (Equipped(i)) { rowStatus[i].text = "EQUIPPED"; rowStatus[i].color = new Color(0.5f, 1f, 0.5f); }
            else if (owned) { rowStatus[i].text = "OWNED"; rowStatus[i].color = Color.gray; }
            else
            {
                rowStatus[i].text = PriceAt(i) + " C";
                rowStatus[i].color = MetaProgress.Crystals >= PriceAt(i) ? new Color(0.5f, 0.9f, 1f) : new Color(1f, 0.45f, 0.45f);
            }
        }

        detailIcon.sprite = skins ? SkinPreview(index, false) : Preview(index);
        if (skins) detailIcon2.sprite = SkinPreview(index, true);
        detailTitle.text = IdAt(index);
        if (mode == 0)
        {
            CharacterDef c = MetaProgress.Characters[index];
            detailSub.text = c.female ? "Hero  -  female" : "Hero  -  male";
            string extra = "";
            if (c.startArmor > 0) extra += "Start armor: " + c.startArmor + "\n";
            if (c.damageReduction > 0f) extra += "Damage taken: -" + Mathf.RoundToInt(c.damageReduction * 100f) + "%\n";
            if (c.damageMult > 1f) extra += "Damage: +" + Mathf.RoundToInt((c.damageMult - 1f) * 100f) + "%\n";
            if (c.startRobot != null) extra += "Companion: " + c.startRobot + "\n";
            detailBody.text = c.description + "\n\nHP " + c.maxHealth + "   Energy " + c.maxEnergy + "   Speed " + c.moveSpeed.ToString("F1") +
                              "\nEnergy regen " + c.energyRegen.ToString("F0") + "/s   Dash cooldown " + c.dashCooldown.ToString("F1") + "s\n" +
                              extra + "Starting weapon: " + c.startWeapon;
        }
        else
        {
            detailSub.text = "Skin - works on every hero";
            detailBody.text = "Changes the colors of your hero.";
        }

        if (Equipped(index)) detailBuy.text = "Equipped";
        else if (Owned(index)) detailBuy.text = "Press Enter to equip";
        else detailBuy.text = "Price: " + PriceAt(index) + " crystals   (Enter to buy)";
    }

    void Say(string text, Color c)
    {
        message.text = text;
        message.color = c;
        messageTimer = 2f;
    }

    void Activate()
    {
        string id = IdAt(index);
        if (!Owned(index))
        {
            bool ok = mode == 0 ? MetaProgress.BuyChar(id) : MetaProgress.BuySkin(id);
            if (!ok) { Say("Not enough crystals (earn them by playing runs)", new Color(1f, 0.5f, 0.4f)); return; }
            Say("Unlocked " + id + "!", new Color(0.5f, 1f, 0.5f));
        }
        if (mode == 0) MetaProgress.SelectedChar = id; else MetaProgress.SelectedSkin = id;
        PlayerController pc = PlayerController.Instance;
        if (pc != null) MetaProgress.ApplyLook(pc.GetComponent<SpriteRenderer>());
    }

    void Update()
    {
        if (crystalHud != null)
            crystalHud.text = LobbyController.Active && !isOpen ? "Crystals: " + MetaProgress.Crystals : "";

        if (!isOpen) return;

        if (messageTimer > 0f)
        {
            messageTimer -= Time.unscaledDeltaTime;
            if (messageTimer <= 0f) message.text = "";
        }

        if (Input.GetKeyDown(KeyCode.T)) { mode = 1 - mode; index = 0; Refresh(); }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) { index = (index - 1 + Count) % Count; Refresh(); }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) { index = (index + 1) % Count; Refresh(); }
        else if (Input.GetKeyDown(KeyCode.Return)) { Activate(); Refresh(); }
        else if ((Input.GetKeyDown(KeyCode.E) && Time.frameCount != openedFrame) || Input.GetKeyDown(KeyCode.Escape)) Close();
    }
}
