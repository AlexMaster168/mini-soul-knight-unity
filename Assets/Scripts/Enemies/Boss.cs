using UnityEngine;

public class Boss : MonoBehaviour
{
    private Enemy enemy;
    private int phase = 1;
    private float phaseTimer;
    private float specialTimer;
    private Transform player;
    private SpriteRenderer sr;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        sr = GetComponent<SpriteRenderer>();
        phaseTimer = 10f;
        specialTimer = 3f;
    }

    void Update()
    {
        if (enemy == null || player == null) return;

        phaseTimer -= Time.deltaTime;
        specialTimer -= Time.deltaTime;

        float hpRatio = (float)enemy.currentHealth / enemy.maxHealth;

        if (hpRatio < 0.3f && phase < 3)
            EnterPhase(3);
        else if (hpRatio < 0.6f && phase < 2)
            EnterPhase(2);

        if (specialTimer <= 0)
        {
            PerformSpecial();
            specialTimer = phase == 3 ? 1.5f : phase == 2 ? 2.5f : 3.5f;
        }

        if (sr != null)
        {
            float pulse = Mathf.Sin(Time.time * (phase * 2f)) * 0.3f;
            Color baseColor = phase == 3 ? Color.red : phase == 2 ? new Color(1f, 0.5f, 0f) : Color.white;
            sr.color = Color.Lerp(baseColor, Color.white, 0.5f + pulse);
        }
    }

    void EnterPhase(int newPhase)
    {
        phase = newPhase;
        enemy.moveSpeed *= 1.3f;
        enemy.damage += 5;

        if (EffectsManager.Instance != null)
            EffectsManager.Instance.SpawnDeathEffect(transform.position);
    }

    void PerformSpecial()
    {
        switch (phase)
        {
            case 1:
                FireSpread(5);
                break;
            case 2:
                FireSpread(8);
                SummonMinion();
                break;
            case 3:
                FireRing(12);
                FireSpread(3);
                break;
        }
    }

    void FireSpread(int count)
    {
        Vector2 toPlayer = (player.position - transform.position).normalized;
        for (int i = 0; i < count; i++)
        {
            float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            float spread = (i - count / 2f) * 15f;
            Vector2 dir = Quaternion.Euler(0, 0, spread) * toPlayer;
            GameSetup.Instance.SpawnEnemyBullet(transform.position, dir, enemy.damage / 2);
        }
    }

    void FireRing(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            GameSetup.Instance.SpawnEnemyBullet(transform.position, dir, enemy.damage / 3);
        }
    }

    void SummonMinion()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * 3f;
            if (enemy.roomManager != null)
            {
                enemy.roomManager.enemiesAlive++;
                enemy.roomManager.SpawnEnemy(pos, "Slime", enemy.enemyFloor);
            }
        }
    }
}
