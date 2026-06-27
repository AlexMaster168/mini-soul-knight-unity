using UnityEngine;
using System.Collections.Generic;

public class AmbientParticles : MonoBehaviour
{
    public static AmbientParticles Instance;

    private List<GameObject> particles = new List<GameObject>();
    private float spawnTimer;
    private Transform player;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = PlayerController.Instance != null ? PlayerController.Instance.transform : null;
    }

    void Update()
    {
        if (player == null)
        {
            player = PlayerController.Instance != null ? PlayerController.Instance.transform : null;
            if (player == null) return;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer > 0.3f)
        {
            spawnTimer = 0;
            SpawnDustParticle();
            if (Random.value < 0.3f)
                SpawnFirefly();
        }

        for (int i = particles.Count - 1; i >= 0; i--)
        {
            if (particles[i] == null)
                particles.RemoveAt(i);
        }
    }

    void SpawnDustParticle()
    {
        if (particles.Count > 50) return;

        Vector3 pos = player.position + new Vector3(
            Random.Range(-12f, 12f),
            Random.Range(-8f, 8f),
            0
        );

        GameObject p = new GameObject("Dust");
        p.transform.position = pos;
        SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(4, new Color(0.8f, 0.75f, 0.6f, 0.2f));
        sr.sortingOrder = 5;
        p.transform.localScale = Vector3.one * Random.Range(0.05f, 0.12f);

        DustMover dm = p.AddComponent<DustMover>();
        dm.velocity = new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(0.2f, 0.6f));
        dm.lifetime = Random.Range(3f, 6f);

        particles.Add(p);
    }

    void SpawnFirefly()
    {
        if (particles.Count > 50) return;

        Vector3 pos = player.position + new Vector3(
            Random.Range(-10f, 10f),
            Random.Range(-7f, 7f),
            0
        );

        GameObject p = new GameObject("Firefly");
        p.transform.position = pos;
        SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(4, new Color(0.9f, 1f, 0.4f, 0.7f));
        sr.sortingOrder = 5;
        p.transform.localScale = Vector3.one * 0.06f;

        FireflyMover fm = p.AddComponent<FireflyMover>();
        fm.wanderSpeed = 1f;
        fm.lifetime = Random.Range(5f, 10f);

        particles.Add(p);
    }
}

public class DustMover : MonoBehaviour
{
    public Vector2 velocity;
    public float lifetime = 4f;
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
        velocity.y -= 0.05f * Time.deltaTime;

        if (sr != null)
        {
            float alpha = Mathf.Clamp01(1f - timer / lifetime) * 0.2f;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}

public class FireflyMover : MonoBehaviour
{
    public float wanderSpeed = 1f;
    public float lifetime = 8f;
    private float timer;
    private Vector2 wanderDir;
    private SpriteRenderer sr;
    private float glowTimer;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        wanderDir = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        timer += Time.deltaTime;
        glowTimer += Time.deltaTime * 3f;

        if (Random.value < 0.02f)
            wanderDir = Random.insideUnitCircle.normalized;

        transform.position += (Vector3)(wanderDir * wanderSpeed * Time.deltaTime);

        if (sr != null)
        {
            float glow = 0.4f + Mathf.Sin(glowTimer) * 0.3f;
            float fade = Mathf.Clamp01(1f - timer / lifetime);
            sr.color = new Color(0.9f, 1f, 0.4f, glow * fade);
            transform.localScale = Vector3.one * (0.06f + Mathf.Sin(glowTimer) * 0.02f);
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
