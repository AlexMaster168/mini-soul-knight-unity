using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    public int goldValue = 5;
    private Vector3 startPos;
    private float lifetime = 20f;
    private bool collected;

    void Start()
    {
        startPos = transform.position;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(1f, 0.85f, 0.1f, 1f);
    }

    void Update()
    {
        if (collected) return;

        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * 4f) * 0.05f;

        lifetime -= Time.deltaTime;
        if (lifetime <= 0)
        {
            Destroy(gameObject);
            return;
        }

        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        float dist = Vector2.Distance(startPos, player.transform.position);

        if (dist < 5f)
        {
            Vector2 dir = ((Vector2)player.transform.position - (Vector2)startPos).normalized;
            float speed = 18f;
            startPos += (Vector3)(dir * speed * Time.deltaTime);
        }

        if (dist < 1.2f)
        {
            collected = true;
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddGold(goldValue);
            if (EffectsManager.Instance != null)
                EffectsManager.Instance.SpawnPickupEffect(transform.position, new Color(1f, 0.85f, 0.1f));
            Destroy(gameObject);
        }
    }
}
