using UnityEngine;

public class PostProcessEffect : MonoBehaviour
{
    public static PostProcessEffect Instance;

    private Material vignetteMat;
    private Material blurMat;
    private float hitFlashTimer;
    private float screenShakeTimer;
    private float screenShakeIntensity;
    private Vector3 originalCameraPos;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        vignetteMat = new Material(Shader.Find("Hidden/Internal-GUIRoundedRect"));
        Camera cam = Camera.main;
        if (cam != null)
            originalCameraPos = cam.transform.position;
    }

    void Update()
    {
        if (hitFlashTimer > 0)
            hitFlashTimer -= Time.deltaTime;

        if (screenShakeTimer > 0)
        {
            screenShakeTimer -= Time.deltaTime;
            Camera cam = Camera.main;
            if (cam != null)
            {
                float x = Random.Range(-screenShakeIntensity, screenShakeIntensity);
                float y = Random.Range(-screenShakeIntensity, screenShakeIntensity);
                cam.transform.position = originalCameraPos + new Vector3(x, y, 0);
            }
        }
        else
        {
            Camera cam = Camera.main;
            if (cam != null)
                originalCameraPos = cam.transform.position;
        }
    }

    public void TriggerHitFlash()
    {
        hitFlashTimer = 0.1f;
    }

    public void TriggerScreenShake(float intensity, float duration)
    {
        screenShakeIntensity = intensity;
        screenShakeTimer = duration;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (hitFlashTimer > 0)
        {
            RenderTexture temp = RenderTexture.GetTemporary(src.width, src.height);
            Graphics.Blit(src, dest);

            float alpha = hitFlashTimer / 0.1f;
            Color flashColor = new Color(1f, 0.2f, 0.2f, alpha * 0.4f);

            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
            GL.Color(flashColor);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 1, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(1, 0, 0);
            GL.End();
            GL.PopMatrix();

            return;
        }

        Graphics.Blit(src, dest);
    }
}
