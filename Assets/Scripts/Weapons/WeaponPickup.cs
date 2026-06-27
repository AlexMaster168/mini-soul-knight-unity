using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public string weaponName = "Pistol";

    private Vector3 startPos;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = 8;

        if (string.IsNullOrEmpty(weaponName) || !GameData.Weapons.ContainsKey(weaponName))
        {
            string[] keys = new string[GameData.Weapons.Count];
            GameData.Weapons.Keys.CopyTo(keys, 0);
            weaponName = keys[Random.Range(0, keys.Length)];
        }

        UpdateVisual();
    }

    void Update()
    {
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * 2f) * 0.15f;

        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        if (Vector2.Distance(transform.position, player.transform.position) < 1f)
        {
            Inventory inv = player.GetComponent<Inventory>();
            if (inv != null)
                inv.AddWeapon(weaponName);
            else
                player.EquipWeapon(weaponName);

            Debug.Log("Picked up: " + weaponName);

            if (EffectsManager.Instance != null)
                EffectsManager.Instance.SpawnPickupEffect(transform.position, GetRarityColor());
            Destroy(gameObject);
        }
    }

    void UpdateVisual()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = GetRarityColor();
    }

    Color GetRarityColor()
    {
        if (!GameData.Weapons.ContainsKey(weaponName)) return Color.white;
        WeaponData data = GameData.Weapons[weaponName];
        if (data.damage >= 100) return new Color(1f, 0.6f, 0f);
        if (data.damage >= 50) return new Color(0.7f, 0.2f, 1f);
        if (data.damage >= 25) return new Color(0.2f, 0.5f, 1f);
        return Color.white;
    }
}
