using UnityEngine;

// Уникальное поведение отдельных типов врагов. Enemy.Update зовёт Tick():
// true  = поведение полностью обработано здесь (погоня/атака/стрельба Enemy пропускаются)
// false = дальше работает стандартная логика Enemy
public class EnemyAbility : MonoBehaviour
{
    public string kind;

    private Enemy e;
    private float timer, timer2, timer3;
    private int state, counter;
    private Vector2 dir, target;
    private float angle;
    private bool flag;
    private GameObject line;
    private GameObject shieldPivot;
    private Vector2 facing = Vector2.right;

    void Start()
    {
        // Enemy добавляется после этого компонента, поэтому берём его только в Start
        e = GetComponent<Enemy>();

        switch (kind)
        {
            case "Turret":
            case "Sentry":
            case "Mortar":
            case "Blinker":
            case "ShieldKnight":
                e.noKnockback = true;
                break;
        }

        timer = Random.Range(0.6f, 1.4f);
        if (kind == "ShieldKnight") CreateShield();
    }

    void OnDestroy()
    {
        if (line != null) Destroy(line);
    }

    public bool Tick(float dist)
    {
        Transform p = e.playerTransform;
        if (p == null) return false;

        switch (kind)
        {
            case "Turret": TickTurret(p, dist); return true;
            case "Sentry": TickSentry(p, dist); return true;
            case "Charger": TickCharger(p, dist); return true;
            case "Shaman": TickShaman(p, dist); return true;
            case "Blinker": TickBlinker(p, dist); return true;
            case "Mortar": TickMortar(p, dist); return true;
            case "Marksman": TickMarksman(p, dist); return true;
            case "Ninja": TickNinja(p, dist); return true;
            case "ShieldKnight": TickShield(p); return false;
            default: return false;
        }
    }

    public int ModifyDamage(int dmg, Vector2 hitDir)
    {
        if (kind == "ShieldKnight" && hitDir.sqrMagnitude > 0.01f)
        {
            // щит держит удары, летящие в лицо
            if (Vector2.Dot(facing, -hitDir.normalized) > 0.35f)
            {
                if (EffectsManager.Instance != null)
                    EffectsManager.Instance.SpawnBulletImpact((Vector2)transform.position + facing * 0.9f, facing);
                return Mathf.Max(1, dmg / 5);
            }
        }
        return dmg;
    }

