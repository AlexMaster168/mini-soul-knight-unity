using UnityEngine;

public class GameSetup : MonoBehaviour
{
    public static GameSetup Instance;

    void Awake()
    {
        Instance = this;
    }

    GameObject CreateBullet(Vector2 position, Vector2 direction, BulletSpec spec)
    {
        GameObject obj = new GameObject(spec.enemy ? "EnemyBullet" : "PlayerBullet");
        obj.transform.position = position;
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArt.BulletSprite(spec.style, spec.color);
        sr.sortingOrder = 15;
        Bullet b = obj.AddComponent<Bullet>();
        b.Init(direction, spec);
        return obj;
    }

    // Пуля игрока с учётом профиля оружия
    public GameObject SpawnPlayerBullet(Vector2 position, Vector2 direction, int damage, WeaponData data, WeaponProfile profile)
    {
        BulletSpec spec = new BulletSpec
        {
            damage = damage,
            speed = data.bulletSpeed > 0f ? data.bulletSpeed : 12f,
            size = data.bulletSize,
            pierce = data.pierce,
            lifetime = profile.lifetime,
            aoeRadius = profile.aoeRadius,
            boomerang = profile.boomerang,
            wobble = profile.wobble,
            trail = profile.trail,
            style = profile.style,
            color = profile.color,
            enemy = false
        };

        if (profile.cls == WeaponClass.Melee)
        {
            spec.speed = 11f;
            spec.pierce = true;
            spec.size = data.bulletSize * 1.6f;
        }
        else
        {
            // визуальный размер спрайта пули (хитбокс считается от него же)
            switch (profile.style)
            {
                case BulletStyle.Slug: spec.size = Mathf.Max(0.5f, data.bulletSize * 1.2f); break;
                case BulletStyle.Streak: spec.size = Mathf.Max(0.7f, data.bulletSize * 1.8f); break;
                case BulletStyle.Laser: spec.size = Mathf.Max(1.2f, data.bulletSize * 5f); break;
                case BulletStyle.Bolt: spec.size = 1.1f; break;
                case BulletStyle.Shard: spec.size = Mathf.Max(0.8f, data.bulletSize * 2f); break;
                case BulletStyle.Rocket: spec.size = 1.1f; break;
                case BulletStyle.Round: spec.size = Mathf.Max(0.3f, data.bulletSize); break;
            }
        }

        return CreateBullet(position, direction, spec);
    }

    public GameObject SpawnRobotBullet(Vector2 position, Vector2 direction, int damage, Color color, float speed, float size, BulletStyle style, bool pierce)
    {
        return CreateBullet(position, direction, new BulletSpec
        {
            damage = damage, speed = speed, size = size, lifetime = 1.6f, pierce = pierce,
            style = style, color = color, enemy = false, trail = style == BulletStyle.Orb
        });
    }

    public GameObject SpawnBullet(Vector2 position, Vector2 direction, int damage)
    {
        return CreateBullet(position, direction, new BulletSpec
        {
            damage = damage, speed = 30f, size = 0.5f, lifetime = 2f,
            style = BulletStyle.Slug, color = new Color(1f, 0.9f, 0.3f)
        });
    }

    public GameObject SpawnEnemyBullet(Vector2 position, Vector2 direction, int damage)
    {
        return SpawnEnemyBullet(position, direction, damage, new Color(1f, 0.3f, 0.3f), 15f, 0.35f, BulletStyle.Round);
    }

    public GameObject SpawnEnemyBullet(Vector2 position, Vector2 direction, int damage, Color color, float speed = 15f, float size = 0.35f, BulletStyle style = BulletStyle.Round, float lifetime = 4f)
    {
        return CreateBullet(position, direction, new BulletSpec
        {
            damage = damage, speed = speed, size = size, lifetime = lifetime,
            style = style, color = color, enemy = true
        });
    }
}
