using System.Collections.Generic;

public class WeaponData
{
    public string name;
    public int damage;
    public float fireRate;
    public int projectiles;
    public int energyCost;
    public float bulletSpeed;
    public float bulletSize;
    public float spread;
    public bool pierce;
    public bool aoe;

    public WeaponData(string name, int damage, float fireRate, int projectiles, int energyCost,
        float bulletSpeed = 20f, float bulletSize = 0.4f, float spread = 0.15f,
        bool pierce = false, bool aoe = false)
    {
        this.name = name;
        this.damage = damage;
        this.fireRate = fireRate;
        this.projectiles = projectiles;
        this.energyCost = energyCost;
        this.bulletSpeed = bulletSpeed;
        this.bulletSize = bulletSize;
        this.spread = spread;
        this.pierce = pierce;
        this.aoe = aoe;
    }
}

public class EnemyData
{
    public string name;
    public int health;
    public int damage;
    public float speed;
    public float attackRange;
    public float attackCooldown;
    public bool canShoot;
    public float shootRange;
    public float shootCooldown;
    public int projectiles;
    public bool explodeOnDeath;
    public float explosionRadius;
    public bool summonMinions;
    public float scale;

    public EnemyData(string name, int health, int damage, float speed, float attackRange,
        float attackCooldown, bool canShoot = false, float shootRange = 0f,
        float shootCooldown = 0f, int projectiles = 1, bool explodeOnDeath = false,
        float explosionRadius = 0f, bool summonMinions = false, float scale = 0.9f)
    {
        this.name = name;
        this.health = health;
        this.damage = damage;
        this.speed = speed;
        this.attackRange = attackRange;
        this.attackCooldown = attackCooldown;
        this.canShoot = canShoot;
        this.shootRange = shootRange;
        this.shootCooldown = shootCooldown;
        this.projectiles = projectiles;
        this.explodeOnDeath = explodeOnDeath;
        this.explosionRadius = explosionRadius;
        this.summonMinions = summonMinions;
        this.scale = scale;
    }
}

public static class GameData
{
    public static Dictionary<string, WeaponData> Weapons = new Dictionary<string, WeaponData>
    {
        // Pistols
        { "Pistol", new WeaponData("Pistol", 25, 0.3f, 1, 5) },
        { "Dual", new WeaponData("Dual", 15, 0.2f, 2, 8) },
        { "Revolver", new WeaponData("Revolver", 45, 0.5f, 1, 12) },
        { "DesertEagle", new WeaponData("DesertEagle", 60, 0.6f, 1, 15) },

        // Shotguns
        { "Shotgun", new WeaponData("Shotgun", 15, 0.8f, 5, 15, 18f, 0.35f, 0.25f) },
        { "SuperShotgun", new WeaponData("SuperShotgun", 18, 0.9f, 8, 22, 18f, 0.35f, 0.3f) },
        { "TacticalSG", new WeaponData("TacticalSG", 20, 0.7f, 6, 18, 20f, 0.3f, 0.2f) },

        // SMGs
        { "Uzi", new WeaponData("Uzi", 8, 0.08f, 1, 3) },
        { "Mac10", new WeaponData("Mac10", 6, 0.06f, 1, 2) },
        { "Thompson", new WeaponData("Thompson", 10, 0.1f, 1, 4) },

        // Rifles
        { "AK", new WeaponData("AK", 12, 0.12f, 1, 4) },
        { "LMG", new WeaponData("LMG", 10, 0.06f, 1, 3) },
        { "M4", new WeaponData("M4", 14, 0.1f, 1, 4) },
        { "Famas", new WeaponData("Famas", 11, 0.09f, 3, 6) },

        // Snipers
        { "Sniper", new WeaponData("Sniper", 100, 1.5f, 1, 25, 40f, 0.3f, 0.02f) },
        { "AWP", new WeaponData("AWP", 150, 2.0f, 1, 35, 50f, 0.3f, 0.01f) },
        { "Crossbow", new WeaponData("Crossbow", 80, 1.0f, 1, 0, 30f, 0.4f, 0.03f) },

        // Energy
        { "LaserRifle", new WeaponData("LaserRifle", 60, 0.6f, 1, 18, 35f, 0.2f, 0f, true) },
        { "Laser", new WeaponData("Laser", 35, 0.15f, 1, 8, 30f, 0.15f, 0f, true) },
        { "Plasma", new WeaponData("Plasma", 50, 0.4f, 3, 15, 15f, 0.5f, 0.2f) },
        { "Shock", new WeaponData("Shock", 30, 0.3f, 4, 12, 22f, 0.3f, 0.35f) },
        { "Thunder", new WeaponData("Thunder", 40, 0.5f, 5, 20, 20f, 0.3f, 0.4f) },
        { "IceGun", new WeaponData("IceGun", 20, 0.3f, 2, 10, 18f, 0.3f, 0.15f) },
        { "PoisonGun", new WeaponData("PoisonGun", 8, 0.2f, 3, 6, 15f, 0.35f, 0.3f) },

        // Heavy
        { "Rocket", new WeaponData("Rocket", 80, 1.2f, 1, 35, 12f, 0.6f, 0.05f, false, true) },
        { "Flamethrower", new WeaponData("Flamethrower", 5, 0.04f, 3, 2, 10f, 0.25f, 0.4f) },
        { "HolyGrenade", new WeaponData("HolyGrenade", 150, 2.0f, 1, 50, 8f, 0.7f, 0f, false, true) },
        { "GrenadeLauncher", new WeaponData("GrenadeLauncher", 60, 1.0f, 1, 25, 14f, 0.5f, 0.1f, false, true) },
        { "Minigun", new WeaponData("Minigun", 7, 0.04f, 2, 3, 25f, 0.2f, 0.2f) },

        // Exotic
        { "Boomerang", new WeaponData("Boomerang", 35, 0.6f, 1, 0, 15f, 0.5f, 0f, true) },
        { "Star Wand", new WeaponData("Star Wand", 30, 0.4f, 3, 10, 20f, 0.3f, 0.3f) },
        { "FairyGun", new WeaponData("FairyGun", 45, 0.35f, 2, 12, 18f, 0.25f, 0.15f) },

        // Melee
        { "Sword", new WeaponData("Sword", 40, 0.4f, 1, 0, 0f, 1.0f, 0f, true) },
        { "Katana", new WeaponData("Katana", 55, 0.5f, 1, 0, 0f, 1.2f, 0f, true) },
        { "Mace", new WeaponData("Mace", 70, 0.8f, 1, 0, 0f, 1.5f, 0f, true) },
        { "Axe", new WeaponData("Axe", 60, 0.6f, 1, 0, 0f, 1.3f, 0f, true) },
        { "Scythe", new WeaponData("Scythe", 85, 0.7f, 1, 0, 0f, 1.4f, 0f, true) },
    };

