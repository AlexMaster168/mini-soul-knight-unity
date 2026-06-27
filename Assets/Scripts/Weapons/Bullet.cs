using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    public bool isEnemyBullet = false;

    private Vector2 direction;
    private int damage;
    private bool pierce;
    private bool initialized;
    private float checkRadius = 0.3f;

    public void Initialize(Vector2 dir, int dmg, float spd = 20f, float size = 0.4f, bool p = false, bool enemy = false)
    {
        if (initialized) return;
        initialized = true;
        direction = dir;
        damage = dmg;
        pierce = p;
        isEnemyBullet = enemy;
        speed = spd > 0 ? spd : 30f;
        transform.localScale = Vector3.one * size;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!initialized) return;
        transform.Translate(direction * speed * Time.deltaTime);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, checkRadius);

        foreach (Collider2D hit in hits)
        {
            if (isEnemyBullet)
            {
                if (hit.CompareTag("Player"))
                {
                    PlayerController pc = hit.GetComponent<PlayerController>();
                    if (pc != null) pc.TakeDamage(damage);
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                if (hit.CompareTag("Enemy"))
                {
                    Enemy enemy = hit.GetComponent<Enemy>();
                    if (enemy != null) enemy.TakeDamage(damage);
                    if (!pierce)
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
            }
        }
    }
}
