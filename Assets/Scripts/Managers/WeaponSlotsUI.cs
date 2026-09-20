using UnityEngine;
using UnityEngine.UI;

// Панель из 5 слотов оружия внизу экрана
public class WeaponSlotsUI : MonoBehaviour
{
    private Image[] frames = new Image[Inventory.MaxSlots];
    private Image[] icons = new Image[Inventory.MaxSlots];
    private string[] shown = new string[Inventory.MaxSlots];
    private Text hint;

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
        float total = Inventory.MaxSlots * size + (Inventory.MaxSlots - 1) * gap;

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
        hr.anchoredPosition = new Vector2(0, 116f);
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

        bool showHint = Time.time - WeaponPickup.hintTime < 0.15f;
        hint.text = showHint ? "[E] swap current weapon for " + WeaponPickup.hintWeapon : "";
    }
}
