using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Решётка в проёме: закрывается на время боя, открывается после победы
public class RoomDoor : MonoBehaviour
{
    private BoxCollider2D col;
    private bool closed;
    private bool locked;   // заперта, пока не зачищены все комнаты этажа
    private bool manual = true;   // закрыта, пока игрок не откроет её клавишей E рядом с дверью

    public bool IsManualClosed { get { return manual; } }
    public bool IsLocked { get { return locked; } }

    // Игрок открывает дверь: она плавно уезжает и остаётся открытой
    public void OpenManual()
    {
        if (!manual) return;
        manual = false;
        if (EffectsManager.Instance != null)
            EffectsManager.Instance.SpawnPickupEffect(transform.position, new Color(0.9f, 0.85f, 0.5f));
        if (ProceduralMusic.Instance != null)
            ProceduralMusic.Instance.PlaySFX("shopOpen");
    }
    private float k;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.enabled = false;
        transform.localScale = Vector3.zero;
    }

    public void SetClosed(bool value)
    {
        closed = value;
        if (closed) col.enabled = true;
    }

    // Запирает проход к боссу (дверь краснеет и не открывается)
    public void SetLocked(bool value)
    {
        locked = value;
        if (locked) col.enabled = true;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color cl = sr.color;
            sr.color = value ? new Color(1f, 0.35f, 0.35f, cl.a) : new Color(1f, 1f, 1f, cl.a);
        }
    }

    void Update()
    {
        float target = closed || locked || manual ? 1f : 0f;
        if (target > 0f) col.enabled = true;
        if (Mathf.Approximately(k, target)) return;
        k = Mathf.MoveTowards(k, target, Time.deltaTime * (manual ? 4f : 1.6f));
        transform.localScale = Vector3.one * k;
        if (!closed && !locked && !manual && k <= 0.01f) col.enabled = false;
    }
}

// Портал на следующий этаж (появляется после босса)
public class Portal : MonoBehaviour
{
    private bool used;
    private SpriteRenderer sr;
    private float t;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        t += Time.deltaTime;
        transform.Rotate(0, 0, 60f * Time.deltaTime);
        float s = 2.6f + Mathf.Sin(t * 3f) * 0.15f;
        transform.localScale = Vector3.one * Mathf.Min(s, t * 6f);

        if (used) return;
        PlayerController pc = PlayerController.Instance;
        if (pc == null) return;
        if (Vector2.Distance(pc.transform.position, transform.position) < 1.2f)
        {
            used = true;
            DungeonGenerator.Instance.StartNextFloor();
        }
    }
}

// Затемнение экрана, заставка этажа и надписи
public class FloorTransition : MonoBehaviour
{
    private static FloorTransition instance;

    private Image fade;
    private Text title, sub;
    private Coroutine bannerRoutine;

    public static FloorTransition Get()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("FloorTransition");
            instance = go.AddComponent<FloorTransition>();
            instance.Build();
        }
        return instance;
    }

    void Build()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        fade = MakeImage("Fade", Color.black);
        fade.rectTransform.anchorMin = Vector2.zero;
        fade.rectTransform.anchorMax = Vector2.one;
        fade.rectTransform.sizeDelta = Vector2.zero;
        fade.color = new Color(0, 0, 0, 0);
        fade.raycastTarget = false;

        title = MakeText("Title", 72, new Color(1f, 0.85f, 0.3f), 0.58f);
        sub = MakeText("Sub", 32, Color.white, 0.48f);
    }

    Image MakeImage(string n, Color c)
    {
        GameObject g = new GameObject(n);
        g.transform.SetParent(transform, false);
        Image i = g.AddComponent<Image>();
        i.color = c;
        return i;
    }

    Text MakeText(string n, int size, Color c, float anchorY)
    {
        GameObject g = new GameObject(n);
        g.transform.SetParent(transform, false);
        Text t = g.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(c.r, c.g, c.b, 0);
        t.raycastTarget = false;
        RectTransform r = t.rectTransform;
        r.anchorMin = r.anchorMax = new Vector2(0.5f, anchorY);
        r.sizeDelta = new Vector2(1400, 100);
        return t;
    }

    void SetTextAlpha(float a)
    {
        title.color = new Color(title.color.r, title.color.g, title.color.b, a);
        sub.color = new Color(sub.color.r, sub.color.g, sub.color.b, a);
    }

    // Просто надпись поверх игры
    public void Banner(string t, string s, float duration)
    {
        if (bannerRoutine != null) StopCoroutine(bannerRoutine);
        bannerRoutine = StartCoroutine(BannerRoutine(t, s, duration));
    }

    IEnumerator BannerRoutine(string t, string s, float duration)
    {
        title.text = t;
        sub.text = s;
        float e = 0f;
        while (e < duration)
        {
            e += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(Mathf.Min(e / 0.4f, (duration - e) / 0.6f));
            SetTextAlpha(a);
            yield return null;
        }
        SetTextAlpha(0f);
        bannerRoutine = null;
    }

    // Затемнение -> mid() -> заставка -> проявление -> done()
    public void Play(string t, string s, Action mid, Action done)
    {
        StartCoroutine(PlayRoutine(t, s, mid, done));
    }

    IEnumerator PlayRoutine(string t, string s, Action mid, Action done)
    {
        if (bannerRoutine != null) { StopCoroutine(bannerRoutine); bannerRoutine = null; SetTextAlpha(0f); }

        float e = 0f;
        while (e < 0.6f)
        {
            e += Time.unscaledDeltaTime;
            fade.color = new Color(0, 0, 0, Mathf.Clamp01(e / 0.6f));
            yield return null;
        }

        if (mid != null) mid();

        title.text = t;
        sub.text = s;
        e = 0f;
        while (e < 2.2f)
        {
            e += Time.unscaledDeltaTime;
            SetTextAlpha(Mathf.Clamp01(Mathf.Min(e / 0.4f, (2.2f - e) / 0.4f)));
            yield return null;
        }
        SetTextAlpha(0f);

        e = 0f;
        while (e < 0.6f)
        {
            e += Time.unscaledDeltaTime;
            fade.color = new Color(0, 0, 0, 1f - Mathf.Clamp01(e / 0.6f));
            yield return null;
        }
        fade.color = new Color(0, 0, 0, 0);

        if (done != null) done();
    }
}
