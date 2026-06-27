using UnityEngine;

public class TorchFlicker : MonoBehaviour
{
    private SpriteRenderer sr;
    private float timer;
    private Vector3 originalScale;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        timer = Random.Range(0f, 10f);
    }

    void Update()
    {
        timer += Time.deltaTime * 6f;

        if (sr != null)
        {
            float flicker = 0.7f + Mathf.Sin(timer) * 0.15f + Mathf.Sin(timer * 2.7f) * 0.1f;
            sr.color = new Color(1f, 0.5f * flicker, 0.05f, 0.9f * flicker);
        }

        float scaleX = originalScale.x * (0.9f + Mathf.Sin(timer * 1.5f) * 0.1f);
        float scaleY = originalScale.y * (0.85f + Mathf.Sin(timer * 2f) * 0.15f);
        transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }
}
