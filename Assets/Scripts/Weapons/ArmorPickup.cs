using UnityEngine;

public class ArmorPickup : MonoBehaviour
{
    public int armorAmount = 50;
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
            player.armor = Mathf.Min(player.armor + armorAmount, player.maxArmor);
            Debug.Log("Armor +" + armorAmount + " (total: " + player.armor + ")");

            if (EffectsManager.Instance != null)
                EffectsManager.Instance.SpawnPickupEffect(transform.position, new Color(0.3f, 0.7f, 1f));
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayPickupSound();
            Destroy(gameObject);
        }
    }
}
