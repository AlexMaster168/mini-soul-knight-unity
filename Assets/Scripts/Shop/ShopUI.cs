using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance;

    private GameObject shopCanvas;
    private GameObject shopPanel;
    private Text goldText;
    private bool isOpen;
    private int selectedIndex;
    private int itemCount = 10;
    private const int PageSize = 5;
    private Image[] rowBgs;
    private Text pageText;
    private Text[] costTexts;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateShopUI();
    }

    void CreateShopUI()
    {
        shopCanvas = new GameObject("ShopCanvas");
        Canvas canvas = shopCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        CanvasScaler scaler = shopCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        shopPanel = new GameObject("ShopPanel");
        shopPanel.transform.SetParent(shopCanvas.transform, false);
        Image panelBg = shopPanel.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.04f, 0.1f, 0.97f);
        RectTransform panelRect = shopPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(560, 500);

        CreateTitle("WEAPON SHOP");
        CreateGoldDisplay();
        CreateHint();

        string[] names = { "Random Weapon", "Health +50", "Energy +80", "Armor +30", "Damage x2 (8s)", "Speed x1.5 (8s)", "FULL Health", "FULL Energy", "FULL Armor", "FULL RESTORE (all)" };
        int[] costs = { 25, 15, 10, 20, 18, 12, 0, 0, 0, 0 };
        Color[] colors = {
            new Color(1f, 0.85f, 0.1f),
            new Color(0.2f, 0.9f, 0.2f),
            new Color(0.2f, 0.5f, 1f),
            new Color(0.3f, 0.7f, 1f),
            new Color(0.9f, 0.2f, 0.2f),
            new Color(0.2f, 0.9f, 0.9f),
            new Color(0.35f, 1f, 0.45f),
            new Color(0.35f, 0.65f, 1f),
            new Color(0.6f, 0.85f, 1f),
            new Color(1f, 0.85f, 0.2f)
        };

        rowBgs = new Image[itemCount];
        costTexts = new Text[itemCount];

        for (int i = 0; i < names.Length; i++)
            CreateItemRow(names[i], costs[i], i, colors[i]);

        CreateCloseHint();

        GameObject pg = new GameObject("Page");
        pg.transform.SetParent(shopPanel.transform, false);
        pageText = pg.AddComponent<Text>();
        pageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        pageText.fontSize = 16;
        pageText.color = new Color(1f, 1f, 1f, 0.7f);
        pageText.alignment = TextAnchor.MiddleCenter;
        RectTransform pr = pg.GetComponent<RectTransform>();
        pr.anchorMin = new Vector2(0, 0);
        pr.anchorMax = new Vector2(1, 0);
        pr.pivot = new Vector2(0.5f, 0);
        pr.anchoredPosition = new Vector2(0, 42);
        pr.sizeDelta = new Vector2(0, 26);
        shopPanel.SetActive(false);
    }

    void CreateTitle(string title)
    {
        GameObject obj = new GameObject("Title");
        obj.transform.SetParent(shopPanel.transform, false);
        Text t = obj.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 30;
        t.color = new Color(1f, 0.85f, 0.3f);
        t.alignment = TextAnchor.MiddleCenter;
        t.fontStyle = FontStyle.Bold;
        t.text = title;
        RectTransform r = obj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0, 1);
        r.anchorMax = new Vector2(1, 1);
        r.pivot = new Vector2(0.5f, 1);
        r.anchoredPosition = new Vector2(0, -15);
        r.sizeDelta = new Vector2(0, 45);
    }

    void CreateGoldDisplay()
    {
        GameObject obj = new GameObject("GoldDisplay");
        obj.transform.SetParent(shopPanel.transform, false);
        goldText = obj.AddComponent<Text>();
        goldText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        goldText.fontSize = 22;
        goldText.color = new Color(1f, 0.85f, 0.1f);
        goldText.alignment = TextAnchor.MiddleCenter;
        goldText.text = "Gold: 0";
        RectTransform r = obj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0, 1);
        r.anchorMax = new Vector2(1, 1);
        r.pivot = new Vector2(0.5f, 1);
        r.anchoredPosition = new Vector2(0, -60);
        r.sizeDelta = new Vector2(0, 30);
    }

    void CreateHint()
    {
        GameObject obj = new GameObject("Hint");
        obj.transform.SetParent(shopPanel.transform, false);
        Text t = obj.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 14;
        t.color = new Color(1f, 1f, 1f, 0.5f);
        t.alignment = TextAnchor.MiddleCenter;
        t.text = "Up/Down select  |  Left/Right page  |  Enter buy  |  F full restore  |  E close";
        RectTransform r = obj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0, 1);
        r.anchorMax = new Vector2(1, 1);
        r.pivot = new Vector2(0.5f, 1);
        r.anchoredPosition = new Vector2(0, -88);
        r.sizeDelta = new Vector2(0, 20);
    }

    void CreateItemRow(string name, int cost, int index, Color color)
    {
        float yStart = -115;
        float rowH = 52;
        float y = yStart - (index % PageSize) * rowH;

        GameObject row = new GameObject("Row_" + index);
        row.transform.SetParent(shopPanel.transform, false);
        Image rowBg = row.AddComponent<Image>();
        rowBg.color = new Color(color.r * 0.12f, color.g * 0.12f, color.b * 0.12f, 0.85f);
        rowBgs[index] = rowBg;
        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0, 1);
        rowRect.anchorMax = new Vector2(1, 1);
        rowRect.pivot = new Vector2(0.5f, 1);
        rowRect.anchoredPosition = new Vector2(0, y);
        rowRect.sizeDelta = new Vector2(-30, rowH - 5);

        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(row.transform, false);
        Text nameText = nameObj.AddComponent<Text>();
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 18;
        nameText.color = color;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.text = name;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0);
        nameRect.anchorMax = new Vector2(0.6f, 1);
        nameRect.offsetMin = new Vector2(15, 0);
        nameRect.offsetMax = Vector2.zero;

        GameObject costObj = new GameObject("Cost");
        costObj.transform.SetParent(row.transform, false);
        Text costTxt = costObj.AddComponent<Text>();
        costTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        costTxt.fontSize = 18;
        costTxt.color = Color.white;
        costTxt.alignment = TextAnchor.MiddleCenter;
        costTxt.fontStyle = FontStyle.Bold;
        costTxt.text = cost + " G";
        costTexts[index] = costTxt;
        RectTransform costRect = costObj.GetComponent<RectTransform>();
        costRect.anchorMin = new Vector2(0.65f, 0);
        costRect.anchorMax = new Vector2(0.97f, 1);
        costRect.offsetMin = Vector2.zero;
        costRect.offsetMax = Vector2.zero;
    }

    void CreateCloseHint()
    {
        GameObject obj = new GameObject("CloseHint");
        obj.transform.SetParent(shopPanel.transform, false);
        Text t = obj.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 16;
        t.color = new Color(1f, 0.4f, 0.4f);
        t.alignment = TextAnchor.MiddleCenter;
        t.fontStyle = FontStyle.Bold;
        t.text = "Press E to close";
        RectTransform r = obj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0, 0);
        r.anchorMax = new Vector2(1, 0);
        r.pivot = new Vector2(0.5f, 0);
        r.anchoredPosition = new Vector2(0, 10);
        r.sizeDelta = new Vector2(0, 30);
    }

    public void ToggleShop()
    {
        isOpen = !isOpen;
        shopPanel.SetActive(isOpen);
        if (isOpen)
        {
            if (ProceduralMusic.Instance != null)
                ProceduralMusic.Instance.PlaySFX("shopOpen");
            selectedIndex = 0;
            UpdateSelection();
            UpdateGoldDisplay();
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public void CloseShop()
    {
        isOpen = false;
        shopPanel.SetActive(false);
    }

    void UpdateSelection()
    {
        int page = selectedIndex / PageSize;
        int pages = (itemCount + PageSize - 1) / PageSize;
        pageText.text = "Page " + (page + 1) + " / " + pages;
        for (int i = 0; i < itemCount; i++)
        {
            rowBgs[i].gameObject.SetActive(i / PageSize == page);
            if (i == selectedIndex)
                rowBgs[i].color = new Color(rowBgs[i].color.r, rowBgs[i].color.g, rowBgs[i].color.b, 1f);
            else
                rowBgs[i].color = new Color(rowBgs[i].color.r, rowBgs[i].color.g, rowBgs[i].color.b, 0.5f);
        }
    }

    void UpdateGoldDisplay()
    {
        if (ScoreManager.Instance != null)
            goldText.text = "Gold: " + ScoreManager.Instance.GetGold();
        for (int i = 6; i < itemCount; i++)
            costTexts[i].text = FullCost(i) > 0 ? FullCost(i) + " G" : "FULL";
    }

    // Полное восстановление: цена за недостающие единицы (HP 2, энергия 1, броня 3)
    int MissingHp() { var p = PlayerController.Instance; return p == null ? 0 : Mathf.Max(0, p.maxHealth - p.currentHealth); }
    int MissingEn() { var p = PlayerController.Instance; return p == null ? 0 : Mathf.Max(0, p.maxEnergy - p.currentEnergy); }
    int MissingAr() { var p = PlayerController.Instance; return p == null ? 0 : Mathf.Max(0, p.maxArmor - p.armor); }
    int FullCost(int index)
    {
        switch (index)
        {
            case 6: return MissingHp() * 2;
            case 7: return MissingEn();
            case 8: return MissingAr() * 3;
            default: return MissingHp() * 2 + MissingEn() + MissingAr() * 3;
        }
    }

    void BuyFull(int index)
    {
        int cost = FullCost(index);
        if (cost <= 0) return;
        if (ScoreManager.Instance == null || !ScoreManager.Instance.SpendGold(cost)) return;
        PlayerController p = PlayerController.Instance;
        if (index == 6 || index == 9) p.currentHealth = p.maxHealth;
        if (index == 7 || index == 9) p.currentEnergy = p.maxEnergy;
        if (index == 8 || index == 9) p.armor = p.maxArmor;
        if (EffectsManager.Instance != null)
            EffectsManager.Instance.SpawnPickupEffect(p.transform.position, new Color(0.5f, 1f, 0.6f));
    }

    void BuyItem(int index)
    {
        switch (index)
        {
            case 0: BuyRandomWeapon(); break;
            case 1: BuyHealth(); break;
            case 2: BuyEnergy(); break;
            case 3: BuyArmor(); break;
            case 4: BuyDamageBoost(); break;
            case 5: BuySpeedBoost(); break;
            case 6: case 7: case 8: case 9: BuyFull(index); break;
        }
        UpdateGoldDisplay();
    }

    void BuyRandomWeapon()
    {
        if (ScoreManager.Instance == null || !ScoreManager.Instance.SpendGold(25)) return;
        string[] weapons = GameData.LootableWeapons();
        string weapon = weapons[Random.Range(0, weapons.Length)];
        Inventory inv = PlayerController.Instance.GetComponent<Inventory>();
        if (inv != null) inv.AddWeapon(weapon, true);
        if (EffectsManager.Instance != null)
            EffectsManager.Instance.SpawnPickupEffect(PlayerController.Instance.transform.position, new Color(1f, 0.85f, 0.1f));
    }

    void BuyHealth()
    {
        if (ScoreManager.Instance == null || !ScoreManager.Instance.SpendGold(15)) return;
        PlayerController p = PlayerController.Instance;
        if (p != null) p.currentHealth = Mathf.Min(p.currentHealth + 50, p.maxHealth);
    }

    void BuyEnergy()
    {
        if (ScoreManager.Instance == null || !ScoreManager.Instance.SpendGold(10)) return;
        PlayerController p = PlayerController.Instance;
        if (p != null) p.currentEnergy = Mathf.Min(p.currentEnergy + 80, p.maxEnergy);
    }

    void BuyArmor()
    {
        if (ScoreManager.Instance == null || !ScoreManager.Instance.SpendGold(20)) return;
        PlayerController p = PlayerController.Instance;
        if (p != null) p.armor = Mathf.Min(p.armor + 30, p.maxArmor);
    }

    void BuyDamageBoost()
    {
        if (ScoreManager.Instance == null || !ScoreManager.Instance.SpendGold(18)) return;
        StartCoroutine(DamageBoostCoroutine(PlayerController.Instance));
    }

    void BuySpeedBoost()
    {
        if (ScoreManager.Instance == null || !ScoreManager.Instance.SpendGold(12)) return;
        StartCoroutine(SpeedBoostCoroutine(PlayerController.Instance));
    }

    System.Collections.IEnumerator DamageBoostCoroutine(PlayerController player)
    {
        int original = player.attackDamage;
        player.attackDamage *= 2;
        yield return new WaitForSeconds(8f);
        player.attackDamage = original;
    }

    System.Collections.IEnumerator SpeedBoostCoroutine(PlayerController player)
    {
        float original = player.moveSpeed;
        player.moveSpeed *= 1.5f;
        yield return new WaitForSeconds(8f);
        player.moveSpeed = original;
    }

    void Update()
    {
        if (!isOpen) return;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = itemCount - 1;
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedIndex++;
            if (selectedIndex >= itemCount) selectedIndex = 0;
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.H))
        {
            BuyItem(selectedIndex);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            selectedIndex = ((selectedIndex / PageSize + 1) * PageSize) % itemCount;
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            selectedIndex = ((selectedIndex / PageSize - 1) * PageSize + itemCount) % itemCount;
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            BuyItem(9);
        }
        else if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }
}
