using UnityEngine;

public class PrefabCreator : MonoBehaviour
{
    void Awake()
    {
        // Create bullet prefab
        GameObject bullet = new GameObject("Bullet");
        bullet.AddComponent<Bullet>();
        bullet.AddComponent<CircleCollider2D>();
        bullet.GetComponent<CircleCollider2D>().isTrigger = true;
        bullet.AddComponent<SpriteRenderer>();
        bullet.GetComponent<SpriteRenderer>().color = Color.yellow;
        bullet.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        // Save as prefab (in a real project, you'd save to Assets/Prefabs)
        // For runtime, we'll just keep it in memory
        DontDestroyOnLoad(bullet);
        bullet.SetActive(false);

        // Store reference
        Resources.Load<GameObject>("Prefabs/Bullet");
    }
}