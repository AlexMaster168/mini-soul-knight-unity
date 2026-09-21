using UnityEngine;
using UnityEngine.UI;

// Описание, цена и редкость оружия (для подсказок над дропом и в магазине)
public static class WeaponInfo
{
    public static Color RarityColor(string n)
    {
        int d = Damage(n);
        if (d >= 100) return new Color(1f, 0.6f, 0f);
        if (d >= 50) return new Color(0.75f, 0.35f, 1f);
        if (d >= 25) return new Color(0.3f, 0.6f, 1f);
        return new Color(0.85f, 0.85f, 0.85f);
    }

    public static string RarityName(string n)
    {
        int d = Damage(n);
        if (d >= 100) return "Legendary";
        if (d >= 50) return "Epic";
        if (d >= 25) return "Rare";
        return "Common";
    }

    static int Damage(string n)
    {
        return GameData.Weapons.ContainsKey(n) ? GameData.Weapons[n].damage : 0;
    }

    public static int Price(string n)
    {
        if (!GameData.Weapons.ContainsKey(n)) return 0;
        WeaponData d = GameData.Weapons[n];
        float dps = d.damage * Mathf.Max(1, d.projectiles) / Mathf.Max(0.04f, d.fireRate);
        float p = 25f + Mathf.Min(dps, 300f) * 0.35f;
        if (d.damage >= 100) p += 45f;
        if (d.aoe) p += 15f;
        if (d.pierce) p += 10f;
        return Mathf.RoundToInt(p / 5f) * 5;
    }

    public static string ClassName(string n)
    {
        switch (WeaponProfile.GetClass(n))
        {
            case WeaponClass.Pistol: return "Pistol";
            case WeaponClass.Shotgun: return "Shotgun";
            case WeaponClass.SMG: return "SMG";
            case WeaponClass.Rifle: return "Rifle";
            case WeaponClass.Sniper: return "Sniper";
            case WeaponClass.Energy: return "Energy";
            case WeaponClass.Heavy: return "Heavy";
            case WeaponClass.Exotic: return "Exotic";
            default: return "Melee";
        }
    }

    public static string Stats(string n)
    {
        if (!GameData.Weapons.ContainsKey(n)) return "";
        WeaponData d = GameData.Weapons[n];
        float dps = d.damage * Mathf.Max(1, d.projectiles) / Mathf.Max(0.04f, d.fireRate);
        string dmg = d.projectiles > 1 ? d.damage + " x" + d.projectiles : d.damage.ToString();
        string energy = d.energyCost > 0 ? d.energyCost + " per shot" : "none";
        return "<color=#FF9800>Damage:</color>  " + dmg + "\n" +
               "<color=#FFC107>Fire rate:</color>  " + (1f / Mathf.Max(0.04f, d.fireRate)).ToString("F1") + " /s\n" +
               "<color=#64B5F6>Energy:</color>  " + energy + "\n" +
               "<color=#A5D6A7>DPS:</color>  ~" + Mathf.RoundToInt(dps);
    }

    public static string Description(string n)
    {
        string text;
        switch (WeaponProfile.GetClass(n))
        {
            case WeaponClass.Pistol: text = "Reliable sidearm. Accurate and cheap on energy."; break;
            case WeaponClass.Shotgun: text = "Fires a spread of pellets. Devastating up close, weak at range."; break;
            case WeaponClass.SMG: text = "Very fast fire rate, low damage per bullet. Sprays the room."; break;
            case WeaponClass.Rifle: text = "Steady automatic fire with good range and accuracy."; break;
            case WeaponClass.Sniper: text = "Slow, precise, high-damage shots that fly far."; break;
            case WeaponClass.Energy: text = "Energy weapon with unusual projectiles."; break;
            case WeaponClass.Heavy: text = "Heavy ordnance. Big impact, big energy bill."; break;
            case WeaponClass.Exotic: text = "Strange weapon with special behaviour."; break;
            default: text = "Melee weapon: costs no energy and hits everything in front of you."; break;
        }

        switch (n)
        {
            case "HolyGrenade": text = "Blessed explosive. Enormous blast that wipes out whole groups."; break;
            case "Flamethrower": text = "Short-range cone of fire. Melts anything that gets close."; break;
            case "Minigun": text = "Endless stream of bullets. Drains energy fast."; break;
            case "Rocket": text = "Explosive rocket with splash damage."; break;
            case "Railgun": text = "Charged slug that pierces every enemy in a line."; break;
            case "Boomerang": text = "Thrown blade that flies out and returns to your hand."; break;
            case "Chakram": text = "Razor ring that slices through enemies and comes back."; break;
            case "Hammer": text = "Slow ground-pound. Smashes an area on impact."; break;
            case "Dagger": text = "Lightning-fast stabs at very short range."; break;
            case "Spear": text = "Long reach thrust that pierces enemies in a line."; break;
            case "Shock": text = "Crackling stars that zigzag toward enemies."; break;
            case "Thunder": text = "Spread of lightning bolts that arc unpredictably."; break;
            case "IceGun": text = "Fires twin ice shards."; break;
            case "PoisonGun": text = "Spits toxic globs in a wide spread."; break;
            case "Blunderbuss": text = "Nine pellets of pure chaos. Recoil will hurt your pride."; break;
            case "GoldenGun": text = "Shiny, accurate, and expensive-looking."; break;
            case "RustyBlade": text = "Emergency blade. Free to use when you run out of energy."; break;
        }

        WeaponData d = GameData.Weapons.ContainsKey(n) ? GameData.Weapons[n] : null;
        WeaponProfile p = WeaponProfile.Get(n);
        string extra = "";
        if (d != null)
        {
            if (d.pierce) extra += "\n- Pierces enemies";
            if (p.aoeRadius > 0f) extra += "\n- Explodes (radius " + p.aoeRadius.ToString("F1") + ")";
            if (p.boomerang) extra += "\n- Returns after being thrown";
            if (p.wobble > 0f) extra += "\n- Projectiles zigzag";
            if (d.projectiles > 1) extra += "\n- " + d.projectiles + " projectiles per shot";
            if (d.energyCost == 0) extra += "\n- Uses no energy";
        }
        return text + extra;
    }

