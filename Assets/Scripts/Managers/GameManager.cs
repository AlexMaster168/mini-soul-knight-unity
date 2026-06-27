using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        Time.timeScale = 1f;
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}
