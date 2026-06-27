using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnHitEffect(Vector2 position)
    {
        for (int i = 0; i < 8; i++)
        {
            GameObject particle = new GameObject("HitParticle");
            particle.transform.position = position;
            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateCircle(6, new Color(1f, 0.9f, 0.2f, 0.9f));
            sr.sortingOrder = 20;
            particle.transform.localScale = Vector3.one * Random.Range(0.08f, 0.18f);

            ParticleMover mover = particle.AddComponent<ParticleMover>();
            mover.velocity = Random.insideUnitCircle.normalized * Random.Range(4f, 8f);
            mover.lifetime = 0.25f;
            mover.shrink = true;
        }

        SpawnLightFlash(position, new Color(1f, 0.9f, 0.3f, 0.5f), 1.5f, 0.15f);
    }

    public void SpawnDeathEffect(Vector2 position)
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject particle = new GameObject("DeathParticle");
            particle.transform.position = (Vector3)position + (Vector3)(Random.insideUnitCircle * 0.3f);
            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();

            float t = i / 20f;
            Color c = Color.Lerp(new Color(1f, 0.3f, 0.1f), new Color(1f, 0.8f, 0.1f), t);
            sr.sprite = SpriteGenerator.CreateCircle(8, c);
            sr.sortingOrder = 20;
            particle.transform.localScale = Vector3.one * Random.Range(0.1f, 0.25f);

            ParticleMover mover = particle.AddComponent<ParticleMover>();
            mover.velocity = Random.insideUnitCircle.normalized * Random.Range(3f, 7f);
            mover.lifetime = Random.Range(0.4f, 0.8f);
            mover.shrink = true;
        }

        SpawnSmokeCloud(position);
        SpawnLightFlash(position, new Color(1f, 0.4f, 0.1f, 0.6f), 3f, 0.3f);
    }

    public void SpawnPickupEffect(Vector2 position, Color color)
    {
        for (int i = 0; i < 12; i++)
        {
            GameObject particle = new GameObject("PickupParticle");
            particle.transform.position = position;
            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateCircle(6, color);
            sr.sortingOrder = 20;
            particle.transform.localScale = Vector3.one * 0.12f;

            ParticleMover mover = particle.AddComponent<ParticleMover>();
            float angle = (360f / 12) * i * Mathf.Deg2Rad;
            mover.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(3f, 6f);
            mover.lifetime = 0.4f;
            mover.shrink = true;
        }

        SpawnLightFlash(position, color, 1.5f, 0.2f);
    }

    public void SpawnBulletImpact(Vector2 position, Vector2 direction)
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject particle = new GameObject("ImpactParticle");
            particle.transform.position = position;
            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateCircle(4, new Color(1f, 0.8f, 0.3f, 0.8f));
            sr.sortingOrder = 20;
            particle.transform.localScale = Vector3.one * Random.Range(0.05f, 0.1f);

            ParticleMover mover = particle.AddComponent<ParticleMover>();
            Vector2 perp = new Vector2(-direction.y, direction.x);
            mover.velocity = (perp * Random.Range(-1f, 1f) + direction * Random.Range(1f, 3f)) * 3f;
            mover.lifetime = 0.2f;
            mover.shrink = true;
        }
    }

    void SpawnSmokeCloud(Vector2 position)
    {
        for (int i = 0; i < 6; i++)
        {
            GameObject smoke = new GameObject("Smoke");
            smoke.transform.position = (Vector3)position + (Vector3)(Random.insideUnitCircle * 0.5f);
            SpriteRenderer sr = smoke.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateCircle(16, new Color(0.3f, 0.3f, 0.3f, 0.4f));
            sr.sortingOrder = 19;
            smoke.transform.localScale = Vector3.one * 0.2f;

            ParticleMover mover = smoke.AddComponent<ParticleMover>();
            mover.velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(1f, 3f));
            mover.lifetime = Random.Range(0.8f, 1.5f);
            mover.shrink = true;
        }
    }

    void SpawnLightFlash(Vector2 position, Color color, float size, float duration)
    {
        GameObject flash = new GameObject("LightFlash");
        flash.transform.position = position;
        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(32, color);
        sr.sortingOrder = 18;
        flash.transform.localScale = Vector3.one * 0.1f;

        LightFlash lf = flash.AddComponent<LightFlash>();
        lf.maxSize = size;
        lf.duration = duration;
    }
}

public class ParticleMover : MonoBehaviour
{
    public Vector2 velocity;
    public float lifetime = 0.5f;
    public bool shrink = false;

    private float timer;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        velocity *= 0.93f;

        if (sr != null)
        {
            float alpha = 1f - (timer / lifetime);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);

            if (shrink)
                transform.localScale *= 0.96f;
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}

public class LightFlash : MonoBehaviour
{
    public float maxSize = 2f;
    public float duration = 0.2f;
    private float timer;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;

        if (sr != null)
        {
            float scale = Mathf.Lerp(0.1f, maxSize, t);
            transform.localScale = Vector3.one * scale;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, (1f - t) * sr.color.a);
        }

        if (timer >= duration)
            Destroy(gameObject);
    }
}
