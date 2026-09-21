using System.Collections.Generic;
using UnityEngine;

// ================= Постоянные улучшения (до конца забега) =================

public class UpgradeDef
{
    public string id, description;
    public int maxLevel, basePrice, step;
    public int freeLevels;   // сколько уровней доступно до 3 этажа (остальные открываются с 3 этажа)
    public Color color;

    public UpgradeDef(string id, int maxLevel, int freeLevels, int basePrice, int step, Color color, string description)
    {
        this.freeLevels = freeLevels;
        this.id = id;
        this.maxLevel = maxLevel;
        this.basePrice = basePrice;
        this.step = step;
        this.color = color;
        this.description = description;
    }

    public int Price(int currentLevel) { return basePrice + step * currentLevel; }
}

public static class UpgradeCatalog
{
    public const int DeepFloor = 3;   // с этого этажа открываются новые улучшения и высокие уровни

    public static readonly UpgradeDef[] All =
    {
        new UpgradeDef("Armor Plating", 10, 5, 200, 100, new Color(0.4f, 0.7f, 1f),
            "Reinforced protection. Each level: +30 max armor (filled instantly) and -6% damage from every hit."),
        new UpgradeDef("Vitality", 10, 5, 150, 100, new Color(0.35f, 0.9f, 0.4f),
            "Each level: +30 max HP and heals you for 30."),
        new UpgradeDef("Firepower", 10, 5, 250, 150, new Color(1f, 0.55f, 0.2f),
            "Each level: +15% damage with every weapon, robots included."),
        new UpgradeDef("Energy Core", 8, 0, 180, 90, new Color(0.3f, 0.6f, 1f),
            "Each level: +60 max energy and restores 60 energy."),
        new UpgradeDef("Recharge", 5, 0, 200, 100, new Color(0.5f, 0.9f, 1f),
            "Each level: +1.5 energy regeneration per second."),
        new UpgradeDef("Swiftness", 5, 0, 200, 100, new Color(1f, 0.95f, 0.4f),
            "Each level: +5% movement speed."),
        new UpgradeDef("Dash Drive", 5, 0, 220, 110, new Color(0.8f, 0.5f, 1f),
            "Each level: the dash recharges 8% faster."),
    };

    public static UpgradeDef Get(string id)
    {
        foreach (UpgradeDef u in All)
            if (u.id == id) return u;
        return null;
    }
}

public class PlayerUpgrades : MonoBehaviour
{
    public static PlayerUpgrades Instance;
    private readonly Dictionary<string, int> levels = new Dictionary<string, int>();

    void Awake() { Instance = this; }

    public int Level(string id)
    {
        int l;
        return levels.TryGetValue(id, out l) ? l : 0;
    }

    // Применяет следующий уровень (золото списывает магазин)
    public void Apply(string id)
    {
        PlayerController pc = PlayerController.Instance;
        UpgradeDef def = UpgradeCatalog.Get(id);
        if (pc == null || def == null || Level(id) >= def.maxLevel) return;
        levels[id] = Level(id) + 1;

        switch (id)
        {
            case "Armor Plating":
                pc.maxArmor += 30;
                pc.armor = Mathf.Min(pc.maxArmor, pc.armor + 30);
                pc.damageReduction = Mathf.Min(0.6f, pc.damageReduction + 0.06f);
                break;
            case "Vitality":
                pc.maxHealth += 30;
                pc.currentHealth = Mathf.Min(pc.maxHealth, pc.currentHealth + 30);
                break;
            case "Firepower":
                pc.damageMultiplier += 0.15f;
                break;
            case "Energy Core":
                pc.maxEnergy += 60;
                pc.currentEnergy = Mathf.Min(pc.maxEnergy, pc.currentEnergy + 60);
                break;
            case "Recharge":
                pc.energyRegenRate += 1.5f;
                break;
            case "Swiftness":
                pc.moveSpeed *= 1.05f;
                break;
            case "Dash Drive":
                pc.dashCooldown *= 0.92f;
                break;
        }
    }
}

// ================= Роботы-помощники (продаются с 3 этажа) =================

public class RobotDef
{
    public string id, description;
    public int price, damage;
    public float cooldown, range, bulletSpeed, bulletSize;
    public bool pierce;
    public BulletStyle style;
    public Color color;

    public RobotDef(string id, int price, int damage, float cooldown, float range, float bulletSpeed, float bulletSize,
                    bool pierce, BulletStyle style, Color color, string description)
    {
        this.id = id; this.price = price; this.damage = damage; this.cooldown = cooldown; this.range = range;
        this.bulletSpeed = bulletSpeed; this.bulletSize = bulletSize; this.pierce = pierce; this.style = style;
        this.color = color; this.description = description;
    }
}

public static class RobotCatalog
{
    public const int MinFloor = 3;

    public static readonly RobotDef[] All =
    {
        new RobotDef("Scout Bot", 400, 14, 0.45f, 11f, 20f, 0.5f, false, BulletStyle.Slug, new Color(0.4f, 0.8f, 1f),
            "Small hover drone. Follows you and automatically shoots the nearest enemy."),
        new RobotDef("Gunner Bot", 650, 20, 0.2f, 12f, 24f, 0.5f, false, BulletStyle.Streak, new Color(1f, 0.7f, 0.25f),
            "Rapid-fire turret drone. Melts anything that gets close to you."),
        new RobotDef("Plasma Bot", 900, 60, 0.8f, 13f, 16f, 0.7f, true, BulletStyle.Orb, new Color(0.7f, 0.35f, 1f),
            "Heavy drone. Its plasma orbs pierce through whole lines of enemies."),
    };

