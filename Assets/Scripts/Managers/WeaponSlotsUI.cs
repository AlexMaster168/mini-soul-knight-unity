using UnityEngine;
using UnityEngine.UI;

// Короткая подсказка внизу экрана (живёт один кадр-два, вызывать каждый кадр)
public static class HudHint
{
    public static string text = "";
    public static float time = -10f;
    public static void Show(string t) { text = t; time = Time.time; }
}

// Панель слотов оружия внизу экрана + слоты супер-способностей
public class WeaponSlotsUI : MonoBehaviour
{
    private Image[] frames = new Image[Inventory.MaxSlots];
    private Image[] icons = new Image[Inventory.MaxSlots];
    private string[] shown = new string[Inventory.MaxSlots];
    private Text hint;
    private Image[] abilityBoxes = new Image[PlayerAbilities.MaxSlots];
    private Text[] abilityTexts = new Text[PlayerAbilities.MaxSlots];
    private Image reserveFrame;
    private Image reserveIcon;
    private Text reserveLabel;

    void Start()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        float size = 84f, gap = 8f;
        int cells = Inventory.MaxSlots + 1;
        float total = cells * size + (cells - 1) * gap;

        for (int i = 0; i < Inventory.MaxSlots; i++)
        {
            GameObject f = new GameObject("Slot" + (i + 1));
            f.transform.SetParent(transform, false);
            frames[i] = f.AddComponent<Image>();
            RectTransform r = frames[i].rectTransform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0f);
            r.pivot = new Vector2(0.5f, 0f);
            r.sizeDelta = new Vector2(size, size);
            r.anchoredPosition = new Vector2(-total / 2f + size / 2f + i * (size + gap), 18f);

            GameObject ic = new GameObject("Icon");
            ic.transform.SetParent(f.transform, false);
            icons[i] = ic.AddComponent<Image>();
            icons[i].preserveAspect = true;
            icons[i].raycastTarget = false;
            RectTransform ir = icons[i].rectTransform;
            ir.anchorMin = Vector2.zero; ir.anchorMax = Vector2.one;
            ir.offsetMin = new Vector2(6, 6); ir.offsetMax = new Vector2(-6, -6);

