using UnityEngine;

public class GameSetup : MonoBehaviour
{
    public static GameSetup Instance;

    public GameObject bulletPrefab;
    public GameObject enemyBulletPrefab;

    void Awake()
    {
        Instance = this;
        CreatePrefabs();
    }

    void CreatePrefabs()
    {
        bulletPrefab = CreateBulletPrefab("PlayerBullet", new Color(1f, 1f, 0.3f), false);
        enemyBulletPrefab = CreateBulletPrefab("EnemyBullet", new Color(1f, 0.3f, 0.3f), true);
    }

    GameObject CreateBulletPrefab(string name, Color color, bool enemy)
    {
        GameObject obj = new GameObject(name);
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(16, color);
        sr.sortingOrder = 15;

        obj.AddComponent<Bullet>();
        obj.transform.localScale = Vector3.one * 0.4f;
        obj.SetActive(false);
        return obj;
    }

    public GameObject SpawnBullet(Vector2 position, Vector2 direction, int damage)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        bullet.SetActive(true);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null) b.Initialize(direction, damage, 30f, 0.4f, false, false);
        return bullet;
    }

    public GameObject SpawnEnemyBullet(Vector2 position, Vector2 direction, int damage)
    {
        GameObject bullet = Instantiate(enemyBulletPrefab, position, Quaternion.identity);
        bullet.SetActive(true);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null) b.Initialize(direction, damage, 15f, 0.35f, false, true);
        return bullet;
    }
}