    public static RobotDef Get(string id)
    {
        foreach (RobotDef r in All)
            if (r.id == id) return r;
        return null;
    }
}

public class PlayerRobots : MonoBehaviour
{
    public static PlayerRobots Instance;
    public List<string> owned = new List<string>();

    void Awake() { Instance = this; }

    public bool Owns(string id) { return owned.Contains(id); }

    public void Add(string id)
    {
        RobotDef def = RobotCatalog.Get(id);
        if (def == null || owned.Contains(id)) return;
        owned.Add(id);

        GameObject go = new GameObject("Robot_" + id);
        go.transform.position = transform.position;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArt.Robot(def.color);
        sr.sortingOrder = 11;
        go.transform.localScale = Vector3.one * 0.95f;
        CompanionRobot r = go.AddComponent<CompanionRobot>();
        r.def = def;
        r.slot = owned.Count - 1;
    }
}

// Летает рядом с игроком и сам стреляет по ближайшему врагу
public class CompanionRobot : MonoBehaviour
{
    public RobotDef def;
    public int slot;

    private SpriteRenderer sr;
    private float fireTimer, scanTimer;
    private Transform target;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        fireTimer = Random.Range(0f, def.cooldown);
    }

    void Update()
    {
        PlayerController pc = PlayerController.Instance;
        if (pc == null || def == null) return;
        float dt = Time.deltaTime;

        float ang = Time.time * 0.9f + slot * 2.1f;
        Vector2 desired = (Vector2)pc.transform.position + new Vector2(Mathf.Cos(ang) * 1.9f, Mathf.Sin(ang) * 1.2f + 0.6f);
        Vector2 pos = transform.position;
        if (Vector2.Distance(pos, desired) > 12f) pos = desired;
        pos = Vector2.Lerp(pos, desired, 1f - Mathf.Exp(-6f * dt));
        pos.y += Mathf.Sin(Time.time * 5f + slot) * 0.15f * dt * 6f;
        transform.position = pos;

        scanTimer -= dt;
        if (scanTimer <= 0f || target == null)
        {
            scanTimer = 0.15f;
            target = FindTarget();
        }

        fireTimer -= dt;
        if (target != null)
        {
            Vector2 to = (Vector2)target.position - (Vector2)transform.position;
            if (sr != null) sr.flipX = to.x < 0f;
            if (fireTimer <= 0f)
            {
                fireTimer = def.cooldown;
                Fire(to.normalized, pc);
            }
        }
    }

    Transform FindTarget()
    {
        Transform best = null;
        float bestD = def.range;
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Enemy e = g.GetComponent<Enemy>();
            if (e == null || e.IsSpawning) continue;
            float d = Vector2.Distance(transform.position, g.transform.position);
            if (d < bestD) { bestD = d; best = g.transform; }
        }
        return best;
    }

    void Fire(Vector2 dir, PlayerController pc)
    {
        int dmg = Mathf.Max(1, Mathf.RoundToInt(def.damage * pc.damageMultiplier));
        GameSetup.Instance.SpawnRobotBullet((Vector2)transform.position + dir * 0.5f, dir, dmg, def.color,
                                            def.bulletSpeed, def.bulletSize, def.style, def.pierce);

        GameObject f = new GameObject("RobotFlash");
        f.transform.position = (Vector2)transform.position + dir * 0.6f;
        SpriteRenderer fsr = f.AddComponent<SpriteRenderer>();
        fsr.sprite = SpriteGenerator.CreateCircle(8, def.color);
        fsr.sortingOrder = 15;
        f.transform.localScale = Vector3.one * 0.18f;
        ParticleMover pm = f.AddComponent<ParticleMover>();
        pm.velocity = dir * 2f;
        pm.lifetime = 0.1f;
        pm.shrink = true;
    }
}

// Метеор с маркером на земле (способность Meteor Storm)
public class MeteorStrike : MonoBehaviour
{
    private Vector2 pos;
    private int damage;
    private float radius, delay, t;

    public void Init(Vector2 pos, int damage, float radius, float delay)
    {
        this.pos = pos;
        this.damage = damage;
        this.radius = radius;
        this.delay = delay;
        transform.position = pos;

        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(32, new Color(1f, 0.3f, 0.1f, 0.35f));
        sr.sortingOrder = 3;
        transform.localScale = Vector3.one * radius * 2f;
    }

    void Update()
    {
        t += Time.deltaTime;
        if (t < delay) return;

        foreach (Collider2D h in Physics2D.OverlapCircleAll(pos, radius))
        {
            if (!h.CompareTag("Enemy")) continue;
            Enemy e = h.GetComponent<Enemy>();
            if (e != null) e.TakeAbilityDamage(damage, ((Vector2)e.transform.position - pos).normalized);
        }

        GameObject ring = new GameObject("MeteorBlast");
        ring.transform.position = pos;
        SpriteRenderer r = ring.AddComponent<SpriteRenderer>();
        r.sprite = SpriteGenerator.CreateCircle(32, new Color(1f, 0.6f, 0.15f, 0.8f));
        r.sortingOrder = 18;
        TimedFx fx = ring.AddComponent<TimedFx>();
        fx.duration = 0.35f;
        fx.endScale = radius * 2.3f;

        if (EffectsManager.Instance != null) EffectsManager.Instance.SpawnDeathEffect(pos);
        if (PostProcessEffect.Instance != null) PostProcessEffect.Instance.TriggerScreenShake(0.1f, 0.12f);
        Destroy(gameObject);
    }
}
