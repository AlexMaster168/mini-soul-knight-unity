using UnityEngine;

public class WeaponAnimator : MonoBehaviour
{
    public static WeaponAnimator Instance;

    private Transform player;
    private Transform pivot;
    private Transform weaponT;
    private SpriteRenderer weaponSr;
    private SpriteRenderer glowSr;
    private string currentWeapon;
    private WeaponProfile profile;

    private Vector2 aimDir = Vector2.right;
    private float kick, rise;
    private float swingT = 1f, swingDur = 0.2f, swingSide = 1f;
    private float equipT = 1f;
    private float lastShot = -10f;
    private float pumpTimer;
    private float hideUntil;
    private float glow;

    const float HoldDist = 0.32f;
    const float MuzzleDist = 1.0f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = PlayerController.Instance.transform;
        CreateWeaponVisual();
        if (currentWeapon != null) ApplyWeapon();
    }

    void CreateWeaponVisual()
    {
        pivot = new GameObject("WeaponPivot").transform;
        pivot.SetParent(player, false);
        pivot.localPosition = new Vector3(0, 0.05f, 0);

        GameObject w = new GameObject("WeaponVisual");
        weaponT = w.transform;
        weaponT.SetParent(pivot, false);
        weaponT.localPosition = new Vector3(HoldDist, 0, 0);
        weaponSr = w.AddComponent<SpriteRenderer>();
        weaponSr.sortingOrder = 12;

        GameObject g = new GameObject("MuzzleGlow");
        g.transform.SetParent(weaponT, false);
        glowSr = g.AddComponent<SpriteRenderer>();
        glowSr.sprite = SpriteGenerator.CreateCircle(16, Color.white);
        glowSr.sortingOrder = 13;
        g.transform.localScale = Vector3.one * 0.35f;
        glowSr.enabled = false;
    }

    public void SetWeapon(string weaponName)
    {
        currentWeapon = weaponName;
        if (weaponSr != null) ApplyWeapon();
    }

    void ApplyWeapon()
    {
        profile = WeaponProfile.Get(currentWeapon);
        weaponSr.sprite = PixelArt.Weapon(currentWeapon);
        weaponSr.enabled = true;
        hideUntil = 0f;
        equipT = 0f;
        kick = rise = 0f;
        swingT = 1f;

        bool energy = profile.cls == WeaponClass.Energy || profile.cls == WeaponClass.Exotic;
        glowSr.enabled = energy;
        if (energy)
        {
            Color c = profile.color;
            glowSr.color = new Color(c.r, c.g, c.b, 0.5f);
            // кончик ствола в локальных координатах спрайта: (27-8)/20 юнитов
            glowSr.transform.localPosition = new Vector3((27f - PixelArt.WeaponGripX) / 20f, 0, 0);
        }
    }

    Vector2 Origin { get { return (Vector2)player.position + new Vector2(0, 0.05f); } }

    void Update()
    {
        if (player == null || weaponT == null || profile == null) return;

        Vector3 m3 = Input.mousePosition;
        m3.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector2 mouse = Camera.main.ScreenToWorldPoint(m3);
        Vector2 to = mouse - Origin;
        if (to.sqrMagnitude > 0.01f) aimDir = to.normalized;

        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        bool left = aimDir.x < 0f;
        float side = left ? -1f : 1f;
        float dt = Time.deltaTime;

        kick = Mathf.Lerp(kick, 0f, 1f - Mathf.Exp(-profile.recover * dt));
        rise = Mathf.Lerp(rise, 0f, 1f - Mathf.Exp(-profile.recover * 0.7f * dt));

        float swingAngle = 0f, swingReach = 0f, swingScale = 1f;
        if (swingT < 1f)
        {
            swingT = Mathf.Min(1f, swingT + dt / swingDur);
            float e = 1f - (1f - swingT) * (1f - swingT);
            swingAngle = Mathf.Lerp(-95f, 95f, e) * swingSide * side;
            swingReach = Mathf.Sin(swingT * Mathf.PI) * 0.4f;
            swingScale = 1f + Mathf.Sin(swingT * Mathf.PI) * 0.25f;
        }

        float equipScale = 1f, equipSpin = 0f;
        if (equipT < 1f)
        {
            equipT = Mathf.Min(1f, equipT + dt / 0.25f);
            float k = equipT - 1f;
            equipScale = 1f + 2.7f * k * k * k + 1.7f * k * k; // easeOutBack
            equipSpin = (1f - equipT) * 360f * side;
        }

        pivot.position = Origin;
        pivot.rotation = Quaternion.Euler(0, 0, angle + rise * side + swingAngle + equipSpin);

        float bob = 0f;
        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) + Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0f)
            bob = Mathf.Sin(Time.time * 14f) * 0.03f;

        float pump = 0f;
        if (pumpTimer > 0f)
        {
            pumpTimer -= dt;
            float phase = 1f - pumpTimer / 0.5f;
            if (phase > 0.4f && phase < 0.9f)
                pump = -0.2f * Mathf.Sin((phase - 0.4f) / 0.5f * Mathf.PI);
        }

        float jitter = 0f;
        if (currentWeapon == "Minigun" && Time.time - lastShot < 0.12f)
            jitter = Random.Range(-0.04f, 0.04f);

        weaponT.localPosition = new Vector3(HoldDist - kick + swingReach + pump, bob + jitter, 0);
        float s = 0.7f * equipScale * swingScale;
        weaponT.localScale = new Vector3(s, left ? -s : s, 1f);

        if (hideUntil > 0f)
        {
            weaponSr.enabled = Time.time > hideUntil;
            if (weaponSr.enabled) hideUntil = 0f;
        }

        if (glowSr.enabled)
        {
            glow = Mathf.Lerp(glow, 0f, 1f - Mathf.Exp(-8f * dt));
            float pulse = 0.5f + Mathf.Sin(Time.time * 6f) * 0.15f + glow * 0.5f;
            Color c = profile.color;
            glowSr.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(pulse));
            glowSr.transform.localScale = Vector3.one * (0.35f + glow * 0.5f);
        }
    }

    public void PlayShootEffect(Vector2 direction)
    {
        if (currentWeapon == null || profile == null) return;

        lastShot = Time.time;
        kick = profile.kick;
        rise = profile.rise;
        glow = 1f;

        switch (profile.cls)
        {
            case WeaponClass.Pistol: PistolEffect(direction); break;
            case WeaponClass.Shotgun: ShotgunEffect(direction); pumpTimer = 0.5f; break;
            case WeaponClass.SMG: SMGEffect(direction); break;
            case WeaponClass.Rifle: RifleEffect(direction); break;
            case WeaponClass.Sniper: SniperEffect(direction); break;
            case WeaponClass.Energy: EnergyEffect(direction); break;
            case WeaponClass.Heavy: HeavyEffect(direction); break;
            case WeaponClass.Exotic: ExoticEffect(direction); break;
            case WeaponClass.Melee: MeleeEffect(direction); break;
        }
    }

    void Shake(float intensity, float duration)
    {
        if (PostProcessEffect.Instance != null)
            PostProcessEffect.Instance.TriggerScreenShake(intensity, duration);
    }

    void PistolEffect(Vector2 dir)
    {
        SpawnMuzzleFlash(dir, 0.15f, new Color(1f, 0.9f, 0.3f), 0.1f);
        SpawnShellCasing(dir);
        Shake(0.03f, 0.05f);
        if (currentWeapon == "DesertEagle" || currentWeapon == "Revolver")
        {
            SpawnMuzzleFlash(dir, 0.25f, new Color(1f, 0.7f, 0.1f), 0.12f);
            SpawnSmokePuff(dir);
            Shake(0.08f, 0.08f);
        }
    }

    void ShotgunEffect(Vector2 dir)
    {
        for (int i = 0; i < 3; i++)
        {
            float angle = Random.Range(-0.5f, 0.5f);
            Vector2 flashDir = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg) * dir;
            SpawnMuzzleFlash(flashDir, 0.2f, new Color(1f, 0.8f, 0.2f), 0.08f);
        }
        SpawnSmokePuff(dir);
        Shake(0.1f, 0.1f);
    }

    void SMGEffect(Vector2 dir)
    {
        float size = Random.Range(0.06f, 0.12f);
        SpawnMuzzleFlash(dir, size, new Color(1f, 0.95f, 0.5f), 0.05f);
        SpawnShellCasing(dir);
        Shake(0.015f, 0.04f);
    }

    void RifleEffect(Vector2 dir)
    {
        SpawnMuzzleFlash(dir, 0.18f, new Color(1f, 0.85f, 0.2f), 0.08f);
        SpawnTracer(dir, 0.3f);
        SpawnShellCasing(dir);
        if (currentWeapon == "AK")
        {
            SpawnSmokePuff(dir);
            Shake(0.06f, 0.06f);
        }
        else
            Shake(0.03f, 0.05f);
    }

    void SniperEffect(Vector2 dir)
    {
        if (currentWeapon == "Crossbow")
        {
            SpawnTracer(dir, 0.4f);
            return;
        }
        SpawnMuzzleFlash(dir, 0.3f, new Color(1f, 0.9f, 0.4f), 0.15f);
        SpawnBeamTrail(dir, 1.5f);
        SpawnSmokePuff(dir);
        Shake(0.15f, 0.12f);
    }

    void EnergyEffect(Vector2 dir)
    {
        Color energyColor = profile.color;
        SpawnEnergyOrb(dir, energyColor);
        SpawnElectricSparks(dir, energyColor);
        SpawnMuzzleFlash(dir, 0.2f, energyColor, 0.12f);
        if (currentWeapon == "Laser" || currentWeapon == "LaserRifle")
            SpawnBeamTrail(dir, 1.2f);
        Shake(0.02f, 0.05f);
    }

    void HeavyEffect(Vector2 dir)
    {
        if (currentWeapon == "Flamethrower")
        {
            for (int i = 0; i < 5; i++)
            {
                float angle = Random.Range(-0.4f, 0.4f);
                Vector2 fireDir = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg) * dir;
                SpawnFireParticle(fireDir);
            }
            return;
        }
        if (currentWeapon == "Minigun")
        {
            SpawnMuzzleFlash(dir, 0.14f, new Color(1f, 0.9f, 0.4f), 0.04f);
            SpawnShellCasing(dir);
            Shake(0.04f, 0.04f);
            return;
        }

        SpawnMuzzleFlash(dir, 0.35f, new Color(1f, 0.5f, 0.1f), 0.2f);
        SpawnFireBurst(dir);
        SpawnSmokePuff(dir);
        if (currentWeapon == "Rocket")
            for (int i = 0; i < 6; i++)   // задний выхлоп
                SpawnSmokeAt(Origin - dir * 0.9f, -dir * Random.Range(1f, 3f) + Random.insideUnitCircle);
        Shake(0.2f, 0.15f);
    }

    void ExoticEffect(Vector2 dir)
    {
        if (currentWeapon == "Boomerang")
        {
            hideUntil = Time.time + 1.0f;
            SpawnSwingArc(dir);
            return;
        }
        for (int i = 0; i < 8; i++)
        {
            Vector2 sparkDir = Quaternion.Euler(0, 0, Random.Range(0f, 360f)) * Vector2.right;
            SpawnSparkle(sparkDir);
        }
        SpawnMuzzleFlash(dir, 0.2f, currentWeapon == "FairyGun" ? new Color(1f, 0.6f, 0.9f) : new Color(1f, 0.85f, 0.3f), 0.1f);
    }

    void MeleeEffect(Vector2 dir)
    {
        switch (currentWeapon)
        {
            case "Katana": swingDur = 0.13f; break;
            case "Sword": swingDur = 0.18f; break;
            case "Axe": swingDur = 0.22f; break;
            case "Mace": swingDur = 0.28f; break;
            case "Scythe": swingDur = 0.3f; break;
            default: swingDur = 0.2f; break;
        }
        swingT = 0f;
        swingSide = -swingSide;
        SpawnSwingArc(dir);
        if (currentWeapon == "Scythe" || currentWeapon == "Mace" || currentWeapon == "Axe")
            Shake(0.1f, 0.08f);
    }

    void SpawnSmokeAt(Vector2 pos, Vector2 vel)
    {
        GameObject smoke = new GameObject("Smoke");
        smoke.transform.position = pos;
        SpriteRenderer sr = smoke.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(12, new Color(0.6f, 0.6f, 0.6f, 0.5f));
        sr.sortingOrder = 14;
        smoke.transform.localScale = Vector3.one * 0.2f;
        ParticleMover pm = smoke.AddComponent<ParticleMover>();
        pm.velocity = vel;
        pm.lifetime = 0.4f;
        pm.shrink = true;
    }

    void SpawnMuzzleFlash(Vector2 dir, float size, Color color, float duration)
    {
        GameObject flash = new GameObject("MuzzleFlash");
        flash.transform.position = Origin + dir * 1.0f;
        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(8, color);
        sr.sortingOrder = 15;
        flash.transform.localScale = Vector3.one * size;
        flash.transform.up = (Vector3)dir;

        ParticleMover pm = flash.AddComponent<ParticleMover>();
        pm.velocity = dir * 2f;
        pm.lifetime = duration;
        pm.shrink = true;
    }

    void SpawnSmokePuff(Vector2 dir)
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject smoke = new GameObject("Smoke");
            smoke.transform.position = Origin + dir * 0.9f + Random.insideUnitCircle * 0.2f;
            SpriteRenderer sr = smoke.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateCircle(12, new Color(0.5f, 0.5f, 0.5f, 0.4f));
            sr.sortingOrder = 14;
            smoke.transform.localScale = Vector3.one * 0.15f;

            ParticleMover pm = smoke.AddComponent<ParticleMover>();
            pm.velocity = dir * Random.Range(0.5f, 2f) + Random.insideUnitCircle;
            pm.lifetime = Random.Range(0.3f, 0.6f);
            pm.shrink = true;
        }
    }

    void SpawnShellCasing(Vector2 dir)
    {
        GameObject shell = new GameObject("Shell");
        shell.transform.position = Origin;
        SpriteRenderer sr = shell.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateSquare(4, new Color(0.8f, 0.7f, 0.2f));
        sr.sortingOrder = 13;
        shell.transform.localScale = Vector3.one * 0.06f;

        Vector2 perp = new Vector2(-dir.y, dir.x);
        ParticleMover pm = shell.AddComponent<ParticleMover>();
        pm.velocity = perp * Random.Range(2f, 4f) + Vector2.up * 3f;
        pm.lifetime = 0.5f;
        pm.shrink = true;
    }

    void SpawnTracer(Vector2 dir, float length)
    {
        GameObject tracer = new GameObject("Tracer");
        tracer.transform.position = Origin + dir * 0.9f;
        SpriteRenderer sr = tracer.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateSquare(4, new Color(1f, 1f, 0.5f, 0.8f));
        sr.sortingOrder = 14;
        tracer.transform.localScale = new Vector3(0.03f, length, 1f);
        tracer.transform.up = (Vector3)dir;

        ParticleMover pm = tracer.AddComponent<ParticleMover>();
        pm.velocity = dir * 30f;
        pm.lifetime = 0.15f;
        pm.shrink = true;
    }

    void SpawnBeamTrail(Vector2 dir, float length)
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject beam = new GameObject("Beam");
            beam.transform.position = Origin + dir * (0.8f + i * 0.3f);
            SpriteRenderer sr = beam.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateSquare(4, new Color(1f, 1f, 0.8f, 0.9f));
            sr.sortingOrder = 14;
            beam.transform.localScale = new Vector3(0.04f, length * (1f - i * 0.2f), 1f);
            beam.transform.up = (Vector3)dir;

            ParticleMover pm = beam.AddComponent<ParticleMover>();
            pm.velocity = dir * 40f;
            pm.lifetime = 0.12f;
            pm.shrink = true;
        }
    }

    void SpawnEnergyOrb(Vector2 dir, Color color)
    {
        GameObject orb = new GameObject("EnergyOrb");
        orb.transform.position = Origin + dir * 0.9f;
        SpriteRenderer sr = orb.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(12, color);
        sr.sortingOrder = 15;
        orb.transform.localScale = Vector3.one * 0.2f;

        ParticleMover pm = orb.AddComponent<ParticleMover>();
        pm.velocity = dir * 5f;
        pm.lifetime = 0.2f;
        pm.shrink = true;
    }

    void SpawnElectricSparks(Vector2 dir, Color color)
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject spark = new GameObject("Spark");
            spark.transform.position = Origin + dir * 0.9f;
            SpriteRenderer sr = spark.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateCircle(4, color);
            sr.sortingOrder = 15;
            spark.transform.localScale = Vector3.one * 0.05f;

            Vector2 sparkDir = dir + Random.insideUnitCircle * 0.8f;
            ParticleMover pm = spark.AddComponent<ParticleMover>();
            pm.velocity = sparkDir * 8f;
            pm.lifetime = 0.1f;
            pm.shrink = true;
        }
    }

    void SpawnFireBurst(Vector2 dir)
    {
        for (int i = 0; i < 6; i++)
        {
            float angle = Random.Range(-0.6f, 0.6f);
            Vector2 fireDir = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg) * dir;
            SpawnFireParticle(fireDir);
        }
    }

    void SpawnFireParticle(Vector2 dir)
    {
        GameObject fire = new GameObject("Fire");
        fire.transform.position = Origin + dir * 0.9f;
        SpriteRenderer sr = fire.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(8, new Color(1f, 0.5f, 0.05f, 0.9f));
        sr.sortingOrder = 15;
        fire.transform.localScale = Vector3.one * Random.Range(0.1f, 0.2f);

        ParticleMover pm = fire.AddComponent<ParticleMover>();
        pm.velocity = dir * Random.Range(5f, 10f);
        pm.lifetime = Random.Range(0.15f, 0.3f);
        pm.shrink = true;
    }

    void SpawnSparkle(Vector2 dir)
    {
        GameObject sparkle = new GameObject("Sparkle");
        sparkle.transform.position = Origin + dir * Random.Range(0.3f, 0.8f);
        SpriteRenderer sr = sparkle.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(4, new Color(1f, 0.8f, 1f, 0.9f));
        sr.sortingOrder = 15;
        sparkle.transform.localScale = Vector3.one * Random.Range(0.04f, 0.08f);

        ParticleMover pm = sparkle.AddComponent<ParticleMover>();
        pm.velocity = dir * Random.Range(2f, 5f);
        pm.lifetime = Random.Range(0.3f, 0.6f);
        pm.shrink = true;
    }

    void SpawnSwingArc(Vector2 dir)
    {
        for (int i = 0; i < 8; i++)
        {
            float angle = -60f + (120f / 7) * i;
            Vector2 arcDir = Quaternion.Euler(0, 0, angle) * dir;

            GameObject arc = new GameObject("SwingArc");
            arc.transform.position = Origin + arcDir * 0.6f;
            SpriteRenderer sr = arc.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateCircle(4, new Color(0.8f, 0.8f, 0.9f, 0.7f));
            sr.sortingOrder = 15;
            arc.transform.localScale = Vector3.one * 0.08f;

            ParticleMover pm = arc.AddComponent<ParticleMover>();
            pm.velocity = arcDir * 3f;
            pm.lifetime = 0.15f;
            pm.shrink = true;
        }
    }

}
