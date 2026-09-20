using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Магазин оружия у торговца: весь каталог, постранично, с описанием справа
public class WeaponShopUI : MonoBehaviour
{
    public static WeaponShopUI Instance;

    const int RowsPerPage = 10;

    private GameObject panel;
    private readonly Image[] rowBg = new Image[RowsPerPage];
    private readonly Image[] rowIcon = new Image[RowsPerPage];
    private readonly Text[] rowName = new Text[RowsPerPage];
    private readonly Text[] rowPrice = new Text[RowsPerPage];
    private Image detailIcon;
    private Text detailTitle, detailSub, detailBody, detailBuy, goldText, pageText, message;

    private List<string> catalog = new List<string>();
    private List<string> abilityList = new List<string>();
    private List<string> upgradeList = new List<string>();
    private List<string> robotList = new List<string>();
    private int mode; // 0 - оружие, 1 - супер-способности, 2 - улучшения, 3 - роботы
    const int ModeCount = 5;
    static readonly string[] modeNames = { "Weapons", "Super Abilities", "Upgrades", "Robots", "Supplies" };

    // Припасы: восстановление брони / здоровья / энергии, цена за единицу, количество задаётся ползунком
    private List<string> supplyList = new List<string> { "Armor", "Health", "Energy" };
    private int supplyPercent = 100;
    static int UnitPrice(string id) { return id == "Armor" ? 3 : id == "Health" ? 2 : 1; }
    private Text titleText;
    private List<string> Cur { get { return mode == 0 ? catalog : mode == 1 ? abilityList : mode == 2 ? upgradeList : mode == 3 ? robotList : supplyList; } }
    private int index;
    private bool isOpen;
    private int openedFrame;
    public int ClosedFrame { get; private set; }
    private float messageTimer;

    void Awake()
    {
        Instance = this;
    }

    public bool IsOpen() { return isOpen; }

    void Start()
    {
        foreach (string w in GameData.LootableWeapons()) catalog.Add(w);
        catalog.Sort((a, b) =>
        {
            int c = WeaponInfo.Price(a).CompareTo(WeaponInfo.Price(b));
            return c != 0 ? c : string.CompareOrdinal(a, b);
        });
        foreach (AbilityDef a in AbilityCatalog.All) abilityList.Add(a.id);
        foreach (UpgradeDef u in UpgradeCatalog.All) upgradeList.Add(u.id);
        foreach (RobotDef r in RobotCatalog.All) robotList.Add(r.id);
        Build();
    }

    void Build()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 95;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        panel = new GameObject("WeaponShopPanel");
        panel.transform.SetParent(transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.04f, 0.1f, 0.97f);
        RectTransform pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.pivot = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(1100, 660);

        titleText = UiKit.MakeText(panel.transform, "Title", 22, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleLeft, 25, 12, 1050, 44);
        titleText.fontStyle = FontStyle.Bold;
        goldText = UiKit.MakeText(panel.transform, "Gold", 24, new Color(1f, 0.85f, 0.1f), TextAnchor.MiddleRight, 560, 12, 515, 44);

        for (int i = 0; i < RowsPerPage; i++)
        {
            float y = 68 + i * 50;
            rowBg[i] = UiKit.MakeImage(panel.transform, "Row" + i, new Color(0.12f, 0.1f, 0.2f, 0.9f), 25, y, 530, 46);
            rowIcon[i] = UiKit.MakeImage(rowBg[i].transform, "Icon", Color.white, 8, 4, 60, 38);
            rowIcon[i].preserveAspect = true;
            rowName[i] = UiKit.MakeText(rowBg[i].transform, "Name", 20, Color.white, TextAnchor.MiddleLeft, 80, 0, 300, 46);
            rowPrice[i] = UiKit.MakeText(rowBg[i].transform, "Price", 20, Color.white, TextAnchor.MiddleRight, 390, 0, 130, 46);
            rowPrice[i].fontStyle = FontStyle.Bold;
        }

