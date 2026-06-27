using UnityEngine;

public class WeaponUpgrade : MonoBehaviour
{
    public static WeaponUpgrade Instance;

    [Header("Current Stats")]
    public int damage = 40;
    public float fireRate = 0.25f;
    public int projectiles = 4;

    [Header("Upgrade Levels")]
    public int damageLevel = 1;
    public int fireRateLevel = 1;
    public int projectileLevel = 1;

    void Awake()
    {
        Instance = this;
    }

    public void UpgradeDamage()
    {
        damageLevel++;
        damage = 40 + (damageLevel * 15);
    }

    public void UpgradeFireRate()
    {
        fireRateLevel++;
        fireRate = 0.25f / (1 + fireRateLevel * 0.2f);
    }

    public void UpgradeProjectiles()
    {
        projectileLevel++;
        projectiles = 4 + projectileLevel;
    }

    public void UpgradeAll()
    {
        UpgradeDamage();
        UpgradeFireRate();
        UpgradeProjectiles();
    }
}
