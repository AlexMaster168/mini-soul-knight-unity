using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Поведение боссов: у каждого свои способности, суперудар в 3 фазе и мелкие прислужники, которые следуют за игроком
public class Boss : MonoBehaviour
{
    private Enemy enemy;
    private int phase = 1;
    private float specialTimer;
    private float ultTimer;
    private float followerTimer;
    private Transform player;
    private SpriteRenderer sr;
    private bool busy;   // выполняет длинную способность
    private readonly List<Enemy> followers = new List<Enemy>();

    void Start()
    {
        enemy = GetComponent<Enemy>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        sr = GetComponent<SpriteRenderer>();
        specialTimer = 3f;
        ultTimer = 8f;
        followerTimer = 5f;
    }

    void Update()
    {
        if (enemy == null || player == null || enemy.IsSpawning || enemy.IsFrozen) return;

        specialTimer -= Time.deltaTime;
        ultTimer -= Time.deltaTime;
        followerTimer -= Time.deltaTime;

        float hpRatio = (float)enemy.currentHealth / enemy.maxHealth;
        if (hpRatio < 0.3f && phase < 3) EnterPhase(3);
        else if (hpRatio < 0.6f && phase < 2) EnterPhase(2);

        followers.RemoveAll(f => f == null);

        // прислужники: мелкие враги, которые бегут на игрока и периодически пополняются
        if (followerTimer <= 0f)
        {
            followerTimer = phase == 3 ? 6f : phase == 2 ? 8f : 10f;
            if (followers.Count < 3 + phase) SpawnFollowers(FollowerType(), 2);
        }

        if (!busy)
        {
            if (phase == 3 && ultTimer <= 0f)
            {
                ultTimer = 15f;
                StartCoroutine(Ultimate());
            }
            else if (specialTimer <= 0f)
            {
                specialTimer = phase == 3 ? 2.2f : phase == 2 ? 3f : 3.8f;
                PerformSpecial();
            }
        }

        if (sr != null && enemy.damageTakenMult >= 1f)
        {
            float pulse = Mathf.Sin(Time.time * (phase * 2f)) * 0.3f;
            Color baseColor = phase == 3 ? Color.red : phase == 2 ? new Color(1f, 0.5f, 0f) : Color.white;
            sr.color = Color.Lerp(baseColor, Color.white, 0.5f + pulse);
        }
    }

    void EnterPhase(int newPhase)
    {
        phase = newPhase;
        enemy.moveSpeed *= 1.2f;
        enemy.damage += 4;
        if (EffectsManager.Instance != null)
            EffectsManager.Instance.SpawnDeathEffect(transform.position);
        HudHint.Flash(newPhase == 3 ? "The boss is enraged!" : "The boss grows stronger!", 2f);
        SpawnFollowers(FollowerType(), 2);
    }

    // Убить прислужников вместе с боссом (вызывает Enemy при смерти босса)
    public void KillFollowers()
    {
        foreach (Enemy f in followers)
            if (f != null) f.TakeDamage(9999999);
        followers.Clear();
    }

    // ================= прислужники =================

    string FollowerType()
    {
        switch (enemy.enemyType)
        {
            case "CrownedBoar": return "Goblin";
            case "Necromancer": return "Skeleton";
            case "Dragon": return "Imp";
            case "GolemKing": return "Charger";
            case "SpiderQueen": return "Spider";
            default: return "Blinker";   // VoidEye
        }
    }

