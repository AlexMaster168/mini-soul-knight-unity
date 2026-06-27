using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Health")]
    public int maxHealth = 150;
    public int currentHealth;

    [Header("Energy")]
    public int maxEnergy = 200;
    public int currentEnergy;
    public float energyRegenRate = 5f;

    [Header("Attack")]
    public int attackDamage = 25;
    public float attackCooldown = 0.12f;
    public int projectilesPerShot = 1;
    public int energyCost = 3;

    [Header("Armor")]
    public int armor = 0;
    public int maxArmor = 100;

    [Header("Special")]
    public bool hasBossWeapon = false;
    public float bossWeaponCooldown = 0f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private float attackTimer;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float invincibilityTimer;
    private GameObject crosshair;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (Inventory.Instance != null && Inventory.Instance.weapons.Count > 0)
            Inventory.Instance.EquipWeapon(Inventory.Instance.currentWeaponIndex);
        else
            EquipWeapon("Pistol");

        CreateCrosshair();
    }

    void CreateCrosshair()
    {
        crosshair = new GameObject("Crosshair");
        SpriteRenderer sr = crosshair.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(8, new Color(1f, 1f, 1f, 0.8f));
        sr.sortingOrder = 50;
        crosshair.transform.localScale = Vector3.one * 0.3f;
    }

    void Update()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        bool shopOpen = ShopUI.Instance != null && ShopUI.Instance.IsOpen();
        bool statsOpen = UIManager.Instance != null && UIManager.Instance.StatsVisible();

        if (!shopOpen && !statsOpen)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
            movement = movement.normalized;

            if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0)
                StartDash();

            if (Input.GetMouseButton(0) && attackTimer <= 0)
                Shoot();
        }
        else
        {
            movement = Vector2.zero;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (Inventory.Instance != null)
                Inventory.Instance.NextWeapon();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchWeapon(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchWeapon(4);

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ToggleStatsPanel();
        }

        dashCooldownTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
        invincibilityTimer -= Time.deltaTime;

        currentEnergy = Mathf.Min((int)(currentEnergy + energyRegenRate * Time.deltaTime), maxEnergy);

        Vector3 mouseFlip3D = Input.mousePosition;
        mouseFlip3D.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector2 mouseFlipPos = Camera.main.ScreenToWorldPoint(mouseFlip3D);

        if (crosshair != null)
            crosshair.transform.position = mouseFlipPos;

        if (spriteRenderer != null)
        {
            if (mouseFlipPos.x < transform.position.x)
                spriteRenderer.flipX = true;
            else if (mouseFlipPos.x > transform.position.x)
                spriteRenderer.flipX = false;
        }
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            Vector2 newPos = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        rb.linearVelocity = movement * dashSpeed;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
            Invoke("ResetAlpha", dashDuration);
        }
    }

    public void StopDash()
    {
        isDashing = false;
        dashTimer = 0;
        rb.linearVelocity = Vector2.zero;
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    void ResetAlpha()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    void Shoot()
    {
        if (currentEnergy < energyCost) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayShootSound();

        if (PostProcessEffect.Instance != null)
            PostProcessEffect.Instance.TriggerScreenShake(0.03f, 0.05f);

        attackTimer = attackCooldown;
        currentEnergy -= energyCost;

        Vector3 mousePos3D = Input.mousePosition;
        mousePos3D.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mousePos3D);
        Vector2 shootDir = (mousePos - (Vector2)transform.position).normalized;

        bool shootBack = Input.GetMouseButton(1);
        if (shootBack)
            shootDir = -shootDir;

        if (WeaponAnimator.Instance != null)
            WeaponAnimator.Instance.PlayShootEffect(shootDir);

        for (int i = 0; i < projectilesPerShot; i++)
        {
            float spread = Random.Range(-0.15f, 0.15f);
            Vector2 dir = Quaternion.Euler(0, 0, spread) * shootDir;
            SpawnPlayerBullet(dir);
        }

        Vector2 nearestEnemyDir = FindNearestEnemyDir();
        if (nearestEnemyDir != Vector2.zero && !shootBack)
        {
            for (int i = 0; i < Mathf.CeilToInt(projectilesPerShot * 0.5f); i++)
            {
                float spread = Random.Range(-0.1f, 0.1f);
                Vector2 dir = Quaternion.Euler(0, 0, spread) * nearestEnemyDir;
                SpawnPlayerBullet(dir);
            }
        }
    }

    void SpawnPlayerBullet(Vector2 dir)
    {
        Vector3 spawnPos = transform.position + (Vector3)(dir * 0.5f);
        GameObject bullet = GameSetup.Instance.SpawnBullet(spawnPos, dir, attackDamage);
    }

    Vector2 FindNearestEnemyDir()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return Vector2.zero;

        float minDist = float.MaxValue;
        Vector2 closest = Vector2.zero;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist && dist < 15f)
            {
                minDist = dist;
                closest = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
            }
        }

        return closest;
    }

    public void TakeDamage(int damage)
    {
        if (!enabled || invincibilityTimer > 0) return;

        int actualDamage = damage;
        if (armor > 0)
        {
            int absorbed = Mathf.Min(armor, damage);
            armor -= absorbed;
            actualDamage = damage - absorbed;
        }

        currentHealth -= actualDamage;
        invincibilityTimer = 0.5f;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = armor > 0 ? new Color(0.5f, 0.8f, 1f) : Color.red;
            Invoke("ResetColor", 0.15f);
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHitSound();

        if (PostProcessEffect.Instance != null)
        {
            PostProcessEffect.Instance.TriggerHitFlash();
            PostProcessEffect.Instance.TriggerScreenShake(0.15f, 0.15f);
        }

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
        enabled = false;
        Time.timeScale = 0f;
        if (spriteRenderer != null)
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);
        if (GameOverUI.Instance != null)
            GameOverUI.Instance.ShowGameOver();
    }

    public void EquipWeapon(string weaponName)
    {
        if (!GameData.Weapons.ContainsKey(weaponName)) return;
        WeaponData data = GameData.Weapons[weaponName];
        attackDamage = data.damage;
        attackCooldown = data.fireRate;
        projectilesPerShot = data.projectiles;
        energyCost = data.energyCost;

        if (WeaponAnimator.Instance != null)
            WeaponAnimator.Instance.SetWeapon(weaponName);
    }

    void SwitchWeapon(int index)
    {
        if (Inventory.Instance != null)
            Inventory.Instance.EquipWeapon(index);
    }
}
