using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    public List<string> weapons = new List<string>();
    public int currentWeaponIndex = 0;

    void Awake()
    {
        Instance = this;
    }

    public void AddWeapon(string weaponName)
    {
        if (!GameData.Weapons.ContainsKey(weaponName)) return;
        if (weapons.Contains(weaponName)) return;

        weapons.Add(weaponName);
        if (weapons.Count == 1)
            EquipWeapon(0);
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
        {
            currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Count;
            EquipWeapon(currentWeaponIndex);
        }
    }

    public void PreviousWeapon()
    {
        if (weapons.Count > 1)
        {
            currentWeaponIndex = (currentWeaponIndex - 1 + weapons.Count) % weapons.Count;
            EquipWeapon(currentWeaponIndex);
        }
    }
}
