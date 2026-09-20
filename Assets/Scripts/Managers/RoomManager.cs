using UnityEngine;
using System.Collections.Generic;

// Бой в одной комнате: волны врагов, запирание дверей, открытие после победы
public class RoomManager : MonoBehaviour
{
    public DungeonGenerator.RoomData roomData;
    public int enemiesAlive = 0;

    private bool started;
    private int wavesLeft;
    private int floor = 1;
    private bool waitingNextWave;
    private float nextWaveTimer;

    static readonly string[] floor1 =
    {
        "Slime", "Slime", "Goblin", "Goblin", "Bat", "Spider", "Imp", "Skeleton", "Zombie", "Wolf",
        "Snake", "Shooter", "BigSlime", "Charger", "Turret"
    };
    static readonly string[] floor2 =
    {
        "Skeleton", "Wolf", "Archer", "Zombie", "Shaman", "ShieldKnight", "Blinker", "Sentry", "Bomber",
        "IceMage", "Speedster", "Werewolf", "Mortar", "Charger", "Ghost", "Orc", "Turret", "BigSlime",
        "Harpy", "Ninja", "Spider", "Snake", "Mage"
    };
    static readonly string[] floor3 =
    {
        "DarkKnight", "FireMage", "NecroMage", "Golem", "Tank", "Ninja", "Mortar", "Marksman", "StormMage",
        "Wraith", "Warlock", "Lich", "Abomination", "LivingBomb", "Guardian", "Nightmare", "Berserker",
        "Troll", "Sentry", "Blinker", "ShieldKnight", "Shaman", "CrystalGolem", "FireElemental", "Assassin",
        "Charger", "Turret", "Shadow"
    };

    static readonly string[] bossPerFloor = { "CrownedBoar", "Necromancer", "Dragon" };

    public bool IsStarted { get { return started; } }

    // Игрок зашёл в комнату
    public void BeginEncounter(int currentFloor)
    {
        if (started || roomData.cleared) return;
        started = true;
        floor = currentFloor;

        roomData.SetDoors(true);

        if (roomData.isBossRoom)
        {
            wavesLeft = 0;
            SpawnBoss();
            return;
        }

        if (floor <= 1) wavesLeft = Random.value < 0.3f ? 2 : 1;
        else if (floor == 2) wavesLeft = Random.Range(1, 3);
        else wavesLeft = Random.Range(2, 4);

        SpawnWave();
    }

    void SpawnWave()
    {
        waitingNextWave = false;
        wavesLeft--;

        string[] pool = floor <= 1 ? floor1 : floor == 2 ? floor2 : floor3;
        int count = 2 + floor + Random.Range(0, 2);

        List<Vector2> used = new List<Vector2>();
        int stationary = 0;
        enemiesAlive += count;

        for (int i = 0; i < count; i++)
        {
            string type = pool[Random.Range(0, pool.Length)];
            // не больше двух неподвижных стрелков в волне
            bool isStatic = type == "Turret" || type == "Sentry" || type == "Mortar" || type == "Blinker";
            if (isStatic && stationary >= 2) type = pool[0];
            if (isStatic) stationary++;

            Vector2 pos = FindSpawnPoint(used);
            used.Add(pos);
            SpawnEnemy(pos, type, floor);
        }
    }

    Vector2 FindSpawnPoint(List<Vector2> used)
    {
        Vector2 center = roomData.worldCenter;
        Vector2 playerPos = PlayerController.Instance != null ? (Vector2)PlayerController.Instance.transform.position : center;
        Vector2 best = center;
        float bestScore = -1f;

        for (int attempt = 0; attempt < 14; attempt++)
        {
            Vector2 c = center + new Vector2(Random.Range(-5.6f, 5.6f), Random.Range(-3.6f, 3.6f));
            float score = Vector2.Distance(c, playerPos);
            foreach (Vector2 u in used)
                if (Vector2.Distance(c, u) < 1.5f) score -= 3f;
            if (score > bestScore) { bestScore = score; best = c; }
            if (score >= 5f) break;
        }
        return best;
    }

    void SpawnBoss()
    {
        string type = bossPerFloor[Mathf.Clamp(floor - 1, 0, bossPerFloor.Length - 1)];
        Vector2 pos = (Vector2)roomData.worldCenter + new Vector2(0, 1.5f);
        enemiesAlive = 1;
        SpawnEnemy(pos, type, floor);

        FloorTransition.Get().Banner("BOSS", BossTitle(type), 2.2f);
    }

    static string BossTitle(string type)
    {
        switch (type)
        {
            case "CrownedBoar": return "The Crowned Boar";
            case "Necromancer": return "The Necromancer";
            case "Dragon": return "Ancient Dragon";
            default: return type;
        }
    }

