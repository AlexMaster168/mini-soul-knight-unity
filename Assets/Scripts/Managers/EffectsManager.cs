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
        for (int i = 0; i < 5; i++)
        {
            GameObject particle = new GameObject("HitParticle");
            particle.transform.position = position;
            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateCircle(8, new Color(1f, 0.9f, 0.2f, 0.9f));
            sr.sortingOrder = 20;
            particle.transform.localScale = Vector3.one * Random.Range(0.1f, 0.2f);

            ParticleMover mover = particle.AddComponent<ParticleMover>();
            mover.velocity = Random.insideUnitCircle.normalized * Random.Range(3f, 6f);
            mover.lifetime = 0.3f;
        }
    }

    public void SpawnDeathEffect(Vector2 position)
    {
        for (int i = 0; i < 12; i++)
        {
            GameObject particle = new GameObject("DeathParticle");
            particle.transform.position = position;
            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateCircle(8, new Color(1f, 0.3f, 0.1f, 0.9f));
            sr.sortingOrder = 20;
            particle.transform.localScale = Vector3.one * Random.Range(0.15f, 0.3f);

            ParticleMover mover = particle.AddComponent<ParticleMover>();
            mover.velocity = Random.insideUnitCircle.normalized * Random.Range(4f, 8f);
            mover.lifetime = 0.5f;
            mover.shrink = true;
        }
    }

    public void SpawnPickupEffect(Vector2 position, Color color)
    {
        for (int i = 0; i < 8; i++)
        {
            GameObject particle = new GameObject("PickupParticle");
            particle.transform.position = position;
            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateCircle(8, color);
            sr.sortingOrder = 20;
            particle.transform.localScale = Vector3.one * 0.15f;

            ParticleMover mover = particle.AddComponent<ParticleMover>();
            mover.velocity = Random.insideUnitCircle.normalized * Random.Range(2f, 5f);
            mover.lifetime = 0.4f;
            mover.shrink = true;
        }
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
        velocity *= 0.95f;

        if (shrink && sr != null)
        {
            float alpha = 1f - (timer / lifetime);
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            transform.localScale *= 0.97f;
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
