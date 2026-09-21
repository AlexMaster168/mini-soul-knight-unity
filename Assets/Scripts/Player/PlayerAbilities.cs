using System.Collections.Generic;
using UnityEngine;

public class AbilityDef
{
    public string id, shortName, description;
    public int price;
    public float cooldown;
    public Color color;

    public AbilityDef(string id, string shortName, int price, float cooldown, Color color, string description)
    {
        this.id = id;
        this.shortName = shortName;
        this.price = price;
        this.cooldown = cooldown;
        this.color = color;
        this.description = description;
    }
}

public static class AbilityCatalog
{
    public static readonly AbilityDef[] All =
    {
        new AbilityDef("Meteor Storm", "METEORS", 700, 45f, new Color(1f, 0.5f, 0.15f),
            "Calls down 16 meteors over 3 seconds on the enemies' heads. Each blast deals 260 damage in a wide area."),
        new AbilityDef("Annihilation", "ANNIHILATE", 800, 60f, new Color(1f, 0.15f, 0.2f),
            "Wipes out every enemy of the current wave. Bosses lose 30% of their max HP."),
        new AbilityDef("Ascension", "ASCEND", 900, 90f, new Color(1f, 0.85f, 0.3f),
            "10 seconds of godhood: invulnerable, x3 damage, free shots."),
        new AbilityDef("Nova", "NOVA", 300, 15f, new Color(0.4f, 0.8f, 1f),
            "Shockwave that blasts every enemy around you for heavy damage and wipes all enemy bullets."),
        new AbilityDef("Life Surge", "HEAL", 350, 45f, new Color(0.35f, 0.9f, 0.4f),
            "Instantly restores 80 HP and grants 40 armor."),
        new AbilityDef("Bullet Storm", "STORM", 400, 20f, new Color(1f, 0.7f, 0.2f),
            "Fires a ring of 24 shots of your current weapon in every direction."),
        new AbilityDef("Time Freeze", "FREEZE", 450, 30f, new Color(0.6f, 0.85f, 1f),
            "Freezes every enemy in place for 4 seconds and clears enemy bullets."),
        new AbilityDef("Guardian Shield", "SHIELD", 450, 35f, new Color(0.5f, 0.6f, 1f),
            "Total invulnerability for 5 seconds."),
        new AbilityDef("Overdrive", "OVERDRIVE", 500, 40f, new Color(1f, 0.4f, 0.4f),
            "For 6 seconds weapons cost no energy and fire 60% faster."),
    };

    public static AbilityDef Get(string id)
    {
        foreach (AbilityDef a in All)
            if (a.id == id) return a;
        return null;
    }
}

// Супер-способности игрока: купленные у торговца, до 3 штук на клавишах Z / X / C
public class PlayerAbilities : MonoBehaviour
{
    public static PlayerAbilities Instance;
    public const int MaxSlots = 3;
    public static readonly KeyCode[] Keys = { KeyCode.Z, KeyCode.X, KeyCode.C };

    public List<string> owned = new List<string>();
    private readonly Dictionary<string, float> readyAt = new Dictionary<string, float>();
    private readonly Dictionary<string, int> usedInRoom = new Dictionary<string, int>();

    private int lastRoom = int.MinValue;

    int RoomId { get { return DungeonGenerator.Instance != null ? DungeonGenerator.Instance.CurrentRoomId : -1; } }

    // способность уже применена в этой комнате (каждую можно один раз за комнату)
    public bool UsedHere(string id)
    {
        int r;
        return usedInRoom.TryGetValue(id, out r) && r == RoomId;
    }

    void Awake()
    {
        Instance = this;
    }

    public bool Owns(string id) { return owned.Contains(id); }
    public bool IsFull { get { return owned.Count >= MaxSlots; } }

    public bool Add(string id)
    {
        if (owned.Contains(id) || IsFull) return false;
        owned.Add(id);
        return true;
    }

    public float Remaining(string id)
    {
        float t;
        return readyAt.TryGetValue(id, out t) ? Mathf.Max(0f, t - Time.time) : 0f;
    }

    void Update()
    {
        PlayerController pc = PlayerController.Instance;
        if (pc == null || !pc.enabled) return;
        if (Time.timeScale == 0f) return;
        if (ShopUI.Instance != null && ShopUI.Instance.IsOpen()) return;
        if (WeaponShopUI.Instance != null && WeaponShopUI.Instance.IsOpen()) return;
        if (DungeonGenerator.Instance != null && DungeonGenerator.Instance.IsTransitioning) return;

        // в новой комнате все способности перезаряжены
        int room = RoomId;
        if (room != lastRoom)
        {
            lastRoom = room;
            readyAt.Clear();
        }

        for (int i = 0; i < owned.Count && i < Keys.Length; i++)
            if (Input.GetKeyDown(Keys[i])) Use(owned[i], pc);
    }