    public void OnDeath()
    {
        if (kind == "BigSlime" && e.roomManager != null)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * 1.2f;
                pos = e.ClampToRoom(pos);
                e.roomManager.enemiesAlive++;
                e.roomManager.SpawnEnemy(pos, "MiniSlime", e.enemyFloor);
            }
        }
    }

    // ---------- вспомогательное ----------

    Vector2 Pos { get { return transform.position; } }

    static Vector2 Rot(Vector2 v, float deg) { return Quaternion.Euler(0, 0, deg) * v; }

    void Flash(Color c, float speed = 12f)
    {
        if (e.Sprite != null)
            e.Sprite.color = Color.Lerp(e.BaseColor, c, Mathf.PingPong(Time.time * speed, 1f));
    }

    GameObject MakeLine(Color c)
    {
        GameObject g = new GameObject("Telegraph");
        SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateSquare(4, Color.white);
        sr.color = c;
        sr.sortingOrder = 9;
        return g;
    }

    static void SetLine(GameObject g, Vector2 from, Vector2 to, float width)
    {
        Vector2 d = to - from;
        g.transform.position = (from + to) * 0.5f;
        g.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
        g.transform.localScale = new Vector3(d.magnitude, width, 1f);
    }

    void AfterImage()
    {
        if (e.Sprite == null) return;
        GameObject g = new GameObject("AfterImage");
        g.transform.position = transform.position;
        g.transform.localScale = transform.localScale;
        SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = e.Sprite.sprite;
        sr.flipX = e.Sprite.flipX;
        sr.color = new Color(0.7f, 0.5f, 1f, 0.5f);
        sr.sortingOrder = 9;
        ParticleMover pm = g.AddComponent<ParticleMover>();
        pm.velocity = Vector2.zero;
        pm.lifetime = 0.25f;
    }

    void Ring(Vector2 pos, float radius, Color c, float duration)
    {
        GameObject g = new GameObject("Marker");
        g.transform.position = pos;
        SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(32, c);
        sr.sortingOrder = 2;
        TimedFx fx = g.AddComponent<TimedFx>();
        fx.duration = duration;
        fx.endScale = radius * 2f;
    }

    // Держать дистанцию: отходим если слишком близко, подходим если далеко, иначе стрейфим
    void KeepDistance(Transform p, float dist, float minD, float maxD, float speedMul = 1f)
    {
        Vector2 to = ((Vector2)p.position - Pos).normalized;
        Vector2 move;
        if (dist < minD) move = -to;
        else if (dist > maxD) move = to;
        else move = Vector2.Perpendicular(to) * Mathf.Sin(Time.time * 1.5f + GetInstanceID());
        e.MoveBy(move * e.moveSpeed * speedMul * Time.deltaTime);
    }

    // ---------- Turret: залп очередью после мигания ----------
    void TickTurret(Transform p, float dist)
    {
        timer -= Time.deltaTime;
        if (state == 0)
        {
            if (dist < e.shootRange && timer <= 0f) { state = 1; timer = 0.5f; }
        }
        else if (state == 1)
        {
            Flash(new Color(1f, 0.35f, 0.35f), 16f);
            if (timer <= 0f) { state = 2; counter = 3; timer2 = 0f; e.RestoreColor(); }
        }
        else
        {
            timer2 -= Time.deltaTime;
            if (timer2 <= 0f)
            {
                Vector2 to = ((Vector2)p.position - Pos).normalized;
                e.FireBullet(to, e.damage / 2, 12f, 0.35f);
                counter--;
                timer2 = 0.12f;
                if (counter <= 0) { state = 0; timer = e.shootCooldown; }
            }
        }
        if (e.Sprite != null) e.Sprite.flipX = p.position.x < transform.position.x;
    }

    // ---------- Sentry: вращающаяся спираль пуль ----------
    void TickSentry(Transform p, float dist)
    {
        timer -= Time.deltaTime;
        timer2 -= Time.deltaTime;
        if (dist > 16f) return;

        if (state == 0)
        {
            transform.localScale = e.BaseScale * (1f + Mathf.Sin(Time.time * 20f) * 0.05f);
            if (timer2 <= 0f)
            {
                angle += 23f;
                for (int k = 0; k < 2; k++)
                    e.FireBullet(Rot(Vector2.right, angle + k * 180f), e.damage / 2, 7.5f, 0.3f);
                timer2 = 0.16f;
            }
            if (timer <= 0f) { state = 1; timer = 1.6f; transform.localScale = e.BaseScale; }
        }
        else if (timer <= 0f)
        {
            state = 0;
            timer = 3f;
            angle = Random.Range(0f, 360f);
        }
    }

    // ---------- Charger: разгон с телеграфом ----------
    void TickCharger(Transform p, float dist)
    {
        float dt = Time.deltaTime;
        Vector2 to = (Vector2)p.position - Pos;

        switch (state)
        {
            case 0: // погоня
                e.MoveBy(to.normalized * e.moveSpeed * dt);
                timer -= dt;
                timer2 -= dt;
                if (dist < e.attackRange && timer2 <= 0f)
                {
                    PlayerController.Instance.TakeDamage(e.damage / 2);
                    timer2 = 1f;
                }
                if (timer <= 0f && dist < 10f && dist > 2.5f)
                {
                    state = 1;
                    timer = 0.9f;
                    flag = false;
                    line = MakeLine(new Color(1f, 0.2f, 0.2f, 0.35f));
                }
                break;

            case 1: // замах: тянем линию, в конце фиксируем направление
                timer -= dt;
                if (timer > 0.3f) dir = to.normalized;
                Flash(new Color(1f, 0.3f, 0.3f), 14f);
                SetLine(line, Pos, Pos + dir * 9f, timer > 0.3f ? 0.25f : 0.5f);
                if (timer <= 0f)
                {
                    Destroy(line);
                    e.RestoreColor();
                    state = 2;
                    timer = 0.75f;
                    timer3 = 0f;
                }
                break;

            case 2: // рывок
                timer -= dt;
                timer3 -= dt;
                bool hitWall = e.MoveBy(dir * 15f * dt, true);
                if (timer3 <= 0f) { AfterImage(); timer3 = 0.05f; }
                if (!flag && Vector2.Distance(Pos, p.position) < 1.1f)
                {
                    flag = true;
                    PlayerController.Instance.TakeDamage(e.damage * 2);
                }
                if (hitWall)
                {
                    state = 3;
                    timer = 1.3f; // оглушён об стену
                    if (PostProcessEffect.Instance != null)
                        PostProcessEffect.Instance.TriggerScreenShake(0.15f, 0.15f);
                    if (EffectsManager.Instance != null)
                        EffectsManager.Instance.SpawnHitEffect(Pos);
                }
                else if (timer <= 0f)
                {
                    state = 3;
                    timer = 0.6f;
                }
                break;

            default: // отдых
                timer -= dt;
                if (e.Sprite != null && timer > 0.3f)
                    e.Sprite.color = Color.Lerp(e.BaseColor, new Color(0.7f, 0.7f, 1f), Mathf.PingPong(Time.time * 8f, 1f));
                if (timer <= 0f)
                {
                    e.RestoreColor();
                    state = 0;
                    timer = Random.Range(1.2f, 2.2f);
                }
                break;
        }
    }

    // ---------- Shaman: лечит союзников, иначе стреляет ----------
    void TickShaman(Transform p, float dist)
    {
        float dt = Time.deltaTime;
        timer -= dt;
        timer2 -= dt;
        KeepDistance(p, dist, 5.5f, 8.5f);

        if (timer <= 0f)
        {
            Enemy wounded = FindWounded();
            if (wounded != null)
            {
                Heal(wounded);
                timer = 3.2f;
            }
            else timer = 1f;
        }

        if (timer2 <= 0f && dist < e.shootRange)
        {
            Vector2 to = ((Vector2)p.position - Pos).normalized;
            e.FireBullet(to, e.damage / 2, 9f, 0.45f);
            timer2 = e.shootCooldown;
        }
    }

    Enemy FindWounded()
    {
        Enemy best = null;
        float bestRatio = 0.75f;
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (g == gameObject) continue;
            Enemy o = g.GetComponent<Enemy>();
            if (o == null || o.IsSpawning || o.maxHealth <= 0) continue;
            float r = (float)o.currentHealth / o.maxHealth;
            if (r < bestRatio) { bestRatio = r; best = o; }
        }
        return best;
    }

    void Heal(Enemy o)
    {
        o.currentHealth = Mathf.Min(o.maxHealth, o.currentHealth + Mathf.CeilToInt(o.maxHealth * 0.35f));
        o.RefreshHealthBar();

        GameObject beam = MakeLine(new Color(0.4f, 1f, 0.5f, 0.7f));
        SetLine(beam, Pos, o.transform.position, 0.15f);
        Destroy(beam, 0.25f);
        if (EffectsManager.Instance != null)
            EffectsManager.Instance.SpawnPickupEffect(o.transform.position, new Color(0.4f, 1f, 0.5f));
    }

    // ---------- Blinker: телепорт + веер пуль ----------
    void TickBlinker(Transform p, float dist)
    {
        float dt = Time.deltaTime;
        timer -= dt;
        SpriteRenderer sr = e.Sprite;

        switch (state)
        {
            case 0: // парит
                e.MoveBy(new Vector2(Mathf.Cos(Time.time * 1.5f), Mathf.Sin(Time.time * 2f)) * 0.8f * dt);
                if (timer <= 0f)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 cand = (Vector2)p.position + Rot(Vector2.right, Random.Range(0f, 360f)) * Random.Range(4f, 6.5f);
                        cand = e.ClampToRoom(cand, 1.2f);
                        if (Vector2.Distance(cand, p.position) > 3.5f) { target = cand; break; }
                        target = cand;
                    }
                    Ring(target, 0.9f, new Color(1f, 0.3f, 0.9f, 0.5f), 0.6f);
                    state = 1;
                    timer = 0.6f;
                }
                break;

            case 1: // растворяется
                if (sr != null) sr.color = new Color(e.BaseColor.r, e.BaseColor.g, e.BaseColor.b, Mathf.Clamp01(timer / 0.4f) * 0.9f + 0.1f);
                if (timer <= 0f)
                {
                    transform.position = target;
                    if (EffectsManager.Instance != null) EffectsManager.Instance.SpawnPickupEffect(target, new Color(1f, 0.4f, 0.9f));
                    state = 2;
                    timer = 0.35f;
                }
                break;

            default: // проявляется и стреляет
                if (sr != null) sr.color = new Color(e.BaseColor.r, e.BaseColor.g, e.BaseColor.b, 1f - Mathf.Clamp01(timer / 0.35f) * 0.9f);
                if (timer <= 0f)
                {
                    if (sr != null) sr.color = e.BaseColor;
                    Vector2 to = ((Vector2)p.position - Pos).normalized;
                    for (int i = -2; i <= 2; i++)
                        e.FireBullet(Rot(to, i * 16f), e.damage / 2, 9f, 0.4f);
                    state = 0;
                    timer = Random.Range(1.4f, 2.2f);
                }
                break;
        }
    }

    // ---------- ShieldKnight: щит поворачивается медленно, обойди со спины ----------
    void CreateShield()
    {
        shieldPivot = new GameObject("ShieldPivot");
        shieldPivot.transform.SetParent(transform, false);
        GameObject s = new GameObject("Shield");
        s.transform.SetParent(shieldPivot.transform, false);
        s.transform.localPosition = new Vector3(0.85f, 0f, 0f);
        SpriteRenderer sr = s.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArt.Shield();
        sr.sortingOrder = 11;
    }

    void TickShield(Transform p)
    {
        Vector2 to = ((Vector2)p.position - Pos).normalized;
        float cur = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
        float want = Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg;
        float next = Mathf.MoveTowardsAngle(cur, want, 110f * Time.deltaTime);
        facing = new Vector2(Mathf.Cos(next * Mathf.Deg2Rad), Mathf.Sin(next * Mathf.Deg2Rad));
        if (shieldPivot != null)
            shieldPivot.transform.rotation = Quaternion.Euler(0, 0, next);
    }

    // ---------- Mortar: навесной обстрел с маркером на земле ----------
    void TickMortar(Transform p, float dist)
    {
        timer -= Time.deltaTime;
        KeepDistance(p, dist, 6.5f, 10.5f);

        if (timer <= 0f && dist < e.shootRange + 2f)
        {
            GameObject shell = new GameObject("MortarShell");
            shell.transform.position = Pos + Vector2.up * 0.5f;
            MortarShell ms = shell.AddComponent<MortarShell>();
            ms.Init(Pos + Vector2.up * 0.5f, p.position, e.damage, 1.7f, 1.1f);
            timer = e.shootCooldown;
            if (PostProcessEffect.Instance != null)
                PostProcessEffect.Instance.TriggerScreenShake(0.03f, 0.05f);
        }
    }

    // ---------- Marksman: лазерный прицел, потом тяжёлый выстрел ----------
    void TickMarksman(Transform p, float dist)
    {
        float dt = Time.deltaTime;
        timer -= dt;

        if (state == 0)
        {
            KeepDistance(p, dist, 6.5f, 12f, 1.3f);
            if (timer <= 0f && dist < e.shootRange)
            {
                state = 1;
                timer = 1.4f;
                line = MakeLine(new Color(1f, 0.15f, 0.15f, 0.2f));
            }
        }
        else
        {
            if (timer > 0.3f) dir = ((Vector2)p.position - Pos).normalized;
            float k = 1f - timer / 1.4f;
            SpriteRenderer lr = line.GetComponent<SpriteRenderer>();
            lr.color = new Color(1f, 0.15f, 0.15f, Mathf.Lerp(0.15f, 0.9f, k));
            SetLine(line, Pos, Pos + dir * 22f, timer > 0.3f ? 0.06f : 0.14f);
            if (e.Sprite != null) e.Sprite.flipX = dir.x < 0f;

            if (timer <= 0f)
            {
                Destroy(line);
                e.FireBullet(dir, e.damage, 28f, 0.8f, BulletStyle.Streak, new Color(1f, 0.35f, 0.25f));
                if (PostProcessEffect.Instance != null)
                    PostProcessEffect.Instance.TriggerScreenShake(0.06f, 0.06f);
                state = 0;
                timer = e.shootCooldown;
            }
        }
    }

    // ---------- Ninja: веер сюрикенов, рывок в сторону, ближний удар ----------
    void TickNinja(Transform p, float dist)
    {
        float dt = Time.deltaTime;
        timer -= dt;
        timer2 -= dt;
        Vector2 to = (Vector2)p.position - Pos;

        if (state == 0)
        {
            if (dist > 1.1f) e.MoveBy(to.normalized * e.moveSpeed * dt);
            if (dist < e.attackRange && timer2 <= 0f)
            {
                PlayerController.Instance.TakeDamage(e.damage);
                timer2 = e.attackCooldown;
            }
            if (timer <= 0f && dist < e.shootRange && dist > 2.5f)
            {
                Vector2 aim = to.normalized;
                for (int i = -1; i <= 1; i++)
                    e.FireBullet(Rot(aim, i * 14f), e.damage / 2, 12f, 0.5f, BulletStyle.Star, new Color(0.85f, 0.85f, 1f));
                state = 1;
                timer3 = 0.5f;
                Vector2 side = Vector2.Perpendicular(aim) * (Random.value < 0.5f ? -1f : 1f);
                target = e.ClampToRoom((Vector2)p.position + side * Random.Range(2.5f, 4f) - aim * Random.Range(0f, 1.5f), 1f);
            }
        }
        else
        {
            timer3 -= dt;
            Vector2 d = target - Pos;
            AfterImage();
            if (d.magnitude < 0.5f || timer3 <= 0f)
            {
                state = 0;
                timer = Random.Range(1.6f, 2.4f);
            }
            else
                e.MoveBy(d.normalized * 20f * dt, true);
        }
    }
}

