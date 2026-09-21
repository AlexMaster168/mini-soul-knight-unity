using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Movement")]
    public float moveSpeed = 8f;
    // счётчики временных бустов: несколько одновременных бустов больше не накапливают бонус навсегда
    [HideInInspector] public int speedBoosts, damageBoosts, multiShotBoosts;
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
    [HideInInspector] public float invulnerableUntil;
    [HideInInspector] public float overdriveUntil;
    [HideInInspector] public float ascendUntil;

    // постоянные улучшения из магазина
    [HideInInspector] public float damageMultiplier = 1f;
    [HideInInspector] public float damageReduction = 0f;

    // с 3 этажа герой бьёт сильнее: +25% урона за каждый этаж начиная с третьего
    public static float DepthDamageBonus
    {
        get
        {
            DungeonGenerator g = DungeonGenerator.Instance;
            if (g == null || g.IsLobby) return 1f;
            return 1f + 0.25f * Mathf.Max(0, g.GetFloor() - 2);
        }
    }

    public int EffectiveDamage
    {
        get { return Mathf.RoundToInt(attackDamage * damageMultiplier * DepthDamageBonus * (damageBoosts > 0 ? 2f : 1f) * (Time.time < ascendUntil ? 3f : 1f)); }
    }
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

    [HideInInspector] public WeaponData weaponData;
    [HideInInspector] public WeaponProfile weaponProfile;
    [HideInInspector] public string weaponName;

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
        if (DungeonGenerator.Instance != null && DungeonGenerator.Instance.IsTransitioning)
        {
            movement = Vector2.zero;
            return;
        }

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

        bool shopOpen = (ShopUI.Instance != null && ShopUI.Instance.IsOpen()) || (WeaponShopUI.Instance != null && WeaponShopUI.Instance.IsOpen())
                       || (LobbyUI.Instance != null && LobbyUI.Instance.IsOpen())
                       || (AbilityMenuUI.Instance != null && AbilityMenuUI.Instance.IsOpen());
        bool statsOpen = UIManager.Instance != null && UIManager.Instance.StatsVisible();

        if (!shopOpen && !statsOpen)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
            movement = movement.normalized;

            if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0)
                StartDash();

            bool wantFront = Input.GetMouseButton(0);
            bool wantBack = Input.GetMouseButton(1);
            if ((wantFront || wantBack) && attackTimer <= 0)
                Shoot(wantFront, wantBack);
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

        float wheel = Input.mouseScrollDelta.y;
        if (wheel != 0f && Inventory.Instance != null && !shopOpen && !statsOpen)
        {
            if (wheel > 0f) Inventory.Instance.PreviousWeapon();
            else Inventory.Instance.NextWeapon();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchWeapon(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchWeapon(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SwitchWeapon(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) SwitchWeapon(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) SwitchWeapon(7);
        if (Input.GetKeyDown(KeyCode.Alpha9) && Inventory.Instance != null) Inventory.Instance.ToggleReserve(this);

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ToggleStatsPanel();
        }

        dashCooldownTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
        invincibilityTimer -= Time.deltaTime;

        currentEnergy = Mathf.Min((int)(currentEnergy + energyRegenRate * Time.deltaTime), maxEnergy);
        if (Inventory.Instance != null)
            Inventory.Instance.UpdateReserve(this);

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

    // ЛКМ - вперёд (к курсору), ПКМ - назад. Оба зажаты - стреляем в обе стороны сразу
    void Shoot(bool front, bool back)
    {
        int sides = (front ? 1 : 0) + (back ? 1 : 0);
        int cost = (Time.time < overdriveUntil || Time.time < ascendUntil) ? 0 : energyCost * sides;
        if (currentEnergy < cost) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayShootSound();

        attackTimer = Time.time < overdriveUntil ? attackCooldown * 0.6f : attackCooldown;
        currentEnergy -= cost;

        Vector3 mousePos3D = Input.mousePosition;
        mousePos3D.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mousePos3D);
        Vector2 aim = (mousePos - (Vector2)transform.position).normalized;

        bool melee = weaponProfile != null && weaponProfile.cls == WeaponClass.Melee;

        for (int side = 0; side < 2; side++)
        {
            bool isBack = side == 1;
            if (isBack ? !back : !front) continue;

            Vector2 shootDir = isBack ? -aim : aim;
            if (!melee) shootDir = ApplyAimAssist(shootDir);

            if (WeaponAnimator.Instance != null)
                WeaponAnimator.Instance.PlayShootEffect(shootDir, isBack);

            FireWeapon(shootDir, melee);
        }
    }

    void FireWeapon(Vector2 shootDir, bool melee)
    {
        if (weaponData == null || weaponProfile == null)
        {
            SpawnLegacyBullet(shootDir);
            return;
        }

        float spread = weaponData.spread * Mathf.Rad2Deg;
        int n = Mathf.Max(1, projectilesPerShot);
        float reach = melee ? 0.5f : 0.6f;

        for (int i = 0; i < n; i++)
        {
            float angle;
            if (n == 1)
                angle = Random.Range(-spread, spread) * 0.5f;
            else
                angle = Mathf.Lerp(-spread, spread, (float)i / (n - 1)) + Random.Range(-spread, spread) * 0.15f;

            Vector2 dir = Quaternion.Euler(0, 0, angle) * shootDir;
            Vector3 spawnPos = transform.position + (Vector3)(dir * reach);
            GameSetup.Instance.SpawnPlayerBullet(spawnPos, dir, EffectiveDamage, weaponData, weaponProfile);
        }
    }

    void SpawnLegacyBullet(Vector2 dir)
    {
        Vector3 spawnPos = transform.position + (Vector3)(dir * 0.5f);
        GameSetup.Instance.SpawnBullet(spawnPos, dir, EffectiveDamage);
    }

    // Лёгкий автоприцел: если враг почти на линии выстрела - довернуть на него
    Vector2 ApplyAimAssist(Vector2 dir)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float bestAngle = 9f;
        Vector2 best = dir;

        foreach (GameObject enemy in enemies)
        {
            Vector2 to = (Vector2)enemy.transform.position - (Vector2)transform.position;
            if (to.magnitude > 16f) continue;
            float ang = Vector2.Angle(dir, to);
            if (ang < bestAngle)
            {
                bestAngle = ang;
                best = to.normalized;
            }
        }
        return best;
    }

    public void TakeDamage(int damage)
    {
        if (!enabled || invincibilityTimer > 0 || Time.time < invulnerableUntil || Time.time < ascendUntil) return;

        int actualDamage = damage;
        if (armor > 0)
        {
            int absorbed = Mathf.Min(armor, damage);
            armor -= absorbed;
            actualDamage = damage - absorbed;
        }

        actualDamage = Mathf.CeilToInt(actualDamage * (1f - damageReduction));
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
        this.weaponName = weaponName;
        weaponData = data;
        weaponProfile = WeaponProfile.Get(weaponName);
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
