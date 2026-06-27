using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    private float damageTimer;
    private SpriteRenderer sr;
    private bool isActive;
    private float cooldown;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (cooldown > 0)
            cooldown -= Time.deltaTime;

        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.transform.position);
        if (dist < 0.8f && cooldown <= 0)
        {
            player.TakeDamage(5);
            cooldown = 0.8f;

            if (sr != null)
            {
                sr.color = new Color(1f, 0.3f, 0.3f);
                Invoke(nameof(ResetColor), 0.15f);
            }

            if (ProceduralMusic.Instance != null)
                ProceduralMusic.Instance.PlaySFX("hit");

            SpawnSpikeEffect();
        }
    }

    void ResetColor()
    {
        if (sr != null)
            sr.color = new Color(0.7f, 0.7f, 0.7f, 0.9f);
    }

    void SpawnSpikeEffect()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject p = new GameObject("SpikeParticle");
            p.transform.position = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
            SpriteRenderer psr = p.AddComponent<SpriteRenderer>();
            psr.sprite = SpriteGenerator.CreateCircle(4, new Color(1f, 0.5f, 0.2f, 0.8f));
            psr.sortingOrder = 20;
            p.transform.localScale = Vector3.one * 0.08f;
            ParticleMover pm = p.AddComponent<ParticleMover>();
            pm.velocity = new Vector2(Random.Range(-2f, 2f), Random.Range(1f, 3f));
            pm.lifetime = 0.3f;
            pm.shrink = true;
        }
    }
}
