using UnityEngine;

public class LightingSystem : MonoBehaviour
{
    public static LightingSystem Instance;

    private GameObject playerLight;
    private float flickerTimer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreatePlayerLight();
        CreateAmbientOverlay();
    }

    void CreatePlayerLight()
    {
        playerLight = new GameObject("PlayerLight");
        playerLight.transform.position = Vector3.zero;
        SpriteRenderer sr = playerLight.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(64, new Color(1f, 0.95f, 0.8f, 0.15f));
        sr.sortingOrder = -1;
        playerLight.transform.localScale = Vector3.one * 8f;
    }

    void CreateAmbientOverlay()
    {
        GameObject overlay = new GameObject("AmbientDarkness");
        overlay.transform.position = new Vector3(0, 0, 5);
        SpriteRenderer sr = overlay.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateSquare(4, new Color(0, 0, 0, 0.35f));
        sr.sortingOrder = 100;
        overlay.transform.localScale = Vector3.one * 200f;
        overlay.transform.SetParent(Camera.main.transform);
        overlay.transform.localPosition = new Vector3(0, 0, 5);
    }

    void Update()
    {
        if (playerLight == null || PlayerController.Instance == null) return;

        playerLight.transform.position = PlayerController.Instance.transform.position;

        flickerTimer += Time.deltaTime * 8f;
        float flicker = 1f + Mathf.Sin(flickerTimer) * 0.05f + Mathf.Sin(flickerTimer * 2.3f) * 0.03f;
        playerLight.transform.localScale = Vector3.one * 8f * flicker;
    }
}