    void Use(string id, PlayerController pc)
    {
        if (Remaining(id) > 0f) return;
        AbilityDef def = AbilityCatalog.Get(id);
        if (def == null) return;
        if (UsedHere(id)) { HudHint.Flash("Already used in this room", 1.5f); return; }

        Vector2 pos = pc.transform.position;
        switch (id)
        {
            case "Nova": DoNova(pos); break;
            case "Life Surge":
                pc.currentHealth = Mathf.Min(pc.maxHealth, pc.currentHealth + 80);
                pc.armor = Mathf.Min(pc.maxArmor, pc.armor + 40);
                Ring(pos, def.color, 5f, 0.5f);
                if (EffectsManager.Instance != null) EffectsManager.Instance.SpawnPickupEffect(pos, def.color);
                break;
            case "Bullet Storm": DoStorm(pc, pos); break;
            case "Time Freeze": DoFreeze(pos, def.color); break;
            case "Guardian Shield":
                pc.invulnerableUntil = Time.time + 5f;
                GameObject aura = new GameObject("ShieldAura");
                aura.transform.SetParent(pc.transform, false);
                SpriteRenderer sr = aura.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteGenerator.CreateCircle(32, new Color(0.5f, 0.6f, 1f, 0.35f));
                sr.sortingOrder = 14;
                aura.transform.localScale = Vector3.one * 2.4f;
                Destroy(aura, 5f);
                break;
            case "Meteor Storm": StartCoroutine(MeteorRoutine(pos)); break;
            case "Annihilation": StartCoroutine(AnnihilateRoutine(pos)); break;
            case "Ascension":
                pc.ascendUntil = Time.time + 10f;
                GameObject halo = new GameObject("AscendAura");
                halo.transform.SetParent(pc.transform, false);
                SpriteRenderer hsr = halo.AddComponent<SpriteRenderer>();
                hsr.sprite = SpriteGenerator.CreateCircle(32, new Color(1f, 0.85f, 0.3f, 0.4f));
                hsr.sortingOrder = 14;
                halo.transform.localScale = Vector3.one * 3f;
                Destroy(halo, 10f);
                Ring(pos, def.color, 6f, 0.5f);
                break;
            case "Overdrive":
                pc.overdriveUntil = Time.time + 6f;
                Ring(pos, def.color, 4f, 0.4f);
                break;
        }

        readyAt[id] = Time.time + def.cooldown;
        usedInRoom[id] = RoomId;
        if (PostProcessEffect.Instance != null)
            PostProcessEffect.Instance.TriggerScreenShake(0.1f, 0.15f);
    }

    void Ring(Vector2 pos, Color c, float radius, float duration)
    {
        GameObject g = new GameObject("AbilityRing");
        g.transform.position = pos;
        SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(32, new Color(c.r, c.g, c.b, 0.5f));
        sr.sortingOrder = 18;
        TimedFx fx = g.AddComponent<TimedFx>();
        fx.duration = duration;
        fx.endScale = radius * 2f;
    }

    System.Collections.IEnumerator MeteorRoutine(Vector2 center)
    {
        for (int i = 0; i < 16; i++)
        {
            Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            Vector2 target;
            if (enemies.Length > 0)
                target = enemies[Random.Range(0, enemies.Length)].transform.position;
            else
                target = center + Random.insideUnitCircle * 6f;
            target += Random.insideUnitCircle * 0.8f;

            GameObject m = new GameObject("Meteor");
            m.AddComponent<MeteorStrike>().Init(target, 260, 2.6f, 0.55f);
            yield return new WaitForSeconds(0.19f);
        }
    }

    System.Collections.IEnumerator AnnihilateRoutine(Vector2 pos)
    {
        Ring(pos, new Color(1f, 0.2f, 0.2f), 16f, 0.7f);
        if (PostProcessEffect.Instance != null) PostProcessEffect.Instance.TriggerScreenShake(0.3f, 0.6f);
        ClearEnemyBullets();

        // убирает только текущую волну: врагов, которые живы сейчас (следующие волны появятся как обычно)
        for (int pass = 0; pass < 1; pass++)
        {
            foreach (Enemy e in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                if (e == null) continue;
                if (e.GetComponent<Boss>() != null)
                {
                    if (pass == 0) e.TakeAbilityDamage(Mathf.RoundToInt(e.maxHealth * 0.3f));
                }
                else
                    e.TakeDamage(9999999);
            }
            yield return new WaitForSeconds(0.35f);
        }
    }

    void ClearEnemyBullets()
    {
        foreach (Bullet b in FindObjectsByType<Bullet>(FindObjectsSortMode.None))
            if (b.isEnemyBullet) Destroy(b.gameObject);
    }

    void DoNova(Vector2 pos)
    {
        Ring(pos, new Color(0.4f, 0.8f, 1f), 7f, 0.4f);
        foreach (Collider2D h in Physics2D.OverlapCircleAll(pos, 7f))
        {
            if (!h.CompareTag("Enemy")) continue;
            Enemy e = h.GetComponent<Enemy>();
            if (e != null) e.TakeAbilityDamage(150, ((Vector2)e.transform.position - pos).normalized);
        }
        ClearEnemyBullets();
    }

    void DoStorm(PlayerController pc, Vector2 pos)
    {
        if (pc.weaponData == null || pc.weaponProfile == null) return;
        for (int i = 0; i < 24; i++)
        {
            float a = i * (360f / 24f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            GameSetup.Instance.SpawnPlayerBullet(pos + dir * 0.6f, dir, pc.EffectiveDamage, pc.weaponData, pc.weaponProfile);
        }
        Ring(pos, new Color(1f, 0.7f, 0.2f), 3f, 0.3f);
    }

    void DoFreeze(Vector2 pos, Color c)
    {
        Ring(pos, c, 9f, 0.5f);
        foreach (Enemy e in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            e.Freeze(4f);
        ClearEnemyBullets();
    }
}