// Навесной снаряд миномёта: летит по дуге, на земле красный маркер, урон по площади
public class MortarShell : MonoBehaviour
{
    private Vector2 from, to;
    private int damage;
    private float radius, flight, t;
    private GameObject marker, fill;
    private SpriteRenderer sr;

    public void Init(Vector2 from, Vector2 to, int damage, float radius, float flight)
    {
        this.from = from;
        this.to = to;
        this.damage = damage;
        this.radius = radius;
        this.flight = flight;

        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(8, new Color(0.15f, 0.12f, 0.15f));
        sr.sortingOrder = 16;
        transform.localScale = Vector3.one * 0.4f;

        marker = new GameObject("MortarMarker");
        marker.transform.position = to;
        SpriteRenderer m = marker.AddComponent<SpriteRenderer>();
        m.sprite = SpriteGenerator.CreateCircle(32, new Color(1f, 0.2f, 0.1f, 0.25f));
        m.sortingOrder = 2;
        marker.transform.localScale = Vector3.one * radius * 2f;

        fill = new GameObject("MortarFill");
        fill.transform.position = to;
        SpriteRenderer f = fill.AddComponent<SpriteRenderer>();
        f.sprite = SpriteGenerator.CreateCircle(32, new Color(1f, 0.25f, 0.1f, 0.4f));
        f.sortingOrder = 3;
        fill.transform.localScale = Vector3.zero;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / flight);
        Vector2 pos = Vector2.Lerp(from, to, k);
        pos.y += Mathf.Sin(k * Mathf.PI) * 3.5f;
        transform.position = pos;
        fill.transform.localScale = Vector3.one * radius * 2f * k;

        if (k >= 1f)
        {
            Land();
            Destroy(marker);
            Destroy(fill);
            Destroy(gameObject);
        }
    }

    void Land()
    {
        PlayerController pc = PlayerController.Instance;
        if (pc != null && Vector2.Distance(pc.transform.position, to) <= radius)
            pc.TakeDamage(damage);

        GameObject ring = new GameObject("MortarBlast");
        ring.transform.position = to;
        SpriteRenderer r = ring.AddComponent<SpriteRenderer>();
        r.sprite = SpriteGenerator.CreateCircle(32, new Color(1f, 0.55f, 0.15f, 0.7f));
        r.sortingOrder = 18;
        TimedFx fx = ring.AddComponent<TimedFx>();
        fx.duration = 0.3f;
        fx.endScale = radius * 2.2f;

        if (EffectsManager.Instance != null)
            EffectsManager.Instance.SpawnDeathEffect(to);
        if (PostProcessEffect.Instance != null)
            PostProcessEffect.Instance.TriggerScreenShake(0.12f, 0.15f);
    }
}