    void SpawnFollowers(string type, int count)
    {
        if (enemy.roomManager == null) return;
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = enemy.ClampToRoom((Vector2)transform.position + Random.insideUnitCircle * 2.5f);
            enemy.roomManager.enemiesAlive++;
            enemy.roomManager.SpawnEnemy(pos, type, enemy.enemyFloor);
            Enemy f = enemy.roomManager.lastSpawned;
            if (f == null) continue;
            f.maxHealth = Mathf.Max(8, Mathf.RoundToInt(f.maxHealth * 0.6f));
            f.currentHealth = f.maxHealth;
            f.enemyScale *= 0.75f;
            f.transform.localScale *= 0.75f;
            followers.Add(f);
        }
    }

    // ================= способности по типам =================

    void PerformSpecial()
    {
        int pool = phase == 1 ? 2 : 3;
        int pick = Random.Range(0, pool);
        switch (enemy.enemyType)
        {
            case "CrownedBoar":
                if (pick == 0) StartCoroutine(Charge(1));
                else if (pick == 1) FireRing(12);
                else StartCoroutine(Stomp());
                break;
            case "Necromancer":
                if (pick == 0) FireSpread(7);
                else if (pick == 1) SpawnFollowers("Skeleton", 3);
                else FireRing(16);
                break;
            case "Dragon":
                if (pick == 0) StartCoroutine(FireBreath());
                else if (pick == 1) FireSpread(5);
                else StartCoroutine(FireRain(5));
                break;
            case "GolemKing":
                if (pick == 0) StartCoroutine(Quake(3));
                else if (pick == 1) StartCoroutine(Stomp());
                else StartCoroutine(StoneShield());
                break;
            case "SpiderQueen":
                if (pick == 0) FireSpread(9);
                else if (pick == 1) StartCoroutine(Leap());
                else SpawnFollowers("Spider", 3);
                break;
            default: // VoidEye
                if (pick == 0) StartCoroutine(VoidBeams(1.6f));
                else if (pick == 1) StartCoroutine(Blink());
                else StartCoroutine(GravityPull(1.4f));
                break;
        }
    }

    // Суперспособность 3 фазы (у каждого босса своя)
    IEnumerator Ultimate()
    {
        busy = true;
        switch (enemy.enemyType)
        {
            case "CrownedBoar":
                HudHint.Flash("Rampage!", 2f);
                yield return StartCoroutine(Charge(3));
                break;
            case "Necromancer":
                HudHint.Flash("Army of the Dead!", 2f);
                SpawnFollowers("Skeleton", 5);
                FireRing(20);
                yield return new WaitForSeconds(1.2f);
                FireRing(20);
                break;
            case "Dragon":
                HudHint.Flash("Inferno!", 2f);
                StartCoroutine(FireRain(9));
                yield return StartCoroutine(FireBreath());
                break;
            case "GolemKing":
                HudHint.Flash("Earthquake!", 2f);
                StartCoroutine(StoneShield());
                yield return StartCoroutine(Quake(5));
                break;
            case "SpiderQueen":
                HudHint.Flash("Broodstorm!", 2f);
                SpawnFollowers("Spider", 5);
                yield return StartCoroutine(Leap());
                yield return StartCoroutine(Leap());
                break;
            default: // VoidEye
                HudHint.Flash("Singularity!", 2f);
                StartCoroutine(GravityPull(2.2f));
                yield return StartCoroutine(VoidBeams(2.6f));
                break;
        }
        busy = false;
    }

    // ---- общие приёмы ----

    Vector2 PlayerPos { get { return player.position; } }

    void FireSpread(int count)
    {
        Vector2 toPlayer = (PlayerPos - (Vector2)transform.position).normalized;
        for (int i = 0; i < count; i++)
        {
            float spread = (i - count / 2f) * 15f;
            Vector2 dir = Quaternion.Euler(0, 0, spread) * toPlayer;
            enemy.FireBullet(dir, enemy.damage / 2);
        }
    }

    void FireRing(int count, float offset = 0f)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i * Mathf.Deg2Rad + offset;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            enemy.FireBullet(dir, enemy.damage / 3);
        }
    }

    // Красный круг на земле: через delay наносит урон всем, кто в нём
    void Telegraph(Vector2 pos, float radius, float delay, int dmg)
    {
        StartCoroutine(TelegraphRoutine(pos, radius, delay, dmg));
    }

    IEnumerator TelegraphRoutine(Vector2 pos, float radius, float delay, int dmg)
    {
        GameObject g = new GameObject("BossTelegraph");
        g.transform.position = pos;
        SpriteRenderer tsr = g.AddComponent<SpriteRenderer>();
        tsr.sprite = SpriteGenerator.CreateCircle(32, new Color(1f, 0.2f, 0.2f, 0.4f));
        tsr.sortingOrder = 3;

        float t = 0f;
        while (t < delay)
        {
            t += Time.deltaTime;
            g.transform.localScale = Vector3.one * radius * 2f * Mathf.Clamp01(t / delay);
            yield return null;
        }

        if (player != null && Vector2.Distance(player.position, pos) <= radius && PlayerController.Instance != null)
            PlayerController.Instance.TakeDamage(dmg);
        if (EffectsManager.Instance != null)
            EffectsManager.Instance.SpawnDeathEffect(pos);
        Destroy(g);
    }

    IEnumerator Charge(int times)
    {
        busy = true;
        for (int n = 0; n < times; n++)
        {
            Vector2 dir = (PlayerPos - (Vector2)transform.position).normalized;
            float t = 0f;
            while (t < 0.7f)   // замах
            {
                t += Time.deltaTime;
                if (sr != null) sr.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(t * 6f, 1f));
                yield return null;
            }
            t = 0f;
            while (t < 0.55f)  // рывок
            {
                t += Time.deltaTime;
                enemy.MoveBy(dir * 16f * Time.deltaTime);
                yield return null;
            }
            FireRing(10);
        }
        busy = false;
    }

    IEnumerator Stomp()
    {
        busy = true;
        Telegraph(transform.position, 3.2f, 0.9f, Mathf.RoundToInt(enemy.damage * 1.5f));
        yield return new WaitForSeconds(0.95f);
        FireRing(14);
        busy = false;
    }

    IEnumerator FireBreath()
    {
        busy = true;
        for (int wave = 0; wave < 5; wave++)
        {
            Vector2 toPlayer = (PlayerPos - (Vector2)transform.position).normalized;
            for (int i = -3; i <= 3; i++)
            {
                Vector2 dir = Quaternion.Euler(0, 0, i * 9f) * toPlayer;
                enemy.FireBullet(dir, enemy.damage / 2, 13f, 0.4f);
            }
            yield return new WaitForSeconds(0.2f);
        }
        busy = false;
    }

    IEnumerator FireRain(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = i == 0 ? PlayerPos : PlayerPos + Random.insideUnitCircle * 4f;
            Telegraph(enemy.ClampToRoom(pos), 1.5f, 1.0f, Mathf.RoundToInt(enemy.damage * 1.3f));
            yield return new WaitForSeconds(0.25f);
        }
    }

    IEnumerator Quake(int rings)
    {
        busy = true;
        for (int i = 0; i < rings; i++)
        {
            FireRing(12, i * 0.26f);
            yield return new WaitForSeconds(0.45f);
        }
        busy = false;
    }

    IEnumerator StoneShield()
    {
        enemy.damageTakenMult = 0.35f;
        if (sr != null) sr.color = new Color(0.55f, 0.7f, 1f);
        yield return new WaitForSeconds(4f);
        enemy.damageTakenMult = 1f;
    }

    IEnumerator Leap()
    {
        busy = true;
        Vector2 target = enemy.ClampToRoom(PlayerPos);
        Telegraph(target, 2f, 0.9f, Mathf.RoundToInt(enemy.damage * 1.4f));
        yield return new WaitForSeconds(0.9f);
        transform.position = target;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.position = target;
        FireRing(8);
        busy = false;
    }

    IEnumerator Blink()
    {
        busy = true;
        if (EffectsManager.Instance != null) EffectsManager.Instance.SpawnDeathEffect(transform.position);
        yield return new WaitForSeconds(0.4f);
        Vector2 target = enemy.ClampToRoom(PlayerPos + Random.insideUnitCircle.normalized * 4.5f);
        transform.position = target;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.position = target;
        if (EffectsManager.Instance != null) EffectsManager.Instance.SpawnDeathEffect(target);
        FireSpread(7);
        busy = false;
    }

    IEnumerator VoidBeams(float duration)
    {
        busy = true;
        float t = 0f, angle = 0f;
        while (t < duration)
        {
            for (int k = 0; k < 4; k++)
            {
                float a = angle + k * 90f * Mathf.Deg2Rad;
                enemy.FireBullet(new Vector2(Mathf.Cos(a), Mathf.Sin(a)), enemy.damage / 3, 11f, 0.35f);
            }
            angle += 14f * Mathf.Deg2Rad;
            t += 0.12f;
            yield return new WaitForSeconds(0.12f);
        }
        busy = false;
    }

    // Притягивает игрока к боссу
    IEnumerator GravityPull(float duration)
    {
        Rigidbody2D prb = player != null ? player.GetComponent<Rigidbody2D>() : null;
        float t = 0f;
        while (t < duration && prb != null)
        {
            t += Time.deltaTime;
            if (Vector2.Distance(prb.position, transform.position) > 1.6f)
                prb.position = Vector2.MoveTowards(prb.position, transform.position, 4.5f * Time.deltaTime);
            yield return null;
        }
    }
}
