using UnityEngine;
using UnityEngine.UI;

// Отдельное меню супер-способностей игрока (клавиша B): названия, клавиши, полные описания, перезарядка
public class AbilityMenuUI : MonoBehaviour
{
    public static AbilityMenuUI Instance;

    private GameObject panel;
    private bool isOpen;
    private int openedFrame;
    private readonly Image[] strip = new Image[PlayerAbilities.MaxSlots];
    private readonly Image[] bg = new Image[PlayerAbilities.MaxSlots];
    private readonly Text[] key = new Text[PlayerAbilities.MaxSlots];
    private readonly Text[] name = new Text[PlayerAbilities.MaxSlots];
    private readonly Text[] state = new Text[PlayerAbilities.MaxSlots];
    private readonly Text[] desc = new Text[PlayerAbilities.MaxSlots];

    public bool IsOpen() { return isOpen; }

    void Awake() { Instance = this; }

    void Start() { Build(); }

    void Build()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 96;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        panel = new GameObject("AbilityMenuPanel");
        panel.transform.SetParent(transform, false);
        Image pbg = panel.AddComponent<Image>();
        pbg.color = new Color(0.05f, 0.04f, 0.1f, 0.97f);
        RectTransform pr = panel.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.pivot = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(900, 620);

        Text title = UiKit.MakeText(panel.transform, "Title", 30, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter, 20, 10, 860, 50);
        title.fontStyle = FontStyle.Bold;
        title.text = "SUPER ABILITIES";
        UiKit.MakeText(panel.transform, "Sub", 16, new Color(1f, 1f, 1f, 0.6f), TextAnchor.MiddleCenter, 20, 58, 860, 26).text =
            "B / Esc - close  |  Z / X / C - use  |  each ability works once per room";

        for (int i = 0; i < PlayerAbilities.MaxSlots; i++)
        {
            float y = 100 + i * 158;
            bg[i] = UiKit.MakeImage(panel.transform, "Row" + i, new Color(0.12f, 0.1f, 0.2f, 0.95f), 20, y, 860, 148);
            strip[i] = UiKit.MakeImage(bg[i].transform, "Strip", Color.white, 0, 0, 10, 148);
            key[i] = UiKit.MakeText(bg[i].transform, "Key", 44, Color.white, TextAnchor.MiddleCenter, 20, 0, 90, 148);
            key[i].fontStyle = FontStyle.Bold;
            name[i] = UiKit.MakeText(bg[i].transform, "Name", 26, Color.white, TextAnchor.MiddleLeft, 120, 8, 560, 40);
            name[i].fontStyle = FontStyle.Bold;
            state[i] = UiKit.MakeText(bg[i].transform, "State", 22, Color.white, TextAnchor.MiddleRight, 690, 8, 160, 40);
            state[i].fontStyle = FontStyle.Bold;
            desc[i] = UiKit.MakeText(bg[i].transform, "Desc", 18, new Color(1f, 1f, 1f, 0.85f), TextAnchor.UpperLeft, 120, 54, 730, 90);
        }
        panel.SetActive(false);
    }

    void Refresh()
    {
        PlayerAbilities pa = PlayerAbilities.Instance;
        for (int i = 0; i < PlayerAbilities.MaxSlots; i++)
        {
            bool has = pa != null && i < pa.owned.Count;
            key[i].text = PlayerAbilities.Keys[i].ToString();
            if (!has)
            {
                strip[i].color = new Color(1f, 1f, 1f, 0.15f);
                key[i].color = new Color(1f, 1f, 1f, 0.3f);
                name[i].text = "Empty slot";
                name[i].color = new Color(1f, 1f, 1f, 0.5f);
                state[i].text = "";
                desc[i].text = "Buy super abilities from a merchant (E). They cost a lot but are extremely powerful.";
                continue;
            }

            AbilityDef def = AbilityCatalog.Get(pa.owned[i]);
            float rem = pa.Remaining(def.id);
            strip[i].color = def.color;
            key[i].color = def.color;
            name[i].text = def.id;
            name[i].color = def.color;
            desc[i].text = def.description + "\n\nCooldown " + def.cooldown + "s. Can be used only once in each room.";
            if (rem > 0f) { state[i].text = Mathf.CeilToInt(rem) + "s"; state[i].color = new Color(1f, 1f, 1f, 0.7f); }
            else if (pa.UsedHere(def.id)) { state[i].text = "USED"; state[i].color = new Color(1f, 0.55f, 0.4f); }
            else { state[i].text = "READY"; state[i].color = new Color(0.5f, 1f, 0.5f); }
        }
    }

    void Update()
    {
        if (panel == null) return;

        bool blocked = (ShopUI.Instance != null && ShopUI.Instance.IsOpen())
                       || (WeaponShopUI.Instance != null && WeaponShopUI.Instance.IsOpen())
                       || (LobbyUI.Instance != null && LobbyUI.Instance.IsOpen());

        if (!isOpen)
        {
            if (Input.GetKeyDown(KeyCode.B) && !blocked && (LobbyController.Active == false))
            {
                isOpen = true;
                openedFrame = Time.frameCount;
                panel.SetActive(true);
            }
        }
        else if ((Input.GetKeyDown(KeyCode.B) && Time.frameCount != openedFrame) || Input.GetKeyDown(KeyCode.Escape))
        {
            isOpen = false;
            panel.SetActive(false);
        }

        if (isOpen) Refresh();
    }
}
