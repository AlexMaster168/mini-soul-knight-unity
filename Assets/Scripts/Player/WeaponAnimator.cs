using UnityEngine;

public class WeaponAnimator : MonoBehaviour
{
    public static WeaponAnimator Instance;

    private Transform player;
    private GameObject weaponVisual;
    private SpriteRenderer weaponSr;
    private string currentWeapon;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = PlayerController.Instance.transform;
        CreateWeaponVisual();
    }

    void CreateWeaponVisual()
    {
        weaponVisual = new GameObject("WeaponVisual");
        weaponVisual.transform.SetParent(player);
        weaponVisual.transform.localPosition = new Vector3(0.4f, 0.1f, 0);
        weaponSr = weaponVisual.AddComponent<SpriteRenderer>();
        weaponSr.sortingOrder = 12;
    }

    public void SetWeapon(string weaponName)
    {
        currentWeapon = weaponName;
        UpdateWeaponVisual();
    }

    void UpdateWeaponVisual()
    {
        if (weaponSr == null) return;

        if (currentWeapon == null)
        {
            weaponSr.sprite = null;
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool flip = mousePos.x < player.position.x;
        weaponSr.flipX = flip;

        if (IsMelee(currentWeapon))
            weaponSr.sprite = SpriteGenerator.CreateSquare(8, new Color(0.7f, 0.7f, 0.75f));
        else if (IsEnergy(currentWeapon))
            weaponSr.sprite = SpriteGenerator.CreateSquare(8, new Color(0.3f, 0.6f, 1f));
        else if (IsHeavy(currentWeapon))
            weaponSr.sprite = SpriteGenerator.CreateSquare(10, new Color(0.4f, 0.4f, 0.45f));
        else
            weaponSr.sprite = SpriteGenerator.CreateSquare(8, new Color(0.5f, 0.5f, 0.55f));
    }

    public void PlayShootEffect(Vector2 direction)
    {
        if (currentWeapon == null) return;

        if (IsPistol(currentWeapon))
            PistolEffect(direction);
        else if (IsShotgun(currentWeapon))
            ShotgunEffect(direction);
        else if (IsSMG(currentWeapon))
            SMGEffect(direction);
        else if (IsRifle(currentWeapon))
            RifleEffect(direction);
        else if (IsSniper(currentWeapon))
            SniperEffect(direction);
        else if (IsEnergy(currentWeapon))
            EnergyEffect(direction);
        else if (IsHeavy(currentWeapon))
            HeavyEffect(direction);
        else if (IsExotic(currentWeapon))
            ExoticEffect(direction);
        else if (IsMelee(currentWeapon))
            MeleeEffect(direction);

        UpdateWeaponVisual();
    }

    void PistolEffect(Vector2 dir)
    {
        SpawnMuzzleFlash(dir, 0.15f, new Color(1f, 0.9f, 0.3f), 0.1f);
        SpawnRecoilKick(dir, 0.15f);
        if (currentWeapon == "DesertEagle")
        {
            SpawnMuzzleFlash(dir, 0.25f, new Color(1f, 0.7f, 0.1f), 0.12f);
            if (PostProcessEffect.Instance != null)
                PostProcessEffect.Instance.TriggerScreenShake(0.08f, 0.08f);
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
        SpawnRecoilKick(dir, 0.25f);
        if (PostProcessEffect.Instance != null)
            PostProcessEffect.Instance.TriggerScreenShake(0.1f, 0.1f);
    }

    void SMGEffect(Vector2 dir)
    {
        float size = Random.Range(0.06f, 0.12f);
        SpawnMuzzleFlash(dir, size, new Color(1f, 0.95f, 0.5f), 0.05f);
        SpawnShellCasing(dir);
    }

    void RifleEffect(Vector2 dir)
    {
        SpawnMuzzleFlash(dir, 0.18f, new Color(1f, 0.85f, 0.2f), 0.08f);
        SpawnTracer(dir, 0.3f);
        if (currentWeapon == "AK")
        {
            SpawnSmokePuff(dir);
            if (PostProcessEffect.Instance != null)
                PostProcessEffect.Instance.TriggerScreenShake(0.06f, 0.06f);
        }
    }

    void SniperEffect(Vector2 dir)
    {
        SpawnMuzzleFlash(dir, 0.3f, new Color(1f, 0.9f, 0.4f), 0.15f);
        SpawnBeamTrail(dir, 1.5f);
        SpawnSmokePuff(dir);
        if (PostProcessEffect.Instance != null)
            PostProcessEffect.Instance.TriggerScreenShake(0.15f, 0.12f);
    }

    void EnergyEffect(Vector2 dir)
    {
        Color energyColor = GetEnergyColor();
        SpawnEnergyOrb(dir, energyColor);
        SpawnElectricSparks(dir, energyColor);
        SpawnMuzzleFlash(dir, 0.2f, energyColor, 0.12f);
    }

    void HeavyEffect(Vector2 dir)
    {
        SpawnMuzzleFlash(dir, 0.35f, new Color(1f, 0.5f, 0.1f), 0.2f);
        SpawnFireBurst(dir);
        SpawnSmokePuff(dir);
        if (PostProcessEffect.Instance != null)
            PostProcessEffect.Instance.TriggerScreenShake(0.2f, 0.15f);

        if (currentWeapon == "Flamethrower")
        {
            for (int i = 0; i < 5; i++)
            {
                float angle = Random.Range(-0.4f, 0.4f);
                Vector2 fireDir = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg) * dir;
                SpawnFireParticle(fireDir);
            }
        }
    }

    void ExoticEffect(Vector2 dir)
    {
        for (int i = 0; i < 8; i++)
        {
            Vector2 sparkDir = Quaternion.Euler(0, 0, Random.Range(0f, 360f)) * Vector2.one;
            SpawnSparkle(sparkDir);
        }
        SpawnMuzzleFlash(dir, 0.2f, new Color(0.8f, 0.4f, 1f), 0.1f);
    }

    void MeleeEffect(Vector2 dir)
    {
        SpawnSwingArc(dir);
        if (currentWeapon == "Scythe" || currentWeapon == "Mace")
        {
            if (PostProcessEffect.Instance != null)
                PostProcessEffect.Instance.TriggerScreenShake(0.1f, 0.08f);
        }
    }

    void SpawnMuzzleFlash(Vector2 dir, float size, Color color, float duration)
    {
        GameObject flash = new GameObject("MuzzleFlash");
        flash.transform.position = (Vector2)player.position + dir * 0.6f;
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

    void SpawnRecoilKick(Vector2 dir, float intensity)
    {
        player.position += (Vector3)(-dir * intensity * 0.3f);
    }

    void SpawnSmokePuff(Vector2 dir)
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject smoke = new GameObject("Smoke");
            smoke.transform.position = (Vector2)player.position + dir * 0.5f + Random.insideUnitCircle * 0.2f;
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
        shell.transform.position = (Vector2)player.position;
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
        tracer.transform.position = (Vector2)player.position + dir * 0.5f;
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
            beam.transform.position = (Vector2)player.position + dir * (0.3f + i * 0.3f);
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
        orb.transform.position = (Vector2)player.position + dir * 0.5f;
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
            spark.transform.position = (Vector2)player.position + dir * 0.4f;
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
        fire.transform.position = (Vector2)player.position + dir * 0.5f;
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
        sparkle.transform.position = (Vector2)player.position + dir * Random.Range(0.3f, 0.8f);
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
            arc.transform.position = (Vector2)player.position + arcDir * 0.6f;
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

    Color GetEnergyColor()
    {
        switch (currentWeapon)
        {
            case "LaserRifle":
            case "Laser": return new Color(0.3f, 0.8f, 1f);
            case "Plasma": return new Color(0.5f, 0.2f, 1f);
            case "Shock":
            case "Thunder": return new Color(0.9f, 0.9f, 0.2f);
            case "IceGun": return new Color(0.4f, 0.8f, 1f);
            case "PoisonGun": return new Color(0.3f, 0.9f, 0.2f);
            default: return Color.cyan;
        }
    }

    bool IsPistol(string w) => w == "Pistol" || w == "Dual" || w == "Revolver" || w == "DesertEagle";
    bool IsShotgun(string w) => w == "Shotgun" || w == "SuperShotgun" || w == "TacticalSG";
    bool IsSMG(string w) => w == "Uzi" || w == "Mac10" || w == "Thompson";
    bool IsRifle(string w) => w == "AK" || w == "LMG" || w == "M4" || w == "Famas";
    bool IsSniper(string w) => w == "Sniper" || w == "AWP" || w == "Crossbow";
    bool IsEnergy(string w) => w == "LaserRifle" || w == "Laser" || w == "Plasma" || w == "Shock" || w == "Thunder" || w == "IceGun" || w == "PoisonGun";
    bool IsHeavy(string w) => w == "Rocket" || w == "Flamethrower" || w == "HolyGrenade" || w == "GrenadeLauncher" || w == "Minigun";
    bool IsExotic(string w) => w == "Boomerang" || w == "Star Wand" || w == "FairyGun";
    bool IsMelee(string w) => w == "Sword" || w == "Katana" || w == "Mace" || w == "Axe" || w == "Scythe";

    void Update()
    {
        if (weaponVisual != null && player != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            bool flip = mousePos.x < player.position.x;
            if (weaponSr != null) weaponSr.flipX = flip;
            weaponVisual.transform.localPosition = flip ? new Vector3(-0.4f, 0.1f, 0) : new Vector3(0.4f, 0.1f, 0);
        }
    }
}