            GameObject n = new GameObject("Num");
            n.transform.SetParent(f.transform, false);
            Text t = n.AddComponent<Text>();
            t.font = font; t.fontSize = 20; t.text = (i + 1).ToString();
            t.color = new Color(1f, 1f, 1f, 0.8f);
            t.alignment = TextAnchor.UpperLeft;
            t.raycastTarget = false;
            RectTransform nr = t.rectTransform;
            nr.anchorMin = Vector2.zero; nr.anchorMax = Vector2.one;
            nr.offsetMin = new Vector2(6, 0); nr.offsetMax = new Vector2(0, -3);
        }

        // слот запасного клинка - появляется только когда нет энергии
        GameObject rf = new GameObject("ReserveSlot");
        rf.transform.SetParent(transform, false);
        reserveFrame = rf.AddComponent<Image>();
        RectTransform rr = reserveFrame.rectTransform;
        rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0f);
        rr.pivot = new Vector2(0.5f, 0f);
        rr.sizeDelta = new Vector2(size, size);
        rr.anchoredPosition = new Vector2(-total / 2f + size / 2f + Inventory.MaxSlots * (size + gap), 18f);
        GameObject ric = new GameObject("Icon");
        ric.transform.SetParent(rf.transform, false);
        reserveIcon = ric.AddComponent<Image>();
        reserveIcon.sprite = PixelArt.Weapon(GameData.ReserveWeapon);
        reserveIcon.preserveAspect = true;
        reserveIcon.raycastTarget = false;
        RectTransform rir = reserveIcon.rectTransform;
        rir.anchorMin = Vector2.zero; rir.anchorMax = Vector2.one;
        rir.offsetMin = new Vector2(6, 6); rir.offsetMax = new Vector2(-6, -6);
        GameObject rl = new GameObject("Label");
        rl.transform.SetParent(rf.transform, false);
        reserveLabel = rl.AddComponent<Text>();
        reserveLabel.font = font; reserveLabel.fontSize = 20; reserveLabel.text = "9";
        reserveLabel.color = new Color(1f, 0.85f, 0.7f);
        reserveLabel.alignment = TextAnchor.UpperLeft;
        reserveLabel.raycastTarget = false;
        RectTransform rlr = reserveLabel.rectTransform;
        rlr.anchorMin = Vector2.zero; rlr.anchorMax = Vector2.one;
        rlr.offsetMin = new Vector2(6, 0); rlr.offsetMax = new Vector2(0, -3);

        GameObject h = new GameObject("Hint");
        h.transform.SetParent(transform, false);
        hint = h.AddComponent<Text>();
        hint.font = font; hint.fontSize = 26; hint.fontStyle = FontStyle.Bold;
        hint.alignment = TextAnchor.MiddleCenter;
        hint.color = new Color(1f, 0.9f, 0.4f);
        hint.raycastTarget = false;
        RectTransform hr = hint.rectTransform;
        hr.anchorMin = hr.anchorMax = new Vector2(0.5f, 0f);
        hr.pivot = new Vector2(0.5f, 0f);
        hr.sizeDelta = new Vector2(900, 40);
        hr.anchoredPosition = new Vector2(0, 205f);

        // слоты супер-способностей: над правым краем панели оружия
        for (int i = 0; i < PlayerAbilities.MaxSlots; i++)
        {
            GameObject ab = new GameObject("Ability" + (i + 1));
            ab.transform.SetParent(transform, false);
            abilityBoxes[i] = ab.AddComponent<Image>();
            RectTransform ar = abilityBoxes[i].rectTransform;
            ar.anchorMin = ar.anchorMax = new Vector2(0.5f, 0f);
            ar.pivot = new Vector2(0.5f, 0f);
            ar.sizeDelta = new Vector2(96, 78);
            ar.anchoredPosition = new Vector2(total / 2f - 48f - i * 104f, 112f);

            GameObject at = new GameObject("Label");
            at.transform.SetParent(ab.transform, false);
            abilityTexts[i] = at.AddComponent<Text>();
            abilityTexts[i].font = font;
            abilityTexts[i].fontSize = 16;
            abilityTexts[i].fontStyle = FontStyle.Bold;
            abilityTexts[i].alignment = TextAnchor.MiddleCenter;
            abilityTexts[i].color = Color.white;
            abilityTexts[i].raycastTarget = false;
            RectTransform tr = abilityTexts[i].rectTransform;
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = tr.offsetMax = Vector2.zero;
            ab.SetActive(false);
        }
    }

    void Update()
    {
        Inventory inv = Inventory.Instance;
        if (inv == null || frames[0] == null) return;

        for (int i = 0; i < Inventory.MaxSlots; i++)
        {
            string w = i < inv.weapons.Count ? inv.weapons[i] : null;
            if (w != shown[i])
            {
                shown[i] = w;
                icons[i].enabled = w != null;
                if (w != null) icons[i].sprite = PixelArt.Weapon(w);
            }
            bool current = i == inv.currentWeaponIndex && w != null;
            frames[i].color = current ? new Color(0.95f, 0.75f, 0.2f, 0.95f) : new Color(0.08f, 0.08f, 0.12f, 0.75f);
        }

        bool reserve = inv.reserveActive;
        // слот 9 виден всегда: тусклый в запасе, яркий когда клинок в руках
        reserveFrame.color = reserve ? new Color(0.9f, 0.3f, 0.2f, 0.95f) : new Color(0.08f, 0.08f, 0.12f, 0.75f);
        reserveIcon.color = reserve ? Color.white : new Color(1f, 1f, 1f, 0.35f);
        if (reserve)
        {
            // основной слот с текущим оружием гасим - оно сейчас недоступно
            for (int i = 0; i < Inventory.MaxSlots; i++)
                if (i == inv.currentWeaponIndex) frames[i].color = new Color(0.4f, 0.12f, 0.1f, 0.85f);
        }

        hint.text = Time.time - HudHint.time < 0.15f ? HudHint.text : "";

        PlayerAbilities pa = PlayerAbilities.Instance;
        for (int i = 0; i < PlayerAbilities.MaxSlots; i++)
        {
            bool has = pa != null && i < pa.owned.Count;
            if (abilityBoxes[i].gameObject.activeSelf != has) abilityBoxes[i].gameObject.SetActive(has);
            if (!has) continue;
            AbilityDef def = AbilityCatalog.Get(pa.owned[i]);
            float rem = pa.Remaining(def.id);
            Color c = def.color;
            abilityBoxes[i].color = rem > 0f ? new Color(c.r * 0.25f, c.g * 0.25f, c.b * 0.25f, 0.85f) : new Color(c.r * 0.7f, c.g * 0.7f, c.b * 0.7f, 0.95f);
            abilityTexts[i].text = "[" + PlayerAbilities.Keys[i] + "]\n" + def.shortName + "\n" + (rem > 0f ? Mathf.CeilToInt(rem) + "s" : "READY");
        }
    }
}
