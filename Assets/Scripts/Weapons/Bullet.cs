using UnityEngine;
using System.Collections.Generic;

public class BulletSpec
{
    public int damage = 10;
    public float speed = 20f;
    public float size = 0.4f;
    public bool pierce;
    public bool enemy;
    public float lifetime = 3f;
    public float aoeRadius;
    public bool boomerang;
    public float wobble;
    public bool trail;
    public BulletStyle style = BulletStyle.Round;
    public Color color = Color.white;
}

public class Bullet : MonoBehaviour
{
    public bool isEnemyBullet = false;

    private BulletSpec spec;
    private Vector2 direction;
    private float age;
    private float radius;
    private bool returning;
    private float trailTimer;
    private float spin;
    private readonly HashSet<Enemy> alreadyHit = new HashSet<Enemy>();
    private Vector2 lateral;
    private bool dead;

    public void Init(Vector2 dir, BulletSpec s)
    {
        spec = s;
        isEnemyBullet = s.enemy;
        direction = dir.normalized;
        lateral = new Vector2(-direction.y, direction.x);
        transform.localScale = Vector3.one * s.size;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        radius = Mathf.Clamp(s.size * (s.style == BulletStyle.Slash ? 0.55f : 0.45f), 0.15f, 1.2f);
        if (s.style == BulletStyle.Slash) radius = Mathf.Max(radius, 0.6f);
    }

    void Update()
    {
        if (spec == null || dead) return;

        age += Time.deltaTime;
        float t = age / spec.lifetime;

        Vector2 step;
        if (spec.boomerang)
        {
            PlayerController pc = PlayerController.Instance;
            if (!returning && t >= 0.5f)
            {
                returning = true;
                alreadyHit.Clear();
            }
            if (returning && pc != null)
            {
                Vector2 toPlayer = (Vector2)pc.transform.position - (Vector2)transform.position;
                if (toPlayer.magnitude < 0.6f) { Destroy(gameObject); return; }
                direction = toPlayer.normalized;
            }
            step = direction * spec.speed * Time.deltaTime;
            spin += 900f * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, 0, spin);
        }
        else
        {
            step = direction * spec.speed * Time.deltaTime;
            if (spec.wobble > 0f)
                step += lateral * Mathf.Cos(age * 40f) * spec.wobble * 0.5f * Time.deltaTime * 4f;
            if (spec.style == BulletStyle.Star)
                transform.Rotate(0, 0, 720f * Time.deltaTime);
            if (spec.style == BulletStyle.Flame)
            {
                float grow = 1f + t * 1.6f;
                transform.localScale = Vector3.one * spec.size * grow;
                radius = Mathf.Clamp(spec.size * grow * 0.45f, 0.15f, 1.2f);
            }
            if (spec.style == BulletStyle.Slash)
                transform.localScale = Vector3.one * spec.size * (0.8f + t * 0.5f);
        }

        transform.position += (Vector3)step;

        if (spec.trail)
        {
            trailTimer -= Time.deltaTime;
            if (trailTimer <= 0f) { SpawnTrail(); trailTimer = 0.03f; }
        }

        if (CheckHits()) return;

        if (age >= spec.lifetime)
        {
            if (spec.aoeRadius > 0f) Explode();
            Destroy(gameObject);
        }
    }

    bool CheckHits()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Wall"))
            {
                if (spec.style == BulletStyle.Slash) continue;
                if (spec.boomerang) { returning = true; continue; }
                if (EffectsManager.Instance != null)
                    EffectsManager.Instance.SpawnBulletImpact(transform.position, -direction);
                if (spec.aoeRadius > 0f) Explode();
                Kill();
                return true;
            }

            if (isEnemyBullet)
            {
                if (hit.CompareTag("Player"))
                {
                    PlayerController pc = hit.GetComponent<PlayerController>();
                    if (pc != null) pc.TakeDamage(spec.damage);
                    Kill();
                    return true;
                }
            }
            else if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy == null || alreadyHit.Contains(enemy)) continue;

                if (spec.aoeRadius > 0f)
                {
                    Explode();
                    Kill();
                    return true;
                }

                alreadyHit.Add(enemy);
                enemy.TakeDamage(spec.damage, direction);
                if (!spec.pierce)
                {
                    Kill();
                    return true;
                }
            }
        }
        return false;
    }

    void Kill()
    {
        dead = true;
        Destroy(gameObject);
    }

    void Explode()
    {
        Vector2 pos = transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, spec.aoeRadius);
        foreach (Collider2D h in hits)
        {
            if (!h.CompareTag("Enemy")) continue;
            Enemy e = h.GetComponent<Enemy>();
            if (e != null) e.TakeDamage(spec.damage, ((Vector2)e.transform.position - pos).normalized);
        }

        GameObject ring = new GameObject("Explosion");
        ring.transform.position = pos;
        SpriteRenderer sr = ring.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(32, new Color(1f, 0.6f, 0.15f, 0.7f));
        sr.sortingOrder = 18;
        TimedFx fx = ring.AddComponent<TimedFx>();
        fx.duration = 0.3f;
        fx.endScale = spec.aoeRadius * 2f;

        if (EffectsManager.Instance != null)
            EffectsManager.Instance.SpawnDeathEffect(pos);
        if (PostProcessEffect.Instance != null)
            PostProcessEffect.Instance.TriggerScreenShake(0.2f, 0.2f);
    }

    void SpawnTrail()
    {
        GameObject p = new GameObject("Trail");
        p.transform.position = transform.position;
        SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
        Color c = spec.style == BulletStyle.Rocket ? new Color(0.6f, 0.6f, 0.6f, 0.6f) : new Color(spec.color.r, spec.color.g, spec.color.b, 0.6f);
        sr.sprite = SpriteGenerator.CreateCircle(8, c);
        sr.sortingOrder = 14;
        p.transform.localScale = Vector3.one * Mathf.Max(0.1f, spec.size * 0.35f);
        ParticleMover pm = p.AddComponent<ParticleMover>();
        pm.velocity = Random.insideUnitCircle * 0.5f;
        pm.lifetime = 0.25f;
        pm.shrink = true;
    }
}

// Расширяющееся и затухающее кольцо (взрывы, ударные волны)
public class TimedFx : MonoBehaviour
{
    public float duration = 0.3f;
    public float endScale = 3f;
    private float t;
    private SpriteRenderer sr;
    private float startAlpha;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) startAlpha = sr.color.a;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / duration);
        transform.localScale = Vector3.one * Mathf.Lerp(0.1f, endScale, 1f - (1f - k) * (1f - k));
        if (sr != null)
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, startAlpha * (1f - k));
        if (t >= duration) Destroy(gameObject);
    }
}
