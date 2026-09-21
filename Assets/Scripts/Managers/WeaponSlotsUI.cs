using UnityEngine;
using UnityEngine.UI;

// Короткая подсказка внизу экрана (живёт один кадр-два, вызывать каждый кадр)
public static class HudHint
{
    public static string text = "";
    public static float time = -10f;
    public static float until = -10f;
    public static void Show(string t) { text = t; time = Time.time; until = Time.time + 0.15f; }
    public static void Flash(string t, float seconds) { text = t; time = Time.time; until = Time.time + seconds; }
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
    private Text[] abilityKey = new Text[PlayerAbilities.MaxSlots];
    private Text[] abilityDesc = new Text[PlayerAbilities.MaxSlots];
    private Text[] abilityState = new Text[PlayerAbilities.MaxSlots];
    private Image[] abilityStrip = new Image[PlayerAbilities.MaxSlots];
    private Image[] abilityCd = new Image[PlayerAbilities.MaxSlots];
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

        // супер-способности: колонка слева по центру экрана (название, клавиша, эффект, перезарядка)
        for (int i = 0; i < PlayerAbilities.MaxSlots; i++)
        {
            GameObject ab = new GameObject("Ability" + (i + 1));
            ab.transform.SetParent(transform, false);
            abilityBoxes[i] = ab.AddComponent<Image>();
            abilityBoxes[i].color = new Color(0.06f, 0.06f, 0.1f, 0.85f);
            RectTransform ar = abilityBoxes[i].rectTransform;
            ar.anchorMin = ar.anchorMax = new Vector2(0f, 0.5f);
            ar.pivot = new Vector2(0f, 0.5f);
            ar.sizeDelta = new Vector2(230, 44);
            ar.anchoredPosition = new Vector2(14f, 56f - i * 50f);

            abilityStrip[i] = MakeImg(ab.transform, Color.white, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(8, 0));
            abilityCd[i] = MakeImg(ab.transform, new Color(0f, 0f, 0f, 0.6f), new Vector2(0, 0), new Vector2(1, 1), new Vector2(8, 0), Vector2.zero);
            abilityKey[i] = MakeTxt(ab.transform, 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(0, 1), new Vector2(8, 0), new Vector2(40, 0));
            abilityTexts[i] = MakeTxt(ab.transform, 16, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0, 0), new Vector2(1, 1), new Vector2(46, 0), new Vector2(-64, 0));
            abilityTexts[i].resizeTextForBestFit = true; abilityTexts[i].resizeTextMinSize = 10; abilityTexts[i].resizeTextMaxSize = 16;
            abilityDesc[i] = MakeTxt(ab.transform, 13, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(0, 0), new Vector2(0, 0), Vector2.zero, Vector2.zero);
            abilityDesc[i].gameObject.SetActive(false);   // полное описание теперь в меню способностей (клавиша B)
            abilityState[i] = MakeTxt(ab.transform, 15, FontStyle.Bold, TextAnchor.MiddleRight, new Vector2(1, 0), new Vector2(1, 1), new Vector2(-62, 0), new Vector2(-6, 0));
            abilityDesc[i].color = new Color(1f, 1f, 1f, 0.7f);
            ab.SetActive(false);
        }
    }

    Image MakeImg(Transform parent, Color col, Vector2 amin, Vector2 amax, Vector2 omin, Vector2 omax)
    {
        GameObject g = new GameObject("Img");
        g.transform.SetParent(parent, false);
        Image im = g.AddComponent<Image>();
        im.color = col;
        im.raycastTarget = false;
        RectTransform r = im.rectTransform;
        r.anchorMin = amin; r.anchorMax = amax; r.offsetMin = omin; r.offsetMax = omax;
        return im;
    }

    Text MakeTxt(Transform parent, int size, FontStyle style, TextAnchor anchor, Vector2 amin, Vector2 amax, Vector2 omin, Vector2 omax)
    {
        GameObject g = new GameObject("Txt");
        g.transform.SetParent(parent, false);
        Text t = g.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size; t.fontStyle = style; t.alignment = anchor;
        t.color = Color.white;
        t.raycastTarget = false;
        RectTransform r = t.rectTransform;
        r.anchorMin = amin; r.anchorMax = amax; r.offsetMin = omin; r.offsetMax = omax;
        return t;
    }

    // короткое описание эффекта под названием способности
    static string ShortEffect(string id)
    {
        switch (id)
        {
            case "Meteor Storm": return "16 meteors on enemies";
            case "Annihilation": return "Kills the current wave";
            case "Ascension": return "Godmode for 10s";
            case "Nova": return "Blast and clear bullets";
            case "Life Surge": return "+80 HP and +40 armor";
            case "Bullet Storm": return "Ring of 24 shots";
            case "Time Freeze": return "Freezes enemies 4s";
            case "Guardian Shield": return "Invulnerable for 5s";
            case "Overdrive": return "Free, fast fire for 6s";
        }
        return "";
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

        hint.text = Time.time < HudHint.until ? HudHint.text : "";

        PlayerAbilities pa = PlayerAbilities.Instance;
        for (int i = 0; i < PlayerAbilities.MaxSlots; i++)
        {
            bool has = pa != null && i < pa.owned.Count;
            if (abilityBoxes[i].gameObject.activeSelf != has) abilityBoxes[i].gameObject.SetActive(has);
            if (!has) continue;
            AbilityDef def = AbilityCatalog.Get(pa.owned[i]);
            float rem = pa.Remaining(def.id);
            bool used = pa.UsedHere(def.id);
            abilityStrip[i].color = def.color;
            abilityKey[i].text = PlayerAbilities.Keys[i].ToString();
            abilityKey[i].color = def.color;
            abilityTexts[i].text = def.id;
            abilityDesc[i].text = def.description;

            // затемнение убывает по мере перезарядки
            RectTransform cd = abilityCd[i].rectTransform;
            float frac = rem > 0f ? Mathf.Clamp01(rem / def.cooldown) : (used ? 1f : 0f);
            cd.anchorMax = new Vector2(frac, 1f);

            if (rem > 0f) { abilityState[i].text = Mathf.CeilToInt(rem) + "s"; abilityState[i].color = new Color(1f, 1f, 1f, 0.7f); }
            else if (used) { abilityState[i].text = "USED"; abilityState[i].color = new Color(1f, 0.55f, 0.4f); }
            else { abilityState[i].text = "READY"; abilityState[i].color = new Color(0.5f, 1f, 0.5f); }
        }
    }
}
