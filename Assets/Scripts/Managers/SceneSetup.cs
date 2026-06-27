using UnityEngine;

public class SceneSetup : MonoBehaviour
{
    void Awake()
    {
        CreateGameSetup();
        CreatePlayer();
        CreateManagers();
        CreateDungeon();
        CreateUI();
    }

    void CreateGameSetup()
    {
        GameObject setupObj = new GameObject("GameSetup");
        setupObj.AddComponent<GameSetup>();
    }

    void CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = Vector3.zero;

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateKnight(32, new Color(0.2f, 0.4f, 0.9f), new Color(0.15f, 0.3f, 0.7f));
        sr.sortingOrder = 10;

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.7f, 0.9f);
        col.offset = new Vector2(0, 0.05f);

        player.AddComponent<PlayerController>();
        player.AddComponent<Inventory>();
        player.transform.localScale = Vector3.one * 1.2f;

        GameObject weaponHolder = new GameObject("WeaponHolder");
        weaponHolder.transform.SetParent(player.transform);
        weaponHolder.transform.localPosition = new Vector3(0.4f, 0.1f, 0);

        Camera.main.backgroundColor = new Color(0.15f, 0.13f, 0.1f);
        Camera.main.orthographicSize = 14;
        Camera.main.transform.position = new Vector3(0, 0, -10);
        CameraFollow cf = Camera.main.gameObject.AddComponent<CameraFollow>();
        cf.target = player.transform;
    }

    void CreateManagers()
    {
        CreateManager("GameManager", typeof(GameManager));
        CreateManager("EffectsManager", typeof(EffectsManager));
        CreateManager("AudioManager", typeof(AudioManager));
        CreateManager("ScoreManager", typeof(ScoreManager));
        CreateManager("GameOverUI", typeof(GameOverUI));
        CreateManager("ProceduralMusic", typeof(ProceduralMusic));
    }

    void CreateManager(string name, System.Type component)
    {
        GameObject obj = new GameObject(name);
        obj.AddComponent(component);
    }

    void CreateDungeon()
    {
        GameObject dungeonObj = new GameObject("DungeonGenerator");
        dungeonObj.AddComponent<DungeonGenerator>();
    }

    void CreateUI()
    {
        GameObject canvas = new GameObject("GameUICanvas");
        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        canvas.AddComponent<UIManager>();
    }
}