        UiKit.MakeImage(panel.transform, "Divider", new Color(1f, 1f, 1f, 0.12f), 575, 68, 2, 500);

        detailIcon = UiKit.MakeImage(panel.transform, "DetailIcon", Color.white, 830, 68, 220, 110);
        detailIcon.preserveAspect = true;
        detailTitle = UiKit.MakeText(panel.transform, "DetailTitle", 30, Color.white, TextAnchor.UpperLeft, 595, 68, 230, 40);
        detailTitle.fontStyle = FontStyle.Bold;
        detailSub = UiKit.MakeText(panel.transform, "DetailSub", 18, Color.gray, TextAnchor.UpperLeft, 595, 110, 230, 26);
        detailBody = UiKit.MakeText(panel.transform, "DetailBody", 18, Color.white, TextAnchor.UpperLeft, 595, 190, 480, 300);
        detailBuy = UiKit.MakeText(panel.transform, "DetailBuy", 22, new Color(1f, 0.9f, 0.4f), TextAnchor.MiddleLeft, 595, 500, 480, 66);
        detailBuy.fontStyle = FontStyle.Bold;

        message = UiKit.MakeText(panel.transform, "Message", 20, new Color(1f, 0.5f, 0.4f), TextAnchor.MiddleCenter, 25, 574, 1050, 30);
        message.fontStyle = FontStyle.Bold;
        pageText = UiKit.MakeText(panel.transform, "Page", 16, new Color(1f, 1f, 1f, 0.6f), TextAnchor.MiddleLeft, 25, 618, 300, 28);
        UiKit.MakeText(panel.transform, "Keys", 16, new Color(1f, 1f, 1f, 0.6f), TextAnchor.MiddleRight, 325, 618, 750, 28).text =
            "Up/Down select  |  Left/Right page/amount  |  T next tab  |  Enter buy  |  F buy max  |  E close";

