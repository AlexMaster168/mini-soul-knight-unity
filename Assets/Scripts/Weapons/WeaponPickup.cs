using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public string weaponName = "";
    public float enableTime;

    // подсказка "E - заменить" для HUD
    public static string hintWeapon;
    public static float hintTime = -10f;

    private Vector3 startPos;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (string.IsNullOrEmpty(weaponName) || !GameData.Weapons.ContainsKey(weaponName))
        {
            // случайное оружие, которого у игрока ещё нет (если такие остались)
            string[] all = GameData.LootableWeapons();
            Inventory inv = Inventory.Instance;
            System.Collections.Generic.List<string> fresh = new System.Collections.Generic.List<string>();
            foreach (string w in all)
                if (inv == null || !inv.weapons.Contains(w)) fresh.Add(w);
            weaponName = fresh.Count > 0 ? fresh[Random.Range(0, fresh.Count)] : all[Random.Range(0, all.Length)];
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = PixelArt.Weapon(weaponName);
            spriteRenderer.sortingOrder = 8;
            transform.localScale = Vector3.one * 0.9f;
        }
    }

    void Update()
    {
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * 2f) * 0.15f;

        PlayerController player = PlayerController.Instance;
        if (player == null) return;
        Inventory inv = player.GetComponent<Inventory>();
        if (inv == null) return;

        float dist = Vector2.Distance(transform.position, player.transform.position);
        if (Time.time >= enableTime && dist < 2.8f)
        {
            string action = inv.weapons.Contains(weaponName)
                ? "You already own this - picking it up gives 25 gold"
                : (!inv.IsFull ? "Step closer to pick it up"
                               : "[E] Swap with current weapon (" + inv.weapons[inv.currentWeaponIndex] + ")");
            WeaponInfoUI.Request(weaponName, dist, action);
        }
        if (Time.time < enableTime || dist >= 1.2f) return;

        if (inv.weapons.Contains(weaponName))
        {
            // дубликат - превращаем в золото
            if (ScoreManager.Instance != null) ScoreManager.Instance.AddGold(25);
            Consume();
        }
        else if (!inv.IsFull)
        {
            inv.AddWeapon(weaponName);
            Consume();
        }
        else
        {
            HudHint.Show("[E] swap current weapon for " + weaponName);
            if (Input.GetKeyDown(KeyCode.E) && inv.AddWeapon(weaponName, true))
                Consume();
        }
    }

    void Consume()
    {
        if (EffectsManager.Instance != null)
            EffectsManager.Instance.SpawnPickupEffect(transform.position, GetRarityColor());
        Destroy(gameObject);
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