    public static Dictionary<string, EnemyData> Enemies = new Dictionary<string, EnemyData>
    {
        // Melee basic
        { "Slime", new EnemyData("Slime", 50, 10, 3.5f, 1.2f, 1f) },
        { "Goblin", new EnemyData("Goblin", 35, 8, 4.5f, 1.2f, 1f, scale: 0.8f) },
        { "Skeleton", new EnemyData("Skeleton", 40, 12, 3f, 1.5f, 1.2f, scale: 1.0f) },
        { "Zombie", new EnemyData("Zombie", 60, 8, 2f, 1.2f, 1.5f, scale: 1.0f) },
        { "Wolf", new EnemyData("Wolf", 30, 15, 5f, 1.3f, 0.8f, scale: 0.85f) },
        { "Bat", new EnemyData("Bat", 20, 6, 5.5f, 1f, 0.6f, scale: 0.6f) },

        // Melee heavy
        { "DarkKnight", new EnemyData("DarkKnight", 120, 20, 2.5f, 1.5f, 1.5f, scale: 1.2f) },
        { "Orc", new EnemyData("Orc", 150, 18, 2f, 1.5f, 1.8f, scale: 1.3f) },
        { "Golem", new EnemyData("Golem", 250, 12, 1.2f, 1.8f, 2.5f, scale: 1.4f) },

        // Ranged
        { "Shooter", new EnemyData("Shooter", 30, 12, 2.5f, 1f, 1.5f, true, 10f, 1.5f, 1) },
        { "Mage", new EnemyData("Mage", 25, 18, 3f, 1f, 1.5f, true, 12f, 2f, 3) },
        { "Archer", new EnemyData("Archer", 28, 14, 2.5f, 1f, 1.2f, true, 12f, 1f, 1) },
        { "IceMage", new EnemyData("IceMage", 30, 10, 2.5f, 1f, 2f, true, 10f, 1.8f, 2) },
        { "FireMage", new EnemyData("FireMage", 25, 22, 2.8f, 1f, 1.5f, true, 11f, 1.5f, 1) },
        { "NecroMage", new EnemyData("NecroMage", 35, 15, 2f, 1f, 2f, true, 10f, 2.5f, 2) },

        // Special movement
        { "Ghost", new EnemyData("Ghost", 35, 10, 3.5f, 1f, 1f, true, 8f, 2f, 1, scale: 0.9f) },
        { "Speedster", new EnemyData("Speedster", 15, 5, 7f, 1f, 0.6f, scale: 0.7f) },
        { "Flyer", new EnemyData("Flyer", 30, 10, 3.5f, 1f, 1.5f, true, 10f, 1.2f, 2, scale: 0.8f) },
        { "Spider", new EnemyData("Spider", 25, 8, 4f, 1.2f, 0.8f, scale: 0.7f) },
        { "Snake", new EnemyData("Snake", 20, 12, 5f, 1.3f, 0.7f, scale: 0.65f) },

        // Exploding/Summoning
        { "Bomber", new EnemyData("Bomber", 20, 30, 4f, 1.5f, 0.8f, false, 0f, 0f, 0, true, 3f, false, 0.8f) },
        { "Spawner", new EnemyData("Spawner", 80, 5, 1f, 1f, 3f, false, 0f, 0f, 0, false, 0f, true, 1.1f) },

        // Tank
        { "Tank", new EnemyData("Tank", 200, 15, 1.5f, 1.5f, 2f, scale: 1.3f) },
        { "Guardian", new EnemyData("Guardian", 180, 20, 1.8f, 1.5f, 1.8f, true, 6f, 2f, 1, scale: 1.2f) },
    };

    public static Dictionary<string, EnemyData> Bosses = new Dictionary<string, EnemyData>
    {
        { "CrownedBoar", new EnemyData("CrownedBoar", 800, 25, 2f, 2f, 1.5f, true, 12f, 1f, 5, false, 0f, false, 2.0f) },
        { "Dragon", new EnemyData("Dragon", 1000, 30, 1.5f, 2f, 2f, true, 15f, 0.8f, 8, false, 0f, false, 2.5f) },
        { "Necromancer", new EnemyData("Necromancer", 600, 20, 2f, 2f, 2f, true, 10f, 1.5f, 3, false, 0f, true, 2.0f) },
    };
}