        // золото - отдельным текстом справа
        goldText.text = "";
        panel.SetActive(false);
    }

    public void Open()
    {
        if (isOpen || panel == null) return;
        isOpen = true;
        openedFrame = Time.frameCount;
        panel.SetActive(true);
        message.text = "";
        Refresh();
        if (ProceduralMusic.Instance != null)
            ProceduralMusic.Instance.PlaySFX("shopOpen");
    }

    public void Close()
    {
        isOpen = false;
        ClosedFrame = Time.frameCount;
        if (panel != null) panel.SetActive(false);
    }

    // Описание позиции для вкладок кроме оружия
    class Entry
    {
        public string sub, body, status, note;
        public int price;
        public Color color;
    }

    Entry Describe(int m, string id)
    {
        Entry en = new Entry();
        if (m == 1)
        {
            AbilityDef d = AbilityCatalog.Get(id);
            PlayerAbilities ab = PlayerAbilities.Instance;
            en.color = d.color; en.price = d.price;
            en.sub = "Super Ability  -  cooldown " + d.cooldown + "s";
            en.body = d.description + "\n\nActivate with Z / X / C (in order of purchase). You can carry up to " + PlayerAbilities.MaxSlots + " abilities.";
            if (ab != null && ab.Owns(id)) en.status = "OWNED";
            else if (ab != null && ab.IsFull) en.note = "Ability slots are full";
        }
        else if (m == 2)
        {
            UpgradeDef u = UpgradeCatalog.Get(id);
            int lvl = PlayerUpgrades.Instance != null ? PlayerUpgrades.Instance.Level(id) : 0;
            en.color = u.color;
            en.price = u.Price(lvl);
            en.sub = "Permanent upgrade  -  level " + lvl + " / " + u.maxLevel;
            en.body = u.description + "\n\nLasts for the whole run.";
            if (lvl >= u.maxLevel) en.status = "MAX";
        }
        else if (m == 4)
        {
            int missing = SupplyMissing(id);
            int pts = Mathf.CeilToInt(missing * supplyPercent / 100f);
            en.color = id == "Armor" ? new Color(0.4f, 0.7f, 1f) : id == "Health" ? new Color(0.35f, 0.9f, 0.4f) : new Color(0.3f, 0.6f, 1f);
            en.price = pts * UnitPrice(id);
            en.sub = "Supply  -  " + UnitPrice(id) + " G per point";
            int filled = supplyPercent / 10;
            string bar = new string('#', filled) + new string('-', 10 - filled);
            en.body = "Restores " + id.ToLower() + ".\n\nAmount:  [" + bar + "]  " + supplyPercent + "%\n" +
                      "Points: " + pts + " of " + missing + " missing\nCost: " + en.price + " G\n\n" +
                      "Left / Right - change amount\nF - buy everything at once (100%)";
            if (missing <= 0) en.status = "FULL";
        }
        else
        {
            RobotDef r = RobotCatalog.Get(id);
            en.color = r.color; en.price = r.price;
            en.sub = "Combat robot  -  " + r.damage + " dmg every " + r.cooldown.ToString("F2") + "s";
            en.body = r.description + "\n\nFights on its own and follows you between floors.";
            int floor = DungeonGenerator.Instance != null ? DungeonGenerator.Instance.GetFloor() : 1;
            if (PlayerRobots.Instance != null && PlayerRobots.Instance.Owns(id)) en.status = "OWNED";
            else if (floor < RobotCatalog.MinFloor) { en.status = "FLOOR " + RobotCatalog.MinFloor + "+"; en.note = "Robots are sold from dungeon floor " + RobotCatalog.MinFloor; }
        }
        return en;
    }

    int SupplyMissing(string id)
    {
        PlayerController pc = PlayerController.Instance;
        if (pc == null) return 0;
        if (id == "Armor") return Mathf.Max(0, pc.maxArmor - pc.armor);
        if (id == "Health") return Mathf.Max(0, pc.maxHealth - pc.currentHealth);
        return Mathf.Max(0, pc.maxEnergy - pc.currentEnergy);
    }

    void Refresh()
    {
        List<string> list = Cur;
        if (index >= list.Count) index = 0;
        int page = index / RowsPerPage;
        Inventory inv = Inventory.Instance;
        int gold = ScoreManager.Instance != null ? ScoreManager.Instance.GetGold() : 0;

        string tabs = "";
        for (int i = 0; i < ModeCount; i++)
            tabs += (i == mode ? "[ " + modeNames[i] + " ]" : modeNames[i]) + "     ";
        titleText.text = "MERCHANT   " + tabs + "(T)";

        for (int i = 0; i < RowsPerPage; i++)
        {
            int idx = page * RowsPerPage + i;
            bool exists = idx < list.Count;
            rowBg[i].gameObject.SetActive(exists);
            if (!exists) continue;

            string id = list[idx];
            int price;
            string status = null;
            Color nameColor;
            if (mode == 0)
            {
                price = WeaponInfo.Price(id);
                if (inv != null && inv.weapons.Contains(id)) status = "OWNED";
                nameColor = WeaponInfo.RarityColor(id);
                rowIcon[i].sprite = PixelArt.Weapon(id);
                rowIcon[i].color = Color.white;
            }
            else
            {
                Entry en = Describe(mode, id);
                price = en.price;
                status = en.status;
                nameColor = en.color;
                rowIcon[i].sprite = null;
                rowIcon[i].color = en.color;
            }

            rowBg[i].color = idx == index ? new Color(0.3f, 0.22f, 0.5f, 1f) : new Color(0.12f, 0.1f, 0.2f, 0.9f);
            rowName[i].text = id;
            rowName[i].color = status != null ? Color.gray : nameColor;
            rowPrice[i].text = status ?? price + " G";
            rowPrice[i].color = status != null ? Color.gray : (gold >= price ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.45f, 0.45f));
        }

        string sel = list[index];
        if (mode == 0)
        {
            Color c = WeaponInfo.RarityColor(sel);
            detailIcon.enabled = true;
            detailIcon.sprite = PixelArt.Weapon(sel);
            detailTitle.text = sel;
            detailTitle.color = c;
            detailSub.text = WeaponInfo.RarityName(sel) + "  -  " + WeaponInfo.ClassName(sel);
            detailSub.color = c * 0.85f;
            detailBody.text = WeaponInfo.Full(sel);

            int selPrice = WeaponInfo.Price(sel);
            bool selOwned = inv != null && inv.weapons.Contains(sel);
            if (selOwned) detailBuy.text = "Already in your arsenal";
            else if (inv != null && inv.IsFull) detailBuy.text = "Price: " + selPrice + " G\nSlots full: replaces " + inv.weapons[inv.currentWeaponIndex];
            else detailBuy.text = "Price: " + selPrice + " G";
        }
        else
        {
            Entry en = Describe(mode, sel);
            detailIcon.enabled = false;
            detailTitle.text = sel;
            detailTitle.color = en.color;
            detailSub.text = en.sub;
            detailSub.color = en.color * 0.85f;
            detailBody.text = en.body;
            if (en.status == "OWNED") detailBuy.text = "Already yours";
            else if (en.status == "MAX") detailBuy.text = "Maximum level reached";
            else if (en.status == "FULL") detailBuy.text = "Already full";
            else detailBuy.text = "Price: " + en.price + " G" + (en.note != null ? "\n" + en.note : "");
        }

        goldText.text = "";
        pageText.text = "Gold: " + gold + "     Page " + (page + 1) + "/" + Mathf.CeilToInt(list.Count / (float)RowsPerPage);
        pageText.color = new Color(1f, 0.85f, 0.1f);
    }

    void Buy(bool max = false)
    {
        Inventory inv = Inventory.Instance;
        if (inv == null || ScoreManager.Instance == null) return;
        string id = Cur[index];
        Color good = new Color(0.5f, 1f, 0.5f);

        if (mode == 0)
        {
            if (inv.weapons.Contains(id)) { Say("You already own this weapon"); return; }
            int price = WeaponInfo.Price(id);
            if (!ScoreManager.Instance.SpendGold(price)) { Say("Not enough gold"); return; }
            inv.AddWeapon(id, true);
            Burst(WeaponInfo.RarityColor(id));
            Say("Bought " + id + "!", good);
            return;
        }

        if (mode == 4) { BuySupply(id, max); return; }

        Entry en = Describe(mode, id);
        if (en.status == "OWNED") { Say("You already have this"); return; }
        if (en.status == "MAX") { Say("Already at maximum level"); return; }
        if (en.status != null && en.status.StartsWith("FLOOR")) { Say(en.note); return; }
        if (en.note != null) { Say(en.note); return; }
        if (mode == 2 && max)
        {
            int bought = 0;
            while (true)
            {
                Entry next = Describe(2, id);
                if (next.status == "MAX") break;
                if (!ScoreManager.Instance.SpendGold(next.price)) break;
                PlayerUpgrades.Instance.Apply(id);
                bought++;
            }
            if (bought == 0) { Say("Not enough gold"); return; }
            Say(id + " upgraded +" + bought + " levels!", good);
            Burst(en.color);
            return;
        }

        if (!ScoreManager.Instance.SpendGold(en.price)) { Say("Not enough gold"); return; }

        if (mode == 1)
        {
            PlayerAbilities.Instance.Add(id);
            Say("Learned " + id + "! Press " + PlayerAbilities.Keys[PlayerAbilities.Instance.owned.Count - 1], good);
        }
        else if (mode == 2)
        {
            PlayerUpgrades.Instance.Apply(id);
            Say(id + " upgraded!", good);
        }
        else
        {
            PlayerRobots.Instance.Add(id);
            Say(id + " joins you!", good);
        }
        Burst(en.color);
    }

    void BuySupply(string id, bool max)
    {
        PlayerController pc = PlayerController.Instance;
        if (pc == null) return;
        if (max) supplyPercent = 100;

        int missing = SupplyMissing(id);
        if (missing <= 0) { Say("Already full"); return; }

        int wanted = Mathf.CeilToInt(missing * supplyPercent / 100f);
        int unit = UnitPrice(id);
        int affordable = ScoreManager.Instance.GetGold() / unit;
        int pts = Mathf.Min(wanted, affordable);
        if (pts <= 0) { Say("Not enough gold"); return; }
        if (!ScoreManager.Instance.SpendGold(pts * unit)) { Say("Not enough gold"); return; }

        if (id == "Armor") pc.armor = Mathf.Min(pc.maxArmor, pc.armor + pts);
        else if (id == "Health") pc.currentHealth = Mathf.Min(pc.maxHealth, pc.currentHealth + pts);
        else pc.currentEnergy = Mathf.Min(pc.maxEnergy, pc.currentEnergy + pts);

        Say(id + " +" + pts + (pts < wanted ? "  (all the gold you had)" : ""), new Color(0.5f, 1f, 0.5f));
        Burst(new Color(0.5f, 0.9f, 1f));
    }

    void Burst(Color c)
    {
        if (EffectsManager.Instance != null && PlayerController.Instance != null)
            EffectsManager.Instance.SpawnPickupEffect(PlayerController.Instance.transform.position, c);
    }

    void Say(string text, Color? color = null)
    {
        message.text = text;
        message.color = color ?? new Color(1f, 0.5f, 0.4f);
        messageTimer = 2f;
    }

    void Update()
    {
        if (!isOpen) return;

        if (messageTimer > 0f)
        {
            messageTimer -= Time.unscaledDeltaTime;
            if (messageTimer <= 0f) message.text = "";
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            mode = (mode + 1) % ModeCount;
            index = 0;
            Refresh();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            index = (index - 1 + Cur.Count) % Cur.Count;
            Refresh();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            index = (index + 1) % Cur.Count;
            Refresh();
        }
        else if (mode == 4 && (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)))
        {
            supplyPercent = Mathf.Min(100, supplyPercent + 10);
            Refresh();
        }
        else if (mode == 4 && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)))
        {
            supplyPercent = Mathf.Max(10, supplyPercent - 10);
            Refresh();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            Buy(true);
            Refresh();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            index = Mathf.Min(Cur.Count - 1, (index / RowsPerPage + 1) * RowsPerPage);
            Refresh();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            index = Mathf.Max(0, (index / RowsPerPage - 1) * RowsPerPage);
            Refresh();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            Buy();
            Refresh();
        }
        else if ((Input.GetKeyDown(KeyCode.E) && Time.frameCount != openedFrame) || Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }
}

// Торговец оружием: стоит в отдельной комнате магазина
public class WeaponMerchant : MonoBehaviour
{
    private float bob;
    private Vector3 basePos;

    void Start()
    {
        basePos = transform.position;
    }

    void Update()
    {
        bob += Time.deltaTime;
        transform.position = basePos + Vector3.up * Mathf.Sin(bob * 2f) * 0.05f;

        PlayerController player = PlayerController.Instance;
        if (player == null || WeaponShopUI.Instance == null || WeaponShopUI.Instance.IsOpen()) return;

        if (Vector2.Distance(basePos, player.transform.position) < 2.5f)
        {
            HudHint.Show("[E] Merchant - weapons, SUPER ABILITIES, upgrades, robots");
            if (Input.GetKeyDown(KeyCode.E) && Time.frameCount != WeaponShopUI.Instance.ClosedFrame)
                WeaponShopUI.Instance.Open();
        }
    }
}