    void Update()
    {
        if (waitingNextWave)
        {
            nextWaveTimer -= Time.deltaTime;
            if (nextWaveTimer <= 0f) SpawnWave();
        }
    }

    public void EnemyDefeated()
    {
        enemiesAlive--;
        if (enemiesAlive > 0) return;

        if (wavesLeft > 0)
        {
            waitingNextWave = true;
            nextWaveTimer = 1.2f;
            return;
        }

        roomData.cleared = true;
        roomData.SetDoors(false);
        DungeonGenerator.Instance.RoomCleared(roomData);
    }

    public void SpawnEnemy(Vector2 position, string type, int floor)
    {
        GameObject enemy = new GameObject("Enemy_" + type);
        enemy.tag = "Enemy";
        enemy.transform.position = position;

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        if (type == "Turret" || type == "Sentry" || type == "Blinker")
            rb.bodyType = RigidbodyType2D.Kinematic;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.8f);

        EnemyData data;
        bool isBoss = GameData.Bosses.ContainsKey(type);
        if (GameData.Enemies.ContainsKey(type))
            data = GameData.Enemies[type];
        else if (isBoss)
            data = GameData.Bosses[type];
        else
            data = GameData.Enemies["Slime"];

        // способность нужно добавить до Enemy.Start
        if (HasAbility(type))
        {
            EnemyAbility ab = enemy.AddComponent<EnemyAbility>();
            ab.kind = type;
        }

        Enemy e = enemy.AddComponent<Enemy>();
        e.roomManager = this;

        float hpMul = isBoss ? 1f : 1f + 0.3f * (floor - 1);
        e.enemyType = type;
        e.maxHealth = Mathf.RoundToInt(data.health * hpMul);
        e.currentHealth = e.maxHealth;
        e.damage = data.damage + floor;
        e.moveSpeed = data.speed + (data.speed > 0f ? floor * 0.05f : 0f);
        e.attackRange = data.attackRange;
        e.attackCooldown = data.attackCooldown;
        e.canShoot = data.canShoot;
        e.shootRange = data.shootRange;
        e.shootCooldown = data.shootCooldown;
        e.projectilesPerShot = data.projectiles;
        e.explodeOnDeath = data.explodeOnDeath;
        e.explosionRadius = data.explosionRadius;
        e.summonMinions = data.summonMinions;
        e.enemyScale = data.scale;
        e.enemyFloor = floor;

        enemy.transform.localScale = Vector3.one * data.scale;

        Color tint;
        sr.sprite = PixelArt.Enemy(type, out tint);
        sr.color = tint;

        GameObject shadow = new GameObject("Shadow");
        shadow.transform.SetParent(enemy.transform);
        shadow.transform.localPosition = new Vector3(0, -0.4f, 0);
        SpriteRenderer shadowSr = shadow.AddComponent<SpriteRenderer>();
        shadowSr.sprite = SpriteGenerator.CreateCircle(12, new Color(0, 0, 0, 0.25f));
        shadowSr.sortingOrder = -1;
        shadow.transform.localScale = new Vector3(1f, 0.35f, 1f);

        if (isBoss)
            enemy.AddComponent<Boss>();

        CreateEnemyHealthBar(enemy.transform, e);
    }

    static bool HasAbility(string type)
    {
        switch (type)
        {
            case "Turret": case "Sentry": case "Charger": case "BigSlime": case "Shaman": case "Blinker":
            case "ShieldKnight": case "Mortar": case "Marksman": case "Ninja":
                return true;
            default: return false;
        }
    }

    void CreateEnemyHealthBar(Transform parent, Enemy e)
    {
        GameObject barBg = new GameObject("HealthBarBg");
        barBg.transform.SetParent(parent);
        barBg.transform.localPosition = new Vector3(0, 0.7f, 0);
        SpriteRenderer bgSr = barBg.AddComponent<SpriteRenderer>();
        bgSr.sprite = SpriteGenerator.CreateSquare(16, new Color(0.2f, 0.05f, 0.05f, 0.9f));
        bgSr.sortingOrder = 20;
        barBg.transform.localScale = new Vector3(0.8f, 0.12f, 1f);

        GameObject barFill = new GameObject("HealthBarFill");
        barFill.transform.SetParent(barBg.transform);
        barFill.transform.localPosition = Vector3.zero;
        barFill.transform.localScale = Vector3.one;
        SpriteRenderer fillSr = barFill.AddComponent<SpriteRenderer>();
        fillSr.sprite = SpriteGenerator.CreateSquare(16, new Color(0.8f, 0.15f, 0.15f));
        fillSr.sortingOrder = 21;

        e.healthBarFill = fillSr;
    }
}