    public static string Full(string n)
    {
        return Stats(n) + "\n\n" + Description(n);
    }
}

// Небольшой набор помощников для процедурного UI
public static class UiKit
{
    public static Font Font { get { return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } }

    // Прямоугольник, привязанный к левому верхнему углу родителя
    public static RectTransform TopLeft(GameObject g, float x, float y, float w, float h)
    {
        RectTransform r = g.GetComponent<RectTransform>();
        if (r == null) r = g.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0, 1);
        r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y);
        r.sizeDelta = new Vector2(w, h);
        return r;
    }

    public static Text MakeText(Transform parent, string name, int size, Color color, TextAnchor anchor, float x, float y, float w, float h)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(parent, false);
        Text t = g.AddComponent<Text>();
        t.font = Font;
        t.fontSize = size;
        t.color = color;
        t.alignment = anchor;
        t.supportRichText = true;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        TopLeft(g, x, y, w, h);
        return t;
    }

    public static Image MakeImage(Transform parent, string name, Color color, float x, float y, float w, float h)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(parent, false);
        Image i = g.AddComponent<Image>();
        i.color = color;
        i.raycastTarget = false;
        TopLeft(g, x, y, w, h);
        return i;
    }
}

// Панель с описанием оружия, которое лежит рядом на полу
public class WeaponInfoUI : MonoBehaviour
{
    private static string reqWeapon;
    private static string reqAction;
    private static float reqDist = float.MaxValue;

    private GameObject panel;
    private Text title, sub, body, action;
    private Image icon;
    private string shown;

    // Ближайший к игроку дроп выигрывает
    public static void Request(string weapon, float dist, string actionLine)
    {
        if (dist < reqDist)
        {
            reqDist = dist;
            reqWeapon = weapon;
            reqAction = actionLine;
        }
    }

    void Start()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 25;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        panel = new GameObject("InfoPanel");
        panel.transform.SetParent(transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.1f, 0.92f);
        bg.raycastTarget = false;
        RectTransform r = panel.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(1, 0.5f);
        r.pivot = new Vector2(1, 0.5f);
        r.anchoredPosition = new Vector2(-30, 0);
        r.sizeDelta = new Vector2(400, 420);

        icon = UiKit.MakeImage(panel.transform, "Icon", Color.white, 300, 14, 90, 50);
        icon.preserveAspect = true;
        title = UiKit.MakeText(panel.transform, "Title", 26, Color.white, TextAnchor.UpperLeft, 18, 14, 280, 34);
        sub = UiKit.MakeText(panel.transform, "Sub", 16, Color.gray, TextAnchor.UpperLeft, 18, 48, 280, 24);
        title.resizeTextForBestFit = true; title.resizeTextMinSize = 14; title.resizeTextMaxSize = 26; title.verticalOverflow = VerticalWrapMode.Truncate;
        sub.resizeTextForBestFit = true; sub.resizeTextMinSize = 11; sub.resizeTextMaxSize = 16; sub.verticalOverflow = VerticalWrapMode.Truncate;
        body = UiKit.MakeText(panel.transform, "Body", 17, Color.white, TextAnchor.UpperLeft, 18, 84, 364, 260);
        action = UiKit.MakeText(panel.transform, "Action", 17, new Color(1f, 0.9f, 0.4f), TextAnchor.LowerLeft, 18, 350, 364, 60);
        action.fontStyle = FontStyle.Bold;
        panel.SetActive(false);
    }

    void LateUpdate()
    {
        bool show = reqWeapon != null;
        if (panel.activeSelf != show) panel.SetActive(show);

        if (show && shown != reqWeapon)
        {
            shown = reqWeapon;
            Color c = WeaponInfo.RarityColor(shown);
            title.text = shown;
            title.color = c;
            sub.text = WeaponInfo.RarityName(shown) + "  -  " + WeaponInfo.ClassName(shown);
            sub.color = c * 0.85f;
            icon.sprite = PixelArt.Weapon(shown);
            body.text = WeaponInfo.Full(shown);
        }
        if (show) action.text = reqAction;

        reqWeapon = null;
        reqDist = float.MaxValue;
    }
}
