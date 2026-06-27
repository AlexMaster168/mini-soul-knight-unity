using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;

    private GameObject gameOverCanvas;
    private GameObject gameOverPanel;

    void Awake()
    {
        Instance = this;
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null) return;

        gameOverCanvas = new GameObject("GameOverCanvas");
        Canvas canvas = gameOverCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        gameOverCanvas.AddComponent<CanvasScaler>();

        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(gameOverCanvas.transform);
        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.85f);
        panelImage.raycastTarget = false;
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        GameObject textObj = new GameObject("GameOverText");
        textObj.transform.SetParent(gameOverPanel.transform);
        Text gameOverText = textObj.AddComponent<Text>();
        gameOverText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        gameOverText.fontSize = 56;
        gameOverText.color = new Color(1f, 0.2f, 0.2f);
        gameOverText.text = "GAME OVER";
        gameOverText.alignment = TextAnchor.MiddleCenter;
        gameOverText.fontStyle = FontStyle.Bold;
        gameOverText.raycastTarget = false;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.65f);
        textRect.anchorMax = new Vector2(0.5f, 0.65f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(500, 80);

        GameObject scoreObj = new GameObject("FinalScore");
        scoreObj.transform.SetParent(gameOverPanel.transform);
        Text scoreText = scoreObj.AddComponent<Text>();
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = 28;
        scoreText.color = Color.white;
        scoreText.alignment = TextAnchor.MiddleCenter;
        scoreText.raycastTarget = false;
        int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0;
        scoreText.text = "Score: " + finalScore;
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
        scoreRect.anchorMax = new Vector2(0.5f, 0.5f);
        scoreRect.pivot = new Vector2(0.5f, 0.5f);
        scoreRect.sizeDelta = new Vector2(300, 40);

        GameObject floorObj = new GameObject("FloorReached");
        floorObj.transform.SetParent(gameOverPanel.transform);
        Text floorText = floorObj.AddComponent<Text>();
        floorText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        floorText.fontSize = 22;
        floorText.color = new Color(1f, 0.85f, 0.3f);
        floorText.alignment = TextAnchor.MiddleCenter;
        floorText.raycastTarget = false;
        int floor = DungeonGenerator.Instance != null ? DungeonGenerator.Instance.GetFloor() : 1;
        floorText.text = "Reached Floor " + floor;
        RectTransform floorRect = floorObj.GetComponent<RectTransform>();
        floorRect.anchorMin = new Vector2(0.5f, 0.42f);
        floorRect.anchorMax = new Vector2(0.5f, 0.42f);
        floorRect.pivot = new Vector2(0.5f, 0.5f);
        floorRect.sizeDelta = new Vector2(300, 35);

        GameObject btnObj = new GameObject("RestartButton");
        btnObj.transform.SetParent(gameOverPanel.transform);
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(RestartGame);
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.25f);
        btnRect.anchorMax = new Vector2(0.5f, 0.25f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(200, 50);

        GameObject btnTextObj = new GameObject("BtnText");
        btnTextObj.transform.SetParent(btnObj.transform);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 24;
        btnText.color = Color.white;
        btnText.text = "RESTART";
        btnText.alignment = TextAnchor.MiddleCenter;
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;
    }

    void Update()
    {
        if (gameOverPanel != null && Input.GetKeyDown(KeyCode.R))
            RestartGame();
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}
