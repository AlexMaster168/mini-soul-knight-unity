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
                if (dist < 3f)
                    direction = -direction;
                else if (dist > 7f)
                    direction = toPlayer.normalized;
                else
                    direction = Vector2.Perpendicular(direction) * Mathf.Sin(Time.time * 4f);
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
            float hw = 6.5f;
            float hh = 4.5f;
            float cx = roomManager.roomData.worldCenter.x;
            float cy = roomManager.roomData.worldCenter.y;
            newPos.x = Mathf.Clamp(newPos.x, cx - hw, cx + hw);
            newPos.y = Mathf.Clamp(newPos.y, cy - hh, cy + hh);
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
                            pc.TakeDamage(damage);
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
                            pc.TakeDamage(damage);
                            if (Random.Range(0f, 1f) < 0.4f)
                                pc.TakeDamage(damage);
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
            case "NecroMage":
                for (int i = 0; i < 5; i++)
                {
                    float a = (360f / 5) * i * Mathf.Deg2Rad;
                    Vector2 ringDir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    GameSetup.Instance.SpawnEnemyBullet(transform.position, ringDir, damage / 3);
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

    void Die()
    {
        if (EffectsManager.Instance != null)
            EffectsManager.Instance.SpawnDeathEffect(transform.position);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDeathSound();
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(100);

        if (explodeOnDeath)
            Explode();

        if (summonMinions)
            SummonMinions();

        DropLoot();

        if (roomManager != null)
            roomManager.EnemyDefeated();

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
                sr.sprite = SpriteGenerator.CreateSquare(16, new Color(0.9f, 0.8f, 0.1f));
                obj.AddComponent<WeaponPickup>();
                break;
            case "health":
                sr.sprite = SpriteGenerator.CreateCircle(16, new Color(0.2f, 0.9f, 0.2f));
                PowerUp pu = obj.AddComponent<PowerUp>();
                pu.type = PowerUp.PowerUpType.HealthRestore;
                break;
            case "energy":
                sr.sprite = SpriteGenerator.CreateCircle(16, new Color(0.2f, 0.5f, 1f));
                PowerUp pe = obj.AddComponent<PowerUp>();
                pe.type = PowerUp.PowerUpType.EnergyRestore;
                break;
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
