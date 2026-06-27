using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    public DungeonGenerator.RoomData roomData;
    public int enemiesAlive = 0;
    private bool enemiesSpawned;
    private static bool transitioning;

    public void SpawnEnemies(int floor)
    {
        if (enemiesSpawned) return;
        enemiesSpawned = true;

        int count;
        if (roomData.isBossRoom)
        {
            SpawnBoss(floor);
            return;
        }

        count = roomData.isStartRoom ? 2 : Mathf.Min(2 + floor * 2, 10);

        string[] floor1 = { "Slime", "Goblin", "Bat", "Spider" };
        string[] floor2 = { "Slime", "Goblin", "Skeleton", "Wolf", "Bat", "Speedster" };
        string[] floor3 = { "Skeleton", "Shooter", "Archer", "Ghost", "Zombie", "Spider", "Snake" };
        string[] floor4 = { "Shooter", "Mage", "DarkKnight", "Bomber", "Flyer", "Orc", "IceMage" };
        string[] floor5 = { "DarkKnight", "FireMage", "NecroMage", "Tank", "Guardian", "Spawner", "Golem" };

        string[] pool = floor <= 1 ? floor1 : floor == 2 ? floor2 : floor == 3 ? floor3 : floor == 4 ? floor4 : floor5;

        enemiesAlive = count;
        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i * Mathf.Deg2Rad;
            float dist = 2.5f + Random.Range(0f, 2f);
            Vector2 pos = (Vector2)roomData.worldCenter + new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
            string type = pool[Random.Range(0, pool.Length)];
            SpawnEnemy(pos, type, floor);
        }
    }

    void SpawnBoss(int floor)
    {
        string[] bossTypes = { "CrownedBoar", "Dragon", "Necromancer" };
        string type = bossTypes[Random.Range(0, bossTypes.Length)];
        Vector2 pos = (Vector2)roomData.worldCenter + new Vector2(0, 2);
        enemiesAlive = 1;
        SpawnEnemy(pos, type, floor);
    }

    void Update()
    {
        if (transitioning) return;

        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        DungeonGenerator gen = DungeonGenerator.Instance;
        if (gen == null) return;

        if (gen.GetCurrentRoom() != roomData) return;

        float dist = Vector2.Distance(player.transform.position, roomData.worldCenter);
        if (dist > 7f && roomData.cleared)
        {
            transitioning = true;
            gen.FindAndEnterNearestRoom(player.transform.position);
            StartCoroutine(ResetTransition());
        }
    }

    System.Collections.IEnumerator ResetTransition()
    {
        yield return new WaitForSeconds(0.3f);
        transitioning = false;
    }

    public void EnemyDefeated()
    {
        enemiesAlive--;
        if (enemiesAlive <= 0)
        {
            roomData.cleared = true;
            DungeonGenerator.Instance.RoomCleared();
        }
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

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.8f);

        Enemy e = enemy.AddComponent<Enemy>();
        e.roomManager = this;

        EnemyData data;
        if (GameData.Enemies.ContainsKey(type))
            data = GameData.Enemies[type];
        else if (GameData.Bosses.ContainsKey(type))
            data = GameData.Bosses[type];
        else
            data = GameData.Enemies["Slime"];

        e.enemyType = type;
        e.maxHealth = data.health + floor * 3;
        e.currentHealth = e.maxHealth;
        e.damage = data.damage + floor;
        e.moveSpeed = data.speed + floor * 0.05f;
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
        sr.sprite = GetEnemySprite(type);

        if (GameData.Bosses.ContainsKey(type))
            enemy.AddComponent<Boss>();

        CreateEnemyHealthBar(enemy.transform, e);
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

    Sprite GetEnemySprite(string type)
    {
        switch (type)
        {
            case "Slime": return SpriteGenerator.CreateSlime(32, new Color(0.9f, 0.2f, 0.2f));
            case "Goblin": return SpriteGenerator.CreateSlime(32, new Color(0.2f, 0.8f, 0.2f));
            case "Skeleton": return SpriteGenerator.CreateSkeleton(32);
            case "Zombie": return SpriteGenerator.CreateSlime(32, new Color(0.3f, 0.6f, 0.3f));
            case "Wolf": return SpriteGenerator.CreateSlime(32, new Color(0.5f, 0.4f, 0.3f));
            case "Bat": return SpriteGenerator.CreateFlyer(32);
            case "DarkKnight": return SpriteGenerator.CreateDarkKnight(32);
            case "Orc": return SpriteGenerator.CreateTank(32);
            case "Golem": return SpriteGenerator.CreateTank(32);
            case "Shooter": return SpriteGenerator.CreateShooter(32);
            case "Mage": return SpriteGenerator.CreateMage(32);
            case "Archer": return SpriteGenerator.CreateShooter(32);
            case "IceMage": return SpriteGenerator.CreateMage(32);
            case "FireMage": return SpriteGenerator.CreateMage(32);
            case "NecroMage": return SpriteGenerator.CreateMage(32);
            case "Ghost": return SpriteGenerator.CreateGhost(32);
            case "Bomber": return SpriteGenerator.CreateBomber(32);
            case "Tank": return SpriteGenerator.CreateTank(32);
            case "Speedster": return SpriteGenerator.CreateSlime(32, new Color(0.2f, 0.8f, 0.9f));
            case "Flyer": return SpriteGenerator.CreateFlyer(32);
            case "Spider": return SpriteGenerator.CreateSlime(32, new Color(0.2f, 0.2f, 0.2f));
            case "Snake": return SpriteGenerator.CreateSlime(32, new Color(0.4f, 0.7f, 0.1f));
            case "Spawner": return SpriteGenerator.CreateBoss(48, new Color(0.6f, 0.2f, 0.6f));
            case "Guardian": return SpriteGenerator.CreateDarkKnight(48);
            case "CrownedBoar": return SpriteGenerator.CreateBoss(64, new Color(0.8f, 0.4f, 0.1f));
            case "Dragon": return SpriteGenerator.CreateBoss(64, new Color(0.8f, 0.1f, 0.1f));
            case "Necromancer": return SpriteGenerator.CreateBoss(64, new Color(0.5f, 0.1f, 0.8f));
            default: return SpriteGenerator.CreateSlime(32, new Color(0.9f, 0.2f, 0.2f));
        }
    }
}
