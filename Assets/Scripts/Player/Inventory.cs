using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public const int MaxSlots = 5;

    public List<string> weapons = new List<string>();
    public int currentWeaponIndex = 0;

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
        if (index >= 0 && index < weapons.Count)
        {
            currentWeaponIndex = index;
            PlayerController player = GetComponent<PlayerController>();
            if (player != null)
                player.EquipWeapon(weapons[index]);
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
