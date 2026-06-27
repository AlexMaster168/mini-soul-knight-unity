using UnityEngine;

public class ShopKeeper : MonoBehaviour
{
    public static ShopKeeper Instance;

    private bool playerInRange;
    private GameObject interactionHint;
    private TextMesh hintText;
    private float openCooldown;

    void Start()
    {
        Instance = this;
        CreateInteractionHint();
    }

    void CreateInteractionHint()
    {
        GameObject hint = new GameObject("ShopHint");
        hint.transform.SetParent(transform);
        hint.transform.localPosition = new Vector3(0, 1.5f, 0);

        hintText = hint.AddComponent<TextMesh>();
        hintText.text = "[E] Shop";
        hintText.characterSize = 0.12f;
        hintText.fontSize = 64;
        hintText.anchor = TextAnchor.MiddleCenter;
        hintText.alignment = TextAlignment.Center;
        hintText.color = new Color(1f, 0.85f, 0.1f);
        hintText.fontStyle = FontStyle.Bold;
        hint.GetComponent<MeshRenderer>().sortingOrder = 20;

        interactionHint = hint;
        interactionHint.SetActive(false);
    }

    void Update()
    {
        openCooldown -= Time.deltaTime;

        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.transform.position);
        playerInRange = dist < 2.5f;

        if (interactionHint != null)
            interactionHint.SetActive(playerInRange && (ShopUI.Instance == null || !ShopUI.Instance.IsOpen()));

        if (playerInRange && Input.GetKeyDown(KeyCode.E) && openCooldown <= 0)
        {
            if (ShopUI.Instance != null)
            {
                if (!ShopUI.Instance.IsOpen())
                {
                    ShopUI.Instance.ToggleShop();
                    openCooldown = 0.3f;
                }
            }
        }
    }
}
