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

    void EnsureEventSystem()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null) return;
        EnsureEventSystem();

        gameOverCanvas = new GameObject("GameOverCanvas");
        Canvas canvas = gameOverCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = gameOverCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        gameOverCanvas.AddComponent<GraphicRaycaster>();

        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(gameOverCanvas.transform, false);
        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.88f);
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        GameObject textObj = new GameObject("GameOverText");
        textObj.transform.SetParent(gameOverPanel.transform, false);
        Text gameOverText = textObj.AddComponent<Text>();
        gameOverText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        gameOverText.fontSize = 64;
        gameOverText.color = new Color(1f, 0.15f, 0.15f);
        gameOverText.text = "YOU DIED";
        gameOverText.alignment = TextAnchor.MiddleCenter;
        gameOverText.fontStyle = FontStyle.Bold;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.7f);
        textRect.anchorMax = new Vector2(0.5f, 0.7f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(600, 80);

        GameObject scoreObj = new GameObject("FinalScore");
        scoreObj.transform.SetParent(gameOverPanel.transform, false);
        Text scoreText = scoreObj.AddComponent<Text>();
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = 28;
        scoreText.color = Color.white;
        scoreText.alignment = TextAnchor.MiddleCenter;
        int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0;
        int finalGold = ScoreManager.Instance != null ? ScoreManager.Instance.GetGold() : 0;
        scoreText.text = "Score: " + finalScore + "   |   Gold: " + finalGold;
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 0.55f);
        scoreRect.anchorMax = new Vector2(0.5f, 0.55f);
        scoreRect.pivot = new Vector2(0.5f, 0.5f);
        scoreRect.sizeDelta = new Vector2(500, 40);

        GameObject floorObj = new GameObject("FloorReached");
        floorObj.transform.SetParent(gameOverPanel.transform, false);
        Text floorText = floorObj.AddComponent<Text>();
        floorText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        floorText.fontSize = 22;
        floorText.color = new Color(1f, 0.85f, 0.3f);
        floorText.alignment = TextAnchor.MiddleCenter;
        int floor = DungeonGenerator.Instance != null ? DungeonGenerator.Instance.GetFloor() : 1;
        floorText.text = "Reached Floor " + floor;
        RectTransform floorRect = floorObj.GetComponent<RectTransform>();
        floorRect.anchorMin = new Vector2(0.5f, 0.45f);
        floorRect.anchorMax = new Vector2(0.5f, 0.45f);
        floorRect.pivot = new Vector2(0.5f, 0.5f);
        floorRect.sizeDelta = new Vector2(300, 35);

        CreateButton("RESTART [R]", new Vector2(0.5f, 0.28f), new Color(0.8f, 0.2f, 0.2f, 0.95f), RestartGame);
    }

    public void ShowWin()
    {
        if (gameOverPanel != null) return;
        EnsureEventSystem();

        gameOverCanvas = new GameObject("WinCanvas");
        Canvas canvas = gameOverCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = gameOverCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        gameOverCanvas.AddComponent<GraphicRaycaster>();

        gameOverPanel = new GameObject("WinPanel");
        gameOverPanel.transform.SetParent(gameOverCanvas.transform, false);
        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0.05f, 0.1f, 0.9f);
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        GameObject textObj = new GameObject("WinText");
        textObj.transform.SetParent(gameOverPanel.transform, false);
        Text winText = textObj.AddComponent<Text>();
        winText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        winText.fontSize = 72;
        winText.color = new Color(1f, 0.85f, 0.1f);
        winText.text = "YOU WIN!";
        winText.alignment = TextAnchor.MiddleCenter;
        winText.fontStyle = FontStyle.Bold;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.72f);
        textRect.anchorMax = new Vector2(0.5f, 0.72f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(600, 90);

        GameObject subObj = new GameObject("SubText");
        subObj.transform.SetParent(gameOverPanel.transform, false);
        Text subText = subObj.AddComponent<Text>();
        subText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subText.fontSize = 24;
        subText.color = new Color(0.6f, 0.9f, 0.6f);
        subText.alignment = TextAnchor.MiddleCenter;
        subText.text = "Boss defeated!";
        RectTransform subRect = subObj.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.5f, 0.62f);
        subRect.anchorMax = new Vector2(0.5f, 0.62f);
        subRect.pivot = new Vector2(0.5f, 0.5f);
        subRect.sizeDelta = new Vector2(400, 35);

        GameObject scoreObj = new GameObject("FinalScore");
        scoreObj.transform.SetParent(gameOverPanel.transform, false);
        Text scoreText = scoreObj.AddComponent<Text>();
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = 28;
        scoreText.color = Color.white;
        scoreText.alignment = TextAnchor.MiddleCenter;
        int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0;
        int finalGold = ScoreManager.Instance != null ? ScoreManager.Instance.GetGold() : 0;
        scoreText.text = "Score: " + finalScore + "   |   Gold: " + finalGold;
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
        scoreRect.anchorMax = new Vector2(0.5f, 0.5f);
        scoreRect.pivot = new Vector2(0.5f, 0.5f);
        scoreRect.sizeDelta = new Vector2(500, 40);

        GameObject floorObj = new GameObject("FloorReached");
        floorObj.transform.SetParent(gameOverPanel.transform, false);
        Text floorText = floorObj.AddComponent<Text>();
        floorText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        floorText.fontSize = 22;
        floorText.color = new Color(1f, 0.85f, 0.3f);
        floorText.alignment = TextAnchor.MiddleCenter;
        int floor = DungeonGenerator.Instance != null ? DungeonGenerator.Instance.GetFloor() : 1;
        floorText.text = "Floor " + floor + " Complete!";
        RectTransform floorRect = floorObj.GetComponent<RectTransform>();
        floorRect.anchorMin = new Vector2(0.5f, 0.4f);
        floorRect.anchorMax = new Vector2(0.5f, 0.4f);
        floorRect.pivot = new Vector2(0.5f, 0.5f);
        floorRect.sizeDelta = new Vector2(300, 35);

        CreateButton("PLAY AGAIN [R]", new Vector2(0.5f, 0.25f), new Color(0.2f, 0.7f, 0.2f, 0.95f), RestartGame);
    }

    void CreateButton(string label, Vector2 anchor, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject("Button");
        btnObj.transform.SetParent(gameOverPanel.transform, false);
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = color;
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        btn.onClick.AddListener(onClick);
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = anchor;
        btnRect.anchorMax = anchor;
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(280, 55);

        GameObject btnTextObj = new GameObject("BtnText");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 24;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.fontStyle = FontStyle.Bold;
        btnText.text = label;
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
