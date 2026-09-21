using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        DamageBoost,
        SpeedBoost,
        HealthRestore,
        MultiShot,
        EnergyRestore
    }

    public PowerUpType type;
    public float duration = 5f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * 2f) * 0.15f;

        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        if (Vector2.Distance(transform.position, player.transform.position) < 1f)
        {
            ApplyPowerUp(player);

            Color effectColor = Color.white;
            switch (type)
            {
                case PowerUpType.DamageBoost: effectColor = Color.red; break;
                case PowerUpType.SpeedBoost: effectColor = Color.cyan; break;
                case PowerUpType.HealthRestore: effectColor = Color.green; break;
                case PowerUpType.MultiShot: effectColor = Color.yellow; break;
                case PowerUpType.EnergyRestore: effectColor = new Color(0.3f, 0.5f, 1f); break;
            }

            if (EffectsManager.Instance != null)
                EffectsManager.Instance.SpawnPickupEffect(transform.position, effectColor);
            Destroy(gameObject);
        }
    }

    void ApplyPowerUp(PlayerController player)
    {
        switch (type)
        {
            case PowerUpType.DamageBoost:
                StartCoroutine(DamageBoost(player));
                break;
            case PowerUpType.SpeedBoost:
                StartCoroutine(SpeedBoost(player));
                break;
            case PowerUpType.HealthRestore:
                player.currentHealth = Mathf.Min(player.currentHealth + 50, player.maxHealth);
                break;
            case PowerUpType.MultiShot:
                StartCoroutine(MultiShotBoost(player));
                break;
            case PowerUpType.EnergyRestore:
                player.currentEnergy = Mathf.Min(player.currentEnergy + 80, player.maxEnergy);
                break;
        }
    }

    System.Collections.IEnumerator DamageBoost(PlayerController player)
    {
        player.damageBoosts++;
        yield return new WaitForSeconds(duration);
        player.damageBoosts--;
    }

    System.Collections.IEnumerator SpeedBoost(PlayerController player)
    {
        if (player.speedBoosts++ == 0) player.moveSpeed *= 1.5f;
        yield return new WaitForSeconds(duration);
        if (--player.speedBoosts == 0) player.moveSpeed /= 1.5f;
    }

    System.Collections.IEnumerator MultiShotBoost(PlayerController player)
    {
        if (player.multiShotBoosts++ == 0) player.projectilesPerShot *= 2;
        yield return new WaitForSeconds(duration);
        if (--player.multiShotBoosts == 0) player.projectilesPerShot /= 2;
    }
}
