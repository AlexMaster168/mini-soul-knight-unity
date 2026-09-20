using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public const int MaxSlots = 8;

    public List<string> weapons = new List<string>();
    public int currentWeaponIndex = 0;

    // true - энергии нет, в руках запасное оружие ближнего боя
    public bool reserveActive;
    // клинок взят вручную (клавиша 9) - не убирается сам при восстановлении энергии
    public bool reserveManual;

    public bool IsFull { get { return weapons.Count >= MaxSlots; } }

    void Awake()
    {
        Instance = this;
        weapons.Add("Pistol");
    }

    // true - оружие добавлено. Если слоты заняты и swapIfFull - заменяет текущее (старое падает на пол)
    public bool AddWeapon(string weaponName, bool swapIfFull = false)
    {
        if (!GameData.Weapons.ContainsKey(weaponName)) return false;
        if (weapons.Contains(weaponName)) return false;

        if (weapons.Count < MaxSlots)
        {
            weapons.Add(weaponName);
            if (weapons.Count == 1)
                EquipWeapon(0);
            return true;
        }

        if (!swapIfFull) return false;

        string old = weapons[currentWeaponIndex];
        weapons[currentWeaponIndex] = weaponName;
        EquipWeapon(currentWeaponIndex);
        DropWeapon(old);
        return true;
    }

    void DropWeapon(string weaponName)
    {
        GameObject obj = new GameObject("DroppedWeapon");
        obj.transform.position = transform.position + Vector3.right * 1.3f;
        obj.AddComponent<SpriteRenderer>().sortingOrder = 8;
        WeaponPickup wp = obj.AddComponent<WeaponPickup>();
        wp.weaponName = weaponName;
        wp.enableTime = Time.time + 1.5f;
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count) return;

        currentWeaponIndex = index;
        PlayerController player = GetComponent<PlayerController>();
        if (player == null) return;

        // выбор другого оружия отменяет ручной клинок
        if (reserveActive && reserveManual)
        {
            reserveActive = false;
            reserveManual = false;
        }

        if (reserveActive)
        {
            // без энергии остаёмся с запасным клинком, если выбранное оружие требует энергию
            WeaponData d = GameData.Weapons[weapons[index]];
            if (d.energyCost > 0 && player.currentEnergy < ReserveExitEnergy(d.energyCost)) return;
            reserveActive = false;
        }
        player.EquipWeapon(weapons[index]);
    }

    // Клавиша 9: взять клинок / вернуть основное оружие
    public void ToggleReserve(PlayerController player)
    {
        if (weapons.Count == 0) return;

        if (reserveActive)
        {
            reserveActive = false;
            reserveManual = false;
            player.EquipWeapon(weapons[currentWeaponIndex]); // если энергии нет - UpdateReserve снова выдаст клинок сам
        }
        else
        {
            reserveActive = true;
            reserveManual = true;
            player.EquipWeapon(GameData.ReserveWeapon);
        }
    }

    static int ReserveExitEnergy(int cost) { return Mathf.Max(20, cost * 2); }

    // Вызывается каждый кадр из PlayerController: выдаёт/убирает запасной клинок
    public void UpdateReserve(PlayerController player)
    {
        if (weapons.Count == 0) return;
        if (reserveActive && reserveManual) return;
        WeaponData cur = GameData.Weapons[weapons[currentWeaponIndex]];

        if (!reserveActive)
        {
            if (player.overdriveUntil > Time.time) return;
            if (cur.energyCost > 0 && player.currentEnergy < cur.energyCost)
            {
                reserveActive = true;
                player.EquipWeapon(GameData.ReserveWeapon);
            }
        }
        else if (cur.energyCost == 0 || player.currentEnergy >= ReserveExitEnergy(cur.energyCost))
        {
            reserveActive = false;
            player.EquipWeapon(weapons[currentWeaponIndex]);
        }
    }

    public void NextWeapon()
    {
        if (weapons.Count > 1)
            EquipWeapon((currentWeaponIndex + 1) % weapons.Count);
    }

    public void PreviousWeapon()
    {
        if (weapons.Count > 1)
            EquipWeapon((currentWeaponIndex - 1 + weapons.Count) % weapons.Count);
    }
}
