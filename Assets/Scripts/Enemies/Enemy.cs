using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public string enemyType = "Slime";
    public int maxHealth = 50;
    public int currentHealth;
    public int damage = 10;
    public float moveSpeed = 3f;
    public float attackRange = 1.2f;
    public float attackCooldown = 2f;

    [Header("Ranged")]
    public bool canShoot = false;
    public float shootRange = 8f;
    public float shootCooldown = 2f;
    public int projectilesPerShot = 1;

    [Header("Special")]
    public bool explodeOnDeath = false;
    public float explosionRadius = 3f;
    public bool summonMinions = false;
    public float enemyScale = 0.9f;

    [HideInInspector] public RoomManager roomManager;
    [HideInInspector] public int enemyFloor = 1;
    [HideInInspector] public SpriteRenderer healthBarFill;

    protected Transform player;
    protected float attackTimer;
    protected float shootTimer;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            Attack();
            CirclePlayer();
        }
        else
            ChasePlayer();

        if (canShoot && dist <= shootRange)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0)
            {
                ShootAtPlayer();
                shootTimer = shootCooldown;
            }
        }

        attackTimer -= Time.deltaTime;
    }

    void CirclePlayer()
    {
        float circleSpeed = 2f;
        float angle = Time.time * circleSpeed + GetInstanceID() * 0.1f;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * attackRange * 0.7f;
        Vector2 target = (Vector2)player.position + offset;
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        Vector2 newPos = (Vector2)transform.position + dir * moveSpeed * 0.5f * Time.deltaTime;

        if (roomManager != null && roomManager.roomData != null)
        {
            float hw = 6.5f;
            float hh = 4.5f;
            float cx = roomManager.roomData.worldCenter.x;
            float cy = roomManager.roomData.worldCenter.y;
            newPos.x = Mathf.Clamp(newPos.x, cx - hw, cx + hw);
            newPos.y = Mathf.Clamp(newPos.y, cy - hh, cy + hh);
        }

        transform.position = newPos;
    }

    protected void ChasePlayer()
    {
        Vector2 toPlayer = (player.position - transform.position);
        Vector2 direction = toPlayer.normalized;
        float dist = toPlayer.magnitude;

        GameObject[] others = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject other in others)
        {
            if (other == gameObject) continue;
            float d = Vector2.Distance(transform.position, other.transform.position);
            if (d < 0.8f && d > 0.01f)
            {
                Vector2 pushDir = ((Vector2)transform.position - (Vector2)other.transform.position).normalized;
                direction += pushDir * 0.5f;
            }
        }

        switch (enemyType)
        {
            case "Shooter":
            case "Mage":
            case "Archer":
            case "IceMage":
            case "FireMage":
            case "NecroMage":
            case "Guardian":
                if (dist < 2.5f)
                {
                    Vector2 perpendicular = Vector2.Perpendicular(toPlayer.normalized);
                    direction = perpendicular * Mathf.Sin(Time.time * 3f);
                }
                else if (dist > 8f)
                    direction = toPlayer.normalized;
                else
                    direction = Vector2.Perpendicular(toPlayer.normalized) * Mathf.Sin(Time.time * 4f);
                break;
            case "Speedster":
            case "Wolf":
            case "Snake":
                direction = toPlayer.normalized;
                moveSpeed = 6f;
                break;
            case "Tank":
            case "Golem":
            case "Orc":
                direction = toPlayer.normalized;
                moveSpeed = 1.8f;
                break;
            case "Ghost":
                float ghostAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) + Mathf.Sin(Time.time * 3f) * 1f;
                direction = new Vector2(Mathf.Cos(ghostAngle), Mathf.Sin(ghostAngle));
                break;
            case "Bomber":
                direction = toPlayer.normalized;
                moveSpeed = 5f;
                break;
            case "Spider":
                float spiderAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) + Mathf.Sin(Time.time * 5f) * 0.5f;
                direction = new Vector2(Mathf.Cos(spiderAngle), Mathf.Sin(spiderAngle));
                break;
            case "Bat":
            case "Flyer":
                float flyAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) + Mathf.Cos(Time.time * 2f) * 1.5f;
                direction = new Vector2(Mathf.Cos(flyAngle), Mathf.Sin(flyAngle));
                break;
            case "Skeleton":
            case "Zombie":
            case "DarkKnight":
                direction = toPlayer.normalized;
                break;
            case "Spawner":
                direction = -toPlayer.normalized * 0.5f;
                break;
            default:
                direction = toPlayer.normalized;
                break;
        }

        direction.Normalize();
        Vector2 newPos = (Vector2)transform.position + direction * moveSpeed * Time.deltaTime;

        if (roomManager != null && roomManager.roomData != null)
        {
            float hw = 6f;
            float hh = 4f;
            float cx = roomManager.roomData.worldCenter.x;
            float cy = roomManager.roomData.worldCenter.y;

            float wallMargin = 0.5f;
            float leftWall = cx - hw + wallMargin;
            float rightWall = cx + hw - wallMargin;
            float bottomWall = cy - hh + wallMargin;
            float topWall = cy + hh - wallMargin;

            if (newPos.x < leftWall)
            {
                newPos.x = leftWall;
                direction.x = Mathf.Abs(direction.x) * 0.5f;
            }
            else if (newPos.x > rightWall)
            {
                newPos.x = rightWall;
                direction.x = -Mathf.Abs(direction.x) * 0.5f;
            }

            if (newPos.y < bottomWall)
            {
                newPos.y = bottomWall;
                direction.y = Mathf.Abs(direction.y) * 0.5f;
            }
            else if (newPos.y > topWall)
            {
                newPos.y = topWall;
                direction.y = -Mathf.Abs(direction.y) * 0.5f;
            }
        }

        transform.position = newPos;

        if (spriteRenderer != null && player != null)
            spriteRenderer.flipX = player.position.x < transform.position.x;
    }

    protected void Attack()
    {
        if (attackTimer <= 0)
        {
            attackTimer = attackCooldown;
            if (player != null)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null)
                {
                    switch (enemyType)
                    {
                        case "Bomber":
                        case "LivingBomb":
                            pc.TakeDamage(damage);
                            SpawnExplosion();
                            currentHealth = 0;
                            Die();
                            break;
                        case "Skeleton":
                        case "Zombie":
                            pc.TakeDamage(damage);
                            if (Random.Range(0f, 1f) < 0.3f)
                                pc.TakeDamage(damage / 2);
                            break;
                        case "DarkKnight":
                        case "Orc":
                        case "Golem":
                            pc.TakeDamage(damage);
                            Vector2 kb = ((Vector2)transform.position - (Vector2)player.position).normalized * 3f;
                            pc.transform.position += (Vector3)kb * 0.3f;
                            break;
                        case "Wolf":
                        case "Snake":
                        case "Werewolf":
                            pc.TakeDamage(damage);
                            if (Random.Range(0f, 1f) < 0.4f)
                                pc.TakeDamage(damage);
                            break;
                        case "Imp":
                            pc.TakeDamage(damage);
                            Vector2 impDash = ((Vector2)player.position - (Vector2)transform.position).normalized * 4f;
                            transform.position += (Vector3)impDash;
                            break;
                        case "Berserker":
                            pc.TakeDamage(damage * 2);
                            break;
                        case "Assassin":
                            pc.TakeDamage(damage * 3);
                            Vector2 dashDir = ((Vector2)player.position - (Vector2)transform.position).normalized * 5f;
                            transform.position += (Vector3)dashDir;
                            break;
                        case "Troll":
                            pc.TakeDamage(damage);
                            currentHealth = Mathf.Min(currentHealth + 10, maxHealth);
                            break;
                        case "Abomination":
                            pc.TakeDamage(damage);
                            if (Random.Range(0f, 1f) < 0.5f)
                                pc.TakeDamage(damage);
                            break;
                        case "CrystalGolem":
                            pc.TakeDamage(damage);
                            break;
                        case "FireElemental":
                            pc.TakeDamage(damage);
                            SpawnFireTrail();
                            break;
                        case "Spider":
                            pc.TakeDamage(damage);
                            break;
                        default:
                            pc.TakeDamage(damage);
                            break;
                    }
                }
            }
        }
    }

    void ShootAtPlayer()
    {
        if (player == null) return;
        Vector2 dir = (player.position - transform.position).normalized;

        switch (enemyType)
        {
            case "FireMage":
            case "FireElemental":
                for (int i = 0; i < 3; i++)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + (i - 1) * 20f;
                    Vector2 fireDir = Quaternion.Euler(0, 0, angle - Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg) * dir;
                    GameSetup.Instance.SpawnEnemyBullet(transform.position, fireDir, damage / 2);
                }
                break;
            case "IceMage":
                Vector2 iceDir = dir;
                GameSetup.Instance.SpawnEnemyBullet(transform.position, iceDir, damage / 3);
                GameSetup.Instance.SpawnEnemyBullet(transform.position, Quaternion.Euler(0, 0, 15f) * iceDir, damage / 3);
                GameSetup.Instance.SpawnEnemyBullet(transform.position, Quaternion.Euler(0, 0, -15f) * iceDir, damage / 3);
                break;
            case "StormMage":
                for (int i = 0; i < 7; i++)
                {
                    float a = (360f / 7) * i * Mathf.Deg2Rad;
                    Vector2 stormDir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    GameSetup.Instance.SpawnEnemyBullet(transform.position, stormDir, damage / 4);
                }
                break;
            case "NecroMage":
            case "Lich":
                for (int i = 0; i < 5; i++)
                {
                    float a = (360f / 5) * i * Mathf.Deg2Rad;
                    Vector2 ringDir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    GameSetup.Instance.SpawnEnemyBullet(transform.position, ringDir, damage / 3);
                }
                break;
            case "Warlock":
                for (int i = 0; i < 3; i++)
                {
                    float spread = (i - 1) * 0.3f;
                    Vector2 warlockDir = Quaternion.Euler(0, 0, spread * Mathf.Rad2Deg) * dir;
                    GameSetup.Instance.SpawnEnemyBullet(transform.position, warlockDir, damage / 2);
                }
                break;
            case "Wraith":
            case "Shadow":
                Vector2 shadowDir = dir;
                GameSetup.Instance.SpawnEnemyBullet(transform.position, shadowDir, damage);
                break;
            case "Nightmare":
                for (int i = 0; i < 4; i++)
                {
                    float a = (360f / 4) * i * Mathf.Deg2Rad + Time.time;
                    Vector2 nightmareDir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    GameSetup.Instance.SpawnEnemyBullet(transform.position, nightmareDir, damage / 3);
                }
                break;
            case "Harpy":
                for (int i = 0; i < 2; i++)
                {
                    float spread = (i == 0 ? -0.2f : 0.2f);
                    Vector2 harpyDir = Quaternion.Euler(0, 0, spread * Mathf.Rad2Deg) * dir;
                    GameSetup.Instance.SpawnEnemyBullet(transform.position, harpyDir, damage / 2);
                }
                break;
            case "Archer":
                GameSetup.Instance.SpawnEnemyBullet(transform.position, dir, damage / 2);
                break;
            default:
                for (int i = 0; i < projectilesPerShot; i++)
                {
                    float spread = Random.Range(-0.2f, 0.2f);
                    Vector2 shootDir = Quaternion.Euler(0, 0, spread) * dir;
                    GameSetup.Instance.SpawnEnemyBullet(transform.position, shootDir, damage / 2);
                }
                break;
        }
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            Invoke("ResetColor", 0.1f);
        }

        Vector2 knockback = ((Vector2)transform.position - (PlayerController.Instance != null ? (Vector2)PlayerController.Instance.transform.position : Vector2.zero)).normalized * 2f;
        transform.position += (Vector3)knockback;

        if (healthBarFill != null)
        {
            float ratio = (float)currentHealth / maxHealth;
            healthBarFill.transform.localScale = new Vector3(ratio, 1f, 1f);
            healthBarFill.transform.localPosition = new Vector3(-(1f - ratio) * 0.4f, 0, 0);
        }

        if (EffectsManager.Instance != null)
            EffectsManager.Instance.SpawnHitEffect(transform.position);

        if (currentHealth <= 0)
            Die();
    }

    void ResetColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    void SpawnExplosion()
    {
        if (EffectsManager.Instance != null)
            EffectsManager.Instance.SpawnDeathEffect(transform.position);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController pc = hit.GetComponent<PlayerController>();
                if (pc != null)
                    pc.TakeDamage(damage);
            }
        }
    }

    void SpawnFireTrail()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject fire = new GameObject("FireTrail");
            fire.transform.position = (Vector3)(Vector2)transform.position + (Vector3)(Random.insideUnitCircle * 0.5f);
            SpriteRenderer sr = fire.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateCircle(8, new Color(1f, 0.4f, 0.05f, 0.8f));
            sr.sortingOrder = 15;
            fire.transform.localScale = Vector3.one * Random.Range(0.1f, 0.2f);

            ParticleMover pm = fire.AddComponent<ParticleMover>();
            pm.velocity = Random.insideUnitCircle * 2f;
            pm.lifetime = Random.Range(0.5f, 1f);
            pm.shrink = true;
        }
    }

    void Die()
    {
        if (EffectsManager.Instance != null)
            EffectsManager.Instance.SpawnDeathEffect(transform.position);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDeathSound();
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(100);

        bool isBoss = GetComponent<Boss>() != null;

        if (explodeOnDeath)
            Explode();

        if (summonMinions)
            SummonMinions();

        DropLoot();

        if (roomManager != null)
            roomManager.EnemyDefeated();

        if (isBoss)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayDeathSound();
            if (ProceduralMusic.Instance != null)
            {
                ProceduralMusic.Instance.PlaySFX("bossDeath");
                ProceduralMusic.Instance.StartVictoryMusic();
            }
            if (GameOverUI.Instance != null)
                GameOverUI.Instance.ShowWin();
        }

        Destroy(gameObject);
    }

    void DropLoot()
    {
        float roll = Random.Range(0f, 1f);

        if (enemyType == "Tank" || enemyType == "DarkKnight")
        {
            if (roll < 0.4f) SpawnDrop("health");
            else if (roll < 0.55f) SpawnDrop("energy");
            else if (roll < 0.65f) SpawnDrop("weapon");
        }
        else if (roll < 0.25f)
            SpawnDrop("health");
        else if (roll < 0.45f)
            SpawnDrop("energy");
        else if (roll < 0.5f)
            SpawnDrop("weapon");

        SpawnGold();
    }

    void SpawnDrop(string type)
    {
        Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * 0.5f;
        GameObject obj = new GameObject("Drop_" + type);
        obj.transform.position = pos;
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 8;
        obj.AddComponent<BoxCollider2D>().isTrigger = true;

        switch (type)
        {
            case "weapon":
                sr.sprite = SpriteGenerator.CreateDropWeapon(16);
                obj.AddComponent<WeaponPickup>();
                break;
            case "health":
                sr.sprite = SpriteGenerator.CreateDropHealth(16);
                PowerUp pu = obj.AddComponent<PowerUp>();
                pu.type = PowerUp.PowerUpType.HealthRestore;
                break;
            case "energy":
                sr.sprite = SpriteGenerator.CreateDropEnergy(16);
                PowerUp pe = obj.AddComponent<PowerUp>();
                pe.type = PowerUp.PowerUpType.EnergyRestore;
                break;
        }
    }

    void SpawnGold()
    {
        int goldTotal = 10;
        int coinCount = 20;

        if (enemyType == "Tank" || enemyType == "DarkKnight" || enemyType == "Orc" || enemyType == "Golem")
        {
            goldTotal = 30;
            coinCount = 30;
        }
        else if (enemyType == "Bomber" || enemyType == "Spawner" || enemyType == "Guardian")
        {
            goldTotal = 25;
            coinCount = 25;
        }
        else if (enemyType == "Speedster" || enemyType == "Bat" || enemyType == "Spider")
        {
            goldTotal = 8;
            coinCount = 15;
        }

        if (enemyType == "CrownedBoar" || enemyType == "Dragon" || enemyType == "Necromancer")
        {
            goldTotal = 500;
            coinCount = 80;
        }

        for (int i = 0; i < coinCount; i++)
        {
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * 2f;
            GameObject obj = new GameObject("GoldCoin");
            obj.transform.position = pos;
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateCoin(16);
            sr.sortingOrder = 8;
            obj.transform.localScale = Vector3.one * 0.8f;
            obj.AddComponent<BoxCollider2D>().isTrigger = true;
            GoldPickup gp = obj.AddComponent<GoldPickup>();
            gp.goldValue = Mathf.Max(1, goldTotal / coinCount);
        }
    }

    void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController pc = hit.GetComponent<PlayerController>();
                if (pc != null) pc.TakeDamage(damage);
            }
        }

        if (EffectsManager.Instance != null)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * explosionRadius;
                EffectsManager.Instance.SpawnDeathEffect(pos);
            }
        }
    }

    void SummonMinions()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * 2f;
            if (roomManager != null)
            {
                roomManager.enemiesAlive++;
                roomManager.SpawnEnemy(pos, "Slime", enemyFloor);
            }
        }
    }
}
