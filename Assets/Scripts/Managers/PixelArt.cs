using UnityEngine;
using System.Collections.Generic;

// Небольшой набор примитивов для процедурной пиксель-графики (y направлена вверх).
public class PixelCanvas
{
    public readonly int w, h;
    public readonly Color[] px;

    public PixelCanvas(int w, int h)
    {
        this.w = w;
        this.h = h;
        px = new Color[w * h];
    }

    public void Px(int x, int y, Color c)
    {
        if (x >= 0 && x < w && y >= 0 && y < h) px[y * w + x] = c;
    }

    public void Rect(int x0, int y0, int x1, int y1, Color c)
    {
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                Px(x, y, c);
    }

    public void Ell(float cx, float cy, float rx, float ry, Color c)
    {
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = (x + 0.5f - cx) / rx;
                float dy = (y + 0.5f - cy) / ry;
                if (dx * dx + dy * dy <= 1f) px[y * w + x] = c;
            }
    }

    public void Ring(float cx, float cy, float r0, float r1, Color c)
    {
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = x + 0.5f - cx;
                float dy = y + 0.5f - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d >= r0 && d <= r1) px[y * w + x] = c;
            }
    }

    public void Tri(float x0, float y0, float x1, float y1, float x2, float y2, Color c)
    {
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float fx = x + 0.5f, fy = y + 0.5f;
                float d1 = (fx - x1) * (y0 - y1) - (x0 - x1) * (fy - y1);
                float d2 = (fx - x2) * (y1 - y2) - (x1 - x2) * (fy - y2);
                float d3 = (fx - x0) * (y2 - y0) - (x2 - x0) * (fy - y0);
                bool neg = d1 < 0 || d2 < 0 || d3 < 0;
                bool pos = d1 > 0 || d2 > 0 || d3 > 0;
                if (!(neg && pos)) px[y * w + x] = c;
            }
    }

    public void Line(float x0, float y0, float x1, float y1, Color c, float thick = 1f)
    {
        int steps = Mathf.CeilToInt(Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0)) * 2f) + 1;
        float r = thick * 0.5f;
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            float cx = Mathf.Lerp(x0, x1, t) + 0.5f;
            float cy = Mathf.Lerp(y0, y1, t) + 0.5f;
            for (int y = Mathf.FloorToInt(cy - r - 1); y <= Mathf.CeilToInt(cy + r + 1); y++)
                for (int x = Mathf.FloorToInt(cx - r - 1); x <= Mathf.CeilToInt(cx + r + 1); x++)
                {
                    float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                    if (dx * dx + dy * dy <= r * r + 0.1f) Px(x, y, c);
                }
        }
    }

    // Левая половина перезаписывает правую -> симметричные существа рисуем только слева
    public void MirrorLeft()
    {
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w / 2; x++)
                px[y * w + (w - 1 - x)] = px[y * w + x];
    }

    public void Outline(Color c)
    {
        Color[] src = (Color[])px.Clone();
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (src[y * w + x].a > 0.01f) continue;
                bool near = (x > 0 && src[y * w + x - 1].a > 0.01f) || (x < w - 1 && src[y * w + x + 1].a > 0.01f)
                         || (y > 0 && src[(y - 1) * w + x].a > 0.01f) || (y < h - 1 && src[(y + 1) * w + x].a > 0.01f);
                if (near) px[y * w + x] = c;
            }
    }

    public Sprite ToSprite(float pivotX = 0.5f, float pivotY = 0.5f, float ppu = -1)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(pivotX, pivotY), ppu > 0 ? ppu : w);
    }
}

public enum BulletStyle { Round, Slug, Streak, Laser, Orb, Rocket, Flame, Slash, Boomerang, Star, Bolt, Shard, Grenade, Drop }

public static class PixelArt
{
    static readonly Color K = new Color(0.08f, 0.06f, 0.1f);
    static readonly Color W = Color.white;
    static readonly Color R = new Color(0.95f, 0.2f, 0.2f);
    static readonly Color Y = new Color(1f, 0.85f, 0.25f);
    static readonly Color O = new Color(1f, 0.55f, 0.12f);
    static readonly Color S = new Color(0.72f, 0.75f, 0.82f);
    static readonly Color D = new Color(0.42f, 0.44f, 0.52f);
    static readonly Color DD = new Color(0.25f, 0.26f, 0.32f);
    static readonly Color Brown = new Color(0.5f, 0.32f, 0.18f);
    static readonly Color DBrown = new Color(0.3f, 0.18f, 0.1f);
    static readonly Color Skin = new Color(0.9f, 0.72f, 0.55f);
    static readonly Color Bone = new Color(0.92f, 0.9f, 0.78f);
    static readonly Color Cyan = new Color(0.45f, 0.9f, 1f);
    static readonly Color Green = new Color(0.35f, 0.9f, 0.4f);

    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    static Color Dk(Color c, float f = 0.65f) { return new Color(c.r * f, c.g * f, c.b * f, 1f); }
    static Color Lt(Color c) { return Color.Lerp(c, Color.white, 0.35f); }
    static string Key(string n, Color c) { return n + ColorUtility.ToHtmlStringRGBA(c); }

    static Sprite Cached(string key, System.Func<Sprite> make)
    {
        Sprite s;
        if (!cache.TryGetValue(key, out s)) { s = make(); cache[key] = s; }
        return s;
    }

    // ================= ВРАГИ (24x24) =================

    static PixelCanvas E() { return new PixelCanvas(24, 24); }

    public static Sprite Slime(Color a, bool crown = false)
    {
        return Cached(Key(crown ? "slimeC" : "slime", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Ell(12, 8, 10.5f, 7.5f, a);
            c.Rect(2, 1, 21, 3, b);
            c.Ell(12, 1.5f, 9, 2, a);
            c.Ell(7.5f, 12, 2.2f, 1.4f, l);
            c.Px(6, 10, l);
            c.Rect(7, 8, 9, 10, W); c.Rect(14, 8, 16, 10, W);
            c.Rect(8, 8, 8, 9, K); c.Rect(15, 8, 15, 9, K);
            c.Rect(10, 5, 13, 5, K); c.Px(9, 6, K); c.Px(14, 6, K);
            if (crown)
            {
                c.Rect(8, 15, 15, 16, Y);
                c.Tri(8, 17, 10, 17, 9, 20, Y); c.Tri(11, 17, 13, 17, 12, 21, Y); c.Tri(14, 17, 16, 17, 15, 20, Y);
                c.Px(12, 16, R);
            }
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Goblin(Color a)
    {
        return Cached(Key("goblin", a), () =>
        {
            var c = E(); Color b = Dk(a);
            c.Ell(4, 15, 3.2f, 2, a);
            c.Ell(12, 14, 7, 5.5f, a);
            c.Rect(8, 14, 9, 15, Y); c.Px(9, 14, K);
            c.Rect(11, 11, 11, 13, b);
            c.Rect(8, 10, 11, 10, K); c.Px(9, 9, W);
            c.Rect(6, 3, 11, 8, Brown);
            c.Rect(6, 5, 11, 5, DBrown);
            c.Rect(3, 4, 5, 8, a);
            c.Rect(7, 0, 9, 2, b);
            c.MirrorLeft();
            c.Line(20, 5, 20, 12, S, 1.5f); c.Rect(19, 4, 21, 4, Brown);
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Wolf(Color a)
    {
        return Cached(Key("wolf", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Tri(3, 15, 7, 15, 4, 22, a);
            c.Ell(12, 13, 8, 7, a);
            c.Ell(4.5f, 10, 3, 3, l);
            c.Tri(5, 16, 8, 16, 5.5f, 20, b);
            c.Rect(7, 14, 9, 15, Y); c.Px(9, 14, K);
            c.Ell(12, 8.5f, 4, 3, l);
            c.Rect(11, 9, 11, 10, K);
            c.Px(8, 5, W); c.Px(9, 6, W);
            c.Rect(6, 0, 10, 4, a); c.Rect(6, 0, 8, 1, b);
            c.MirrorLeft();
            c.Rect(11, 8, 12, 9, K);
            c.Line(19, 2, 23, 9, a, 2);
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Spider(Color a)
    {
        return Cached(Key("spider", a), () =>
        {
            var c = E(); Color b = Dk(a, 0.75f);
            c.Line(8, 10, 2, 17, b, 1.5f); c.Line(2, 17, 1, 13, b, 1.2f);
            c.Line(8, 9, 1, 10, b, 1.5f); c.Line(1, 10, 1, 5, b, 1.2f);
            c.Line(8, 8, 2, 5, b, 1.5f); c.Line(2, 5, 2, 1, b, 1.2f);
            c.Line(9, 7, 5, 2, b, 1.5f);
            c.Ell(12, 8, 6, 5.5f, a);
            c.Ell(12, 16, 4.5f, 3.5f, b);
            c.Rect(9, 16, 10, 17, R);
            c.Rect(11, 6, 11, 10, R);
            c.MirrorLeft();
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Snake(Color a)
    {
        return Cached(Key("snake", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Ell(12, 4, 9.5f, 3.5f, a);
            c.Ell(12, 4, 6, 1.2f, b);
            c.Rect(9, 6, 11, 12, a);
            c.Ell(12, 15, 7.5f, 4.5f, a);
            c.Ell(12, 15, 3.5f, 3, l);
            c.Ell(12, 19, 3.5f, 3, a);
            c.Rect(10, 19, 10, 20, Y); c.Px(10, 19, K);
            c.MirrorLeft();
            c.Rect(11, 15, 12, 16, R);
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Imp(Color a)
    {
        return Cached(Key("imp", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Tri(8, 11, 1, 20, 2, 8, b);
            c.Tri(6.5f, 20, 8.5f, 20, 6, 23, Bone);
            c.Ell(12, 16, 5, 4, a);
            c.Rect(8, 16, 9, 17, Y);
            c.Rect(10, 13, 11, 13, K);
            c.Ell(12, 8.5f, 4.5f, 5, a);
            c.Ell(12, 8, 2.5f, 3, l);
            c.Rect(8, 1, 9, 4, b); c.Rect(7, 0, 9, 0, K);
            c.Rect(5, 7, 7, 8, a);
            c.MirrorLeft();
            c.Line(15, 3, 20, 5, a, 1.5f); c.Tri(20, 3, 20, 8, 23, 5, a);
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Zombie(Color a)
    {
        return Cached(Key("zombie", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Ell(12, 17, 5, 4.5f, a);
            c.Rect(8, 20, 10, 21, DBrown);
            c.Rect(8, 17, 9, 18, W); c.Px(9, 17, K);
            c.Rect(9, 14, 11, 14, K); c.Px(10, 14, W);
            c.Rect(7, 6, 11, 13, new Color(0.35f, 0.3f, 0.5f));
            c.Rect(9, 8, 10, 9, new Color(0.55f, 0.1f, 0.12f));
            c.Rect(2, 11, 6, 12, a); c.Rect(1, 10, 2, 13, l);
            c.Rect(8, 1, 10, 5, new Color(0.25f, 0.25f, 0.35f));
            c.Rect(7, 0, 10, 0, K);
            c.MirrorLeft();
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Bat(Color a)
    {
        return Cached(Key("bat", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Tri(10, 14, 1, 20, 1, 8, b);
            c.Tri(10, 11, 4, 3, 1, 8, b);
            c.Line(10, 14, 1, 20, a, 1); c.Line(10, 12, 1, 8, a, 1);
            c.Ell(12, 12, 3.5f, 4.5f, a);
            c.Tri(9.5f, 15, 10.5f, 15, 9, 19, a);
            c.Rect(10, 13, 10, 14, R);
            c.Px(11, 10, W);
            c.MirrorLeft();
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Orc(Color a)
    {
        return Cached(Key("orc", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Ell(12, 17, 5.5f, 4.5f, a);
            c.Rect(8, 17, 9, 18, R); c.Line(7, 20, 10, 18, K);
            c.Rect(9, 13, 11, 13, K); c.Tri(8, 13, 9.5f, 13, 8, 16, W);
            c.Rect(5, 6, 11, 12, D);
            c.Rect(5, 8, 11, 8, DD);
            c.Ell(4.5f, 12, 3, 2.5f, S);
            c.Rect(2, 5, 4, 11, a);
            c.Rect(6, 0, 10, 5, b);
            c.MirrorLeft();
            c.Line(21, 3, 21, 16, Brown, 2); c.Ell(21, 17, 2.5f, 2.5f, D);
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Troll(Color a)
    {
        return Cached(Key("troll", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Ell(12, 8, 8.5f, 7.5f, a);
            c.Ell(12, 7, 5, 5, l);
            c.Ell(12, 17.5f, 4.5f, 4, a);
            c.Rect(9, 17, 10, 18, Y); c.Px(10, 17, K);
            c.Rect(9, 14, 11, 14, K); c.Px(9, 13, W);
            c.Rect(2, 5, 4, 14, a); c.Ell(3, 4, 2.5f, 2, b);
            c.Rect(7, 0, 10, 2, b);
            c.MirrorLeft();
            c.Line(21, 1, 21, 15, Brown, 2); c.Ell(21, 17, 2.5f, 3, Brown);
            c.Px(20, 18, D); c.Px(22, 16, D);
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Golem(Color a, bool crystal)
    {
        return Cached(Key(crystal ? "crystal" : "golem", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Rect(5, 1, 11, 5, b);
            c.Ell(12, 10, 8.5f, 6.5f, a);
            c.Ell(12, 18, 4.5f, 3.5f, a);
            c.Ell(4, 10, 3, 5, a);
            c.Rect(9, 18, 10, 18, crystal ? Cyan : R);
            c.Rect(9, 17, 10, 17, crystal ? Cyan : R);
            if (crystal)
            {
                c.Tri(6, 14, 9, 14, 7, 20, Cyan); c.Tri(10, 15, 12, 15, 11.5f, 22, W);
                c.Tri(4, 8, 7, 8, 3, 13, Cyan);
            }
            else
            {
                c.Line(8, 12, 12, 8, DD, 1); c.Line(12, 8, 10, 4, DD, 1);
                c.Rect(7, 11, 8, 12, l);
            }
            c.MirrorLeft();
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Werewolf(Color a)
    {
        return Cached(Key("werewolf", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Tri(6, 17, 9, 17, 6, 23, a);
            c.Ell(12, 17, 5, 4, a);
            c.Ell(12, 14, 3, 2, l);
            c.Rect(8, 17, 9, 18, Y);
            c.Rect(11, 14, 11, 15, K); c.Px(9, 12, W);
            c.Ell(12, 8.5f, 7, 5.5f, a);
            c.Ell(12, 8, 3.5f, 3, l);
            c.Rect(2, 4, 4, 12, a);
            c.Tri(1, 4, 2, 1, 4, 4, W); c.Tri(3, 3, 3.5f, 0, 5, 3, W);
            c.Rect(7, 0, 10, 3, b);
            c.MirrorLeft();
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Turret(Color a)
    {
        return Cached(Key("turret", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Rect(3, 1, 11, 4, D); c.Rect(3, 4, 11, 4, S);
            c.Rect(8, 5, 11, 7, DD);
            c.Ell(12, 11, 7, 5.5f, a);
            c.Ell(10, 13, 2.5f, 1.5f, l);
            c.Px(4, 2, S); c.Px(6, 2, S);
            c.Rect(8, 10, 11, 11, R);
            c.MirrorLeft();
            c.Rect(10, 14, 13, 21, DD); c.Rect(10, 21, 13, 21, S); c.Rect(11, 20, 12, 21, K);
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Sentry(Color a)
    {
        return Cached(Key("sentry", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Line(9, 8, 5, 1, DD, 2);
            c.Rect(5, 0, 8, 1, D);
            c.Ell(12, 14, 7, 7, b);
            c.Ell(12, 14, 5.5f, 5.5f, a);
            c.Ell(12, 14, 3.5f, 3.5f, l);
            c.Ell(12, 14, 1.8f, 1.8f, W);
            c.Tri(1, 14, 5, 16, 5, 12, S);
            c.Tri(10, 22, 14, 22, 12, 19, S);
            c.Tri(4, 20, 6, 21, 7, 18, S);
            c.MirrorLeft();
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Charger(Color a, bool crown = false)
    {
        return Cached(Key(crown ? "boar" : "charger", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Ell(12, 9, 10, 7, a);
            c.Tri(5, 15, 3, 21, 9, 17, b);
            c.Ell(12, 12, 7, 6, a);
            c.Ell(12, 8, 4.5f, 3.2f, new Color(0.9f, 0.62f, 0.58f));
            c.Rect(10, 8, 10, 9, K);
            c.Rect(7, 13, 8, 14, R); c.Line(6, 16, 9, 14, K);
            c.Tri(6, 10, 3, 4, 8, 6, W);
            c.Rect(4, 0, 7, 3, K);
            c.MirrorLeft();
            if (crown)
            {
                c.Rect(8, 18, 15, 19, Y);
                c.Tri(8, 20, 10, 20, 9, 23, Y); c.Tri(11, 20, 13, 20, 12, 23, Y); c.Tri(14, 20, 16, 20, 15, 23, Y);
            }
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Shaman(Color a)
    {
        return Cached(Key("shaman", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Rect(4, 1, 11, 3, a);
            c.Rect(5, 4, 11, 8, a);
            c.Rect(6, 9, 11, 12, a);
            c.Rect(4, 1, 11, 1, Y);
            c.Rect(9, 4, 11, 10, b);
            c.Ell(12, 15.5f, 6, 6, b);
            c.Ell(12, 14.5f, 4, 3.5f, K);
            c.Rect(9, 14, 10, 15, Green);
            c.Ell(12, 20, 2, 1.2f, Bone);
            c.Rect(3, 8, 5, 11, b);
            c.MirrorLeft();
            c.Line(20, 1, 20, 17, Brown, 1.5f);
            c.Ell(20, 19, 2.3f, 2.3f, Green); c.Ell(20, 19, 1, 1, W);
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Blinker(Color a)
    {
        return Cached(Key("blinker", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Ring(12, 12, 8f, 10.5f, a);
            c.Ring(12, 12, 6.5f, 8f, b);
            c.Ell(12, 12, 6.5f, 6.5f, new Color(0.1f, 0.04f, 0.2f));
            c.Ell(12, 12, 5, 3.5f, W);
            c.Ell(12, 12, 2.3f, 2.3f, a);
            c.Rect(11, 10, 12, 14, K);
            c.Px(3, 20, l); c.Px(20, 4, l); c.Px(2, 5, a); c.Px(21, 19, a);
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite ShieldKnight(Color a)
    {
        return Cached(Key("shieldknight", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Ell(12, 17, 5.5f, 5, S);
            c.Rect(8, 16, 11, 17, K);
            c.Rect(11, 21, 12, 23, R);
            c.Rect(6, 6, 11, 12, D);
            c.Rect(9, 8, 11, 10, a);
            c.Rect(3, 6, 5, 12, S);
            c.Rect(7, 1, 10, 5, D); c.Rect(7, 0, 10, 0, K);
            c.MirrorLeft();
            c.Line(21, 4, 21, 17, S, 1.5f); c.Rect(19, 5, 23, 5, Brown);
            c.Outline(K);
            return c.ToSprite();
        });
    }

    // отдельный щит - вращается к игроку
    public static Sprite Shield()
    {
        return Cached("shield", () =>
        {
            var c = new PixelCanvas(12, 20);
            c.Ell(6, 10, 4.5f, 9, D);
            c.Ell(6, 10, 3, 7.5f, S);
            c.Rect(5, 3, 6, 17, DD);
            c.Ell(6, 10, 1.5f, 1.5f, Y);
            c.Outline(K);
            return c.ToSprite(0.5f, 0.5f, 20);
        });
    }

    public static Sprite Mortar(Color a)
    {
        return Cached(Key("mortar", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Rect(4, 4, 19, 8, Brown);
            c.Rect(4, 8, 19, 8, DBrown);
            c.Line(9, 8, 15, 20, D, 5); c.Line(9, 8, 15, 20, S, 2);
            c.Ell(16, 20.5f, 3, 1.5f, K);
            c.Ell(6, 4, 4.5f, 4.5f, DBrown); c.Ell(6, 4, 2.5f, 2.5f, D); c.Px(6, 4, S);
            c.Ell(18, 4, 4.5f, 4.5f, DBrown); c.Ell(18, 4, 2.5f, 2.5f, D); c.Px(18, 4, S);
            c.Ell(21, 10, 2, 2, K); c.Px(21, 12, Y); c.Px(22, 13, O);
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Marksman(Color a)
    {
        return Cached(Key("marksman", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Rect(4, 1, 11, 4, a); c.Rect(5, 5, 11, 10, a);
            c.Rect(9, 3, 11, 9, b);
            c.Ell(12, 15, 5.5f, 5.5f, b);
            c.Rect(5, 17, 11, 18, DBrown);
            c.Ell(12, 14.5f, 3.5f, 3, K);
            c.Rect(9, 14, 10, 15, R);
            c.MirrorLeft();
            c.Rect(1, 9, 23, 10, DD); c.Rect(1, 7, 5, 10, Brown);
            c.Rect(11, 11, 15, 12, K); c.Px(15, 11, R);
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Ninja(Color a)
    {
        return Cached(Key("ninja", a), () =>
        {
            var c = E(); Color b = Dk(a), l = Lt(a);
            c.Ell(12, 16, 5.5f, 5, a);
            c.Rect(8, 15, 11, 16, Skin); c.Px(9, 15, W); c.Px(9, 16, K);
            c.Rect(6, 18, 11, 19, R);
            c.Rect(6, 5, 11, 12, a);
            c.Rect(6, 7, 11, 7, R);
            c.Rect(3, 6, 5, 12, a); c.Rect(3, 5, 4, 5, Skin);
            c.Rect(7, 0, 10, 4, b);
            c.MirrorLeft();
            c.Line(19, 19, 23, 22, R, 1.5f); c.Line(19, 18, 23, 16, R, 1.5f);
            c.Line(19, 8, 23, 8, W, 1); c.Line(21, 6, 21, 10, W, 1);
            c.Outline(K);
            return c.ToSprite();
        });
    }

    // ================= БОССЫ (32x32) =================

    public static Sprite Dragon()
    {
        return Cached("dragon", () =>
        {
            var c = new PixelCanvas(32, 32);
            Color a = new Color(0.8f, 0.15f, 0.12f), b = Dk(a), l = Lt(a);
            c.Tri(15, 20, 1, 30, 2, 8, b);
            c.Tri(15, 16, 6, 4, 2, 9, b);
            c.Line(15, 20, 1, 30, a, 1.5f); c.Line(15, 16, 2, 9, a, 1.5f);
            c.Ell(16, 12, 9, 9, a);
            c.Ell(16, 10, 5.5f, 6.5f, new Color(1f, 0.8f, 0.4f));
            c.Ell(16, 23, 6, 5, a);
            c.Ell(16, 20, 3.5f, 2.6f, l);
            c.Rect(12, 23, 14, 25, Y); c.Px(13, 24, K);
            c.Rect(13, 20, 13, 21, K);
            c.Tri(10, 26, 12, 26, 9, 31, Bone);
            c.Rect(11, 1, 14, 4, b);
            c.MirrorLeft();
            c.Tri(20, 4, 28, 2, 30, 8, a);
            c.Outline(K);
            return c.ToSprite();
        });
    }

    public static Sprite Necromancer()
    {
        return Cached("necromancer", () =>
        {
            var c = new PixelCanvas(32, 32);
            Color a = new Color(0.4f, 0.12f, 0.6f), b = Dk(a), l = Lt(a);
            c.Rect(5, 1, 15, 5, a); c.Rect(6, 6, 15, 12, a); c.Rect(7, 13, 15, 17, a);
            c.Rect(5, 1, 15, 1, Green);
            c.Rect(12, 5, 15, 16, b);
            c.Ell(16, 21, 7.5f, 7.5f, b);
            c.Ell(16, 20, 5, 5, Bone);
            c.Rect(12, 20, 13, 22, K); c.Rect(12, 20, 13, 20, Green);
            c.Rect(15, 17, 15, 18, K); c.Px(13, 17, K);
            c.Rect(12, 25, 15, 27, Bone);
            c.Rect(3, 10, 6, 15, b);
            c.MirrorLeft();
            c.Line(28, 2, 28, 24, Brown, 2);
            c.Ell(28, 27, 3, 3, Bone); c.Rect(27, 27, 27, 28, K); c.Rect(29, 27, 29, 28, Green);
            c.Ring(28, 27, 3.5f, 4.5f, Green);
            c.Outline(K);
            return c.ToSprite();
        });
    }

    // ================= ОРУЖИЕ (28x12, хват в x=8) =================

    public const int WeaponGripX = 8;
    const int WW = 28, WH = 12;

    public static Sprite Weapon(string name)
    {
        return Cached("w_" + name, () =>
        {
            var c = new PixelCanvas(WW, WH);
            DrawWeapon(c, name);
            c.Outline(K);
            return c.ToSprite(WeaponGripX / (float)WW, 0.5f, 20);
        });
    }

    static void Pistol(PixelCanvas c, Color body, int len, int oy = 0)
    {
        c.Rect(6, 6 + oy, 6 + len, 9 + oy, body);
        c.Rect(7, 9 + oy, 5 + len, 9 + oy, S);
        c.Rect(6 + len, 7 + oy, 8 + len, 8 + oy, S);
        c.Rect(6, 2 + oy, 9, 5 + oy, Brown);
        c.Rect(10, 5 + oy, 11, 5 + oy, DD);
    }

    static void Rifle(PixelCanvas c, Color body, Color wood, int len, bool scope, bool mag, bool drum = false)
    {
        c.Rect(0, 4, 7, 8, wood);
        c.Rect(7, 5, 7 + len, 8, body);
        c.Rect(7 + len, 6, 9 + len, 7, S);
        c.Rect(10, 8, 6 + len, 8, Lt(body));
        c.Rect(7, 1, 9, 5, DBrown);
        if (mag && !drum) c.Rect(12, 0, 14, 4, DD);
        if (drum) c.Ell(13, 3, 3.5f, 3.5f, DD);
        if (scope) { c.Rect(12, 9, 18, 10, K); c.Px(19, 9, Cyan); }
    }

    static void DrawWeapon(PixelCanvas c, string n)
    {
        switch (n)
        {
            case "Pistol": Pistol(c, D, 13); break;
            case "Dual": Pistol(c, D, 12); c.Rect(7, 10, 19, 11, S); c.Rect(19, 10, 21, 11, D); break;
            case "Revolver":
                Pistol(c, D, 10); c.Ell(13, 7.5f, 3.2f, 3.2f, S); c.Rect(16, 7, 24, 8, D); c.Px(13, 7, K); break;
            case "DesertEagle":
                c.Rect(6, 6, 22, 10, S); c.Rect(6, 6, 22, 6, D); c.Rect(22, 7, 25, 9, S);
                c.Rect(6, 1, 10, 5, DBrown); c.Rect(9, 11, 10, 11, K); c.Rect(19, 11, 20, 11, K); break;
            case "Shotgun":
                c.Rect(0, 3, 7, 7, Brown); c.Rect(7, 6, 26, 8, D); c.Rect(12, 4, 19, 6, Brown);
                c.Rect(7, 1, 9, 5, DBrown); c.Rect(26, 6, 27, 8, S); break;
            case "SuperShotgun":
                c.Rect(0, 3, 7, 7, Brown); c.Rect(7, 8, 26, 9, D); c.Rect(7, 6, 26, 7, DD);
                c.Rect(7, 1, 9, 5, DBrown); c.Rect(26, 6, 27, 9, S); c.Rect(14, 4, 18, 5, Y); break;
            case "TacticalSG":
                c.Rect(0, 4, 7, 7, DD); c.Rect(7, 5, 24, 8, DD); c.Rect(7, 9, 24, 9, S);
                c.Rect(11, 2, 16, 4, D); c.Rect(7, 1, 9, 5, DD); c.Rect(24, 6, 27, 7, S); break;
            case "Uzi": c.Rect(6, 5, 20, 9, DD); c.Rect(20, 6, 24, 7, S); c.Rect(7, 0, 9, 5, D); c.Rect(12, 1, 14, 4, D); c.Rect(6, 9, 20, 9, D); break;
            case "Mac10": c.Rect(6, 5, 17, 9, D); c.Rect(17, 6, 22, 7, S); c.Rect(7, 0, 9, 5, DD); c.Rect(12, 2, 13, 4, DD); c.Rect(6, 10, 9, 10, S); break;
            case "Thompson":
                c.Rect(0, 4, 7, 7, Brown); c.Rect(7, 5, 22, 8, DD); c.Rect(22, 6, 26, 7, S);
                c.Ell(13, 3, 3.5f, 3.5f, D); c.Rect(7, 1, 9, 5, Brown); c.Rect(16, 3, 21, 5, Brown); break;
            case "AK": Rifle(c, DD, Brown, 15, false, true); c.Rect(16, 0, 18, 3, DD); c.Rect(14, 3, 16, 4, DD); break;
            case "M4": Rifle(c, DD, DD, 15, true, true); break;
            case "LMG": Rifle(c, D, DD, 15, false, true, true); c.Line(22, 5, 25, 1, D, 1); c.Line(22, 5, 20, 1, D, 1); break;
            case "Famas": Rifle(c, D, Dk(Green, 0.6f), 12, false, true); c.Rect(2, 8, 6, 9, S); break;
            case "Sniper": Rifle(c, DD, DBrown, 17, true, false); c.Rect(12, 9, 20, 11, DD); c.Px(21, 10, Cyan); c.Line(24, 5, 26, 1, D, 1); break;
            case "AWP": Rifle(c, new Color(0.22f, 0.4f, 0.25f), new Color(0.22f, 0.4f, 0.25f), 17, true, false); c.Rect(12, 9, 20, 11, DD); c.Px(21, 10, R); break;
            case "Crossbow":
                c.Rect(5, 6, 22, 7, Brown); c.Rect(7, 3, 9, 6, DBrown);
                c.Line(20, 6, 15, 1, D, 2); c.Line(20, 7, 15, 11, D, 2); c.Line(15, 1, 15, 11, W, 1);
                c.Line(9, 7, 25, 7, S, 1); c.Tri(25, 5, 25, 9, 27, 7, S); break;
            case "LaserRifle":
                c.Rect(0, 5, 7, 8, DD); c.Rect(7, 5, 22, 8, W); c.Rect(7, 6, 22, 6, Cyan);
                c.Rect(22, 6, 26, 7, Cyan); c.Rect(7, 1, 9, 5, DD); c.Rect(12, 9, 17, 10, Cyan); break;
            case "Laser": c.Rect(6, 5, 18, 8, W); c.Rect(6, 6, 18, 6, Cyan); c.Rect(18, 6, 24, 7, Cyan); c.Rect(6, 1, 9, 5, DD); c.Px(10, 9, R); break;
            case "Plasma":
                c.Rect(6, 5, 16, 8, DD); c.Ell(20, 6.5f, 5, 5, new Color(0.5f, 0.2f, 1f)); c.Ell(20, 6.5f, 2.5f, 2.5f, W);
                c.Rect(6, 1, 9, 5, DD); break;
            case "Shock":
                c.Rect(6, 5, 14, 8, DD); c.Rect(14, 6, 24, 7, D);
                for (int x = 15; x < 24; x += 3) c.Rect(x, 4, x + 1, 9, Y);
                c.Rect(6, 1, 9, 5, DD); break;
            case "Thunder":
                c.Rect(6, 5, 12, 8, DD); c.Line(12, 6, 16, 10, Y, 1.5f); c.Line(16, 10, 19, 3, Y, 1.5f); c.Line(19, 3, 25, 7, Y, 1.5f);
                c.Rect(6, 1, 9, 5, DD); break;
            case "IceGun":
                c.Rect(6, 5, 15, 8, S); c.Tri(15, 4, 15, 10, 27, 7, Cyan); c.Tri(15, 6, 15, 8, 23, 7, W);
                c.Rect(6, 1, 9, 5, D); break;
            case "PoisonGun":
                c.Rect(6, 5, 20, 8, D); c.Ell(13, 10, 3.5f, 2.5f, Green); c.Rect(20, 6, 25, 7, Green);
                c.Rect(6, 1, 9, 5, DBrown); c.Px(13, 10, W); break;
            case "Rocket":
                c.Rect(0, 4, 6, 9, DD); c.Rect(6, 4, 24, 9, new Color(0.35f, 0.45f, 0.3f)); c.Rect(6, 9, 24, 9, S);
                c.Tri(24, 3, 24, 10, 27, 6.5f, R); c.Rect(7, 0, 9, 4, DD); c.Rect(14, 4, 15, 9, Y); break;
            case "Flamethrower":
                c.Rect(6, 5, 14, 9, D); c.Ell(9, 4, 4, 3, R); c.Rect(14, 6, 22, 7, DD); c.Rect(22, 5, 24, 8, D);
                c.Tri(24, 5, 24, 8, 27, 6.5f, O); c.Rect(6, 1, 8, 4, DBrown); break;
            case "HolyGrenade":
                c.Ell(13, 6, 6, 6, Y); c.Rect(12, 2, 13, 10, W); c.Rect(9, 6, 16, 7, W); c.Rect(12, 11, 14, 11, DD);
                c.Ell(11, 8, 1.5f, 1.5f, new Color(1f, 1f, 0.8f)); break;
            case "GrenadeLauncher":
                c.Rect(0, 4, 6, 7, Brown); c.Rect(6, 4, 20, 9, DD); c.Ell(13, 6.5f, 4, 4, D); c.Rect(20, 3, 26, 10, D);
                c.Rect(26, 4, 27, 9, K); c.Rect(6, 0, 9, 4, DBrown); break;
            case "Minigun":
                c.Rect(6, 4, 11, 9, DD); c.Rect(11, 3, 26, 4, D); c.Rect(11, 6, 26, 7, S); c.Rect(11, 9, 26, 10, D);
                c.Rect(24, 3, 27, 10, DD); c.Rect(6, 0, 9, 4, DBrown); c.Rect(7, 9, 9, 11, R); break;
            case "Boomerang":
                c.Line(6, 9, 15, 2, Brown, 3); c.Line(15, 2, 24, 9, Brown, 3);
                c.Line(6, 9, 15, 2, O, 1); c.Line(15, 2, 24, 9, O, 1); break;
            case "Star Wand":
                c.Line(2, 3, 18, 7, Brown, 1.5f); c.Ell(21, 7.5f, 4, 4, Y);
                c.Tri(21, 2, 19, 7, 23, 7, Y); c.Tri(15, 8, 27, 8, 21, 11, Y); c.Ell(21, 7.5f, 1.5f, 1.5f, W); break;
            case "FairyGun":
                c.Rect(6, 5, 18, 8, new Color(1f, 0.6f, 0.85f)); c.Ell(21, 6.5f, 3.5f, 3.5f, W);
                c.Tri(10, 9, 16, 9, 12, 12, Cyan); c.Tri(10, 4, 16, 4, 12, 0, Cyan); c.Rect(6, 1, 9, 5, DBrown); break;
            case "Sword":
                c.Rect(4, 5, 7, 7, DBrown); c.Rect(8, 2, 9, 10, Y); c.Rect(10, 5, 25, 8, S); c.Rect(10, 6, 25, 6, W);
                c.Tri(25, 5, 25, 8, 27, 7, S); break;
            case "Katana":
                c.Rect(3, 5, 8, 6, DD); c.Rect(9, 3, 9, 8, Y); c.Line(10, 6, 26, 8, S, 2); c.Line(10, 6, 26, 8, W, 1); break;
            case "Mace":
                c.Rect(2, 5, 17, 6, Brown); c.Ell(21, 6, 4.5f, 4.5f, D); c.Ell(21, 6, 2.5f, 2.5f, S);
                c.Tri(20, 10, 22, 10, 21, 12, S); c.Tri(20, 2, 22, 2, 21, 0, S); c.Tri(25, 5, 25, 7, 27, 6, S); break;
            case "Axe":
                c.Rect(2, 5, 20, 6, Brown); c.Tri(16, 6, 22, 6, 17, 12, S); c.Tri(16, 5, 22, 5, 17, 0, S);
                c.Rect(16, 3, 22, 9, D); c.Rect(21, 4, 24, 8, S); break;
            case "Scythe":
                c.Rect(0, 5, 20, 6, Brown); c.Line(19, 6, 22, 10, D, 2); c.Line(22, 10, 27, 9, S, 2); c.Line(27, 9, 27, 4, S, 1.5f); c.Line(21, 10, 26, 8, W, 1); break;
            default: Pistol(c, D, 13); break;
        }
    }

    // ================= ПУЛИ (16x16, летят вправо) =================

    public static Sprite BulletSprite(BulletStyle style, Color col)
    {
        return Cached(Key("b_" + style, col), () =>
        {
            var c = new PixelCanvas(16, 16);
            Color hi = Color.Lerp(col, Color.white, 0.6f);
            switch (style)
            {
                case BulletStyle.Slug: c.Rect(3, 6, 13, 9, col); c.Rect(4, 7, 12, 8, hi); c.Px(14, 7, col); c.Px(14, 8, col); break;
                case BulletStyle.Streak: c.Rect(0, 7, 15, 8, col); c.Rect(6, 7, 15, 8, hi); c.Rect(2, 6, 9, 9, new Color(col.r, col.g, col.b, 0.5f)); break;
                case BulletStyle.Laser: c.Rect(0, 6, 15, 9, new Color(col.r, col.g, col.b, 0.55f)); c.Rect(0, 7, 15, 8, hi); break;
                case BulletStyle.Orb:
                    c.Ell(8, 8, 8, 8, new Color(col.r, col.g, col.b, 0.35f)); c.Ell(8, 8, 5.5f, 5.5f, col); c.Ell(8, 8, 3, 3, hi); break;
                case BulletStyle.Rocket:
                    c.Rect(2, 6, 11, 9, S); c.Tri(11, 5, 11, 10, 15, 7.5f, R); c.Tri(2, 6, 2, 9, 0, 7.5f, O); c.Tri(3, 6, 5, 6, 3, 3, D); c.Tri(3, 9, 5, 9, 3, 12, D); break;
                case BulletStyle.Flame:
                    c.Ell(8, 8, 7, 6, new Color(1f, 0.35f, 0.05f, 0.8f)); c.Ell(9, 8, 5, 4, new Color(1f, 0.7f, 0.1f, 0.9f)); c.Ell(10, 8, 2.5f, 2, new Color(1f, 1f, 0.6f)); break;
                case BulletStyle.Slash:
                    for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++)
                        {
                            float dx = x + 0.5f - 1f, dy = y + 0.5f - 8f;
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            if (d >= 8.5f && d <= 14.5f && dx > 0)
                            {
                                float edge = Mathf.InverseLerp(8.5f, 14.5f, d);
                                c.px[y * 16 + x] = Color.Lerp(col, hi, 1f - Mathf.Abs(edge - 0.6f) * 2f);
                                c.px[y * 16 + x].a = Mathf.Clamp01(1.3f - Mathf.Abs(dy) / 9f);
                            }
                        }
                    break;
                case BulletStyle.Boomerang:
                    c.Line(3, 13, 8, 3, col, 3); c.Line(8, 3, 13, 13, col, 3); c.Line(3, 13, 8, 3, hi, 1); c.Line(8, 3, 13, 13, hi, 1); break;
                case BulletStyle.Star:
                    c.Tri(8, 15.5f, 1, 4, 15, 4, col); c.Tri(8, 0.5f, 1, 11.5f, 15, 11.5f, col); c.Ell(8, 8, 2.2f, 2.2f, hi); break;
                case BulletStyle.Bolt:
                    c.Rect(1, 7, 12, 8, Brown); c.Tri(12, 5, 12, 10, 15, 7.5f, S); c.Tri(0, 5, 2, 7, 0, 7, col); c.Tri(0, 10, 2, 8, 0, 8, col); break;
                case BulletStyle.Shard:
                    c.Tri(2, 5, 2, 10, 15, 7.5f, col); c.Tri(4, 6.5f, 4, 8.5f, 12, 7.5f, hi); break;
                case BulletStyle.Grenade:
                    c.Ell(8, 8, 6, 6, col); c.Ell(6, 10, 2, 2, hi); c.Rect(7, 13, 9, 15, DD); break;
                case BulletStyle.Drop:
                    c.Ell(8, 8, 6.5f, 6.5f, col); c.Ell(6.5f, 9.5f, 2, 2, hi); break;
                default:
                    c.Ell(8, 8, 7.5f, 7.5f, col); c.Ell(6, 10, 3, 3, hi); break;
            }
            return c.ToSprite();
        });
    }

    // ================= ОКРУЖЕНИЕ =================

    public static Sprite DoorBars(Color glow)
    {
        return Cached(Key("door", glow), () =>
        {
            var c = new PixelCanvas(16, 16);
            c.Rect(0, 0, 15, 15, new Color(0.14f, 0.1f, 0.12f));
            for (int x = 1; x < 16; x += 4) { c.Rect(x, 0, x + 1, 15, D); c.Rect(x, 0, x, 15, S); }
            c.Rect(0, 3, 15, 4, DD); c.Rect(0, 11, 15, 12, DD);
            c.Rect(0, 0, 15, 0, glow); c.Rect(0, 15, 15, 15, glow);
            return c.ToSprite();
        });
    }

    public static Sprite Portal()
    {
        return Cached("portal", () =>
        {
            var c = new PixelCanvas(32, 32);
            c.Ell(16, 16, 15, 15, new Color(0.3f, 0.1f, 0.6f, 0.5f));
            c.Ring(16, 16, 10, 13, new Color(0.6f, 0.35f, 1f));
            c.Ring(16, 16, 6, 8.5f, new Color(0.4f, 0.8f, 1f));
            c.Ell(16, 16, 4, 4, W);
            for (int i = 0; i < 6; i++)
            {
                float ang = i * 60f * Mathf.Deg2Rad;
                c.Px(16 + Mathf.RoundToInt(Mathf.Cos(ang) * 14), 16 + Mathf.RoundToInt(Mathf.Sin(ang) * 14), Cyan);
            }
            return c.ToSprite();
        });
    }

    // ================= Таблица врагов =================

    // Возвращает спрайт и тинт (Color.white = без тонировки)
    public static Sprite Enemy(string type, out Color tint)
    {
        tint = Color.white;
        switch (type)
        {
            case "Slime": return Slime(new Color(0.9f, 0.25f, 0.25f));
            case "MiniSlime": return Slime(new Color(0.3f, 0.85f, 0.4f));
            case "BigSlime": return Slime(new Color(0.3f, 0.85f, 0.4f), true);
            case "Speedster": return Slime(new Color(0.25f, 0.85f, 0.95f));
            case "Goblin": return Goblin(new Color(0.35f, 0.75f, 0.25f));
            case "Skeleton": return SpriteGenerator.CreateSkeleton(32);
            case "Zombie": return Zombie(new Color(0.45f, 0.62f, 0.4f));
            case "Wolf": return Wolf(new Color(0.55f, 0.47f, 0.4f));
            case "Werewolf": return Werewolf(new Color(0.42f, 0.35f, 0.55f));
            case "Bat": return Bat(new Color(0.4f, 0.28f, 0.5f));
            case "Spider": return Spider(new Color(0.28f, 0.24f, 0.34f));
            case "Snake": return Snake(new Color(0.45f, 0.75f, 0.2f));
            case "Imp": return Imp(new Color(0.95f, 0.35f, 0.15f));
            case "Orc": return Orc(new Color(0.4f, 0.6f, 0.3f));
            case "Berserker": return Orc(new Color(0.75f, 0.3f, 0.25f));
            case "Troll": return Troll(new Color(0.5f, 0.66f, 0.36f));
            case "Abomination": return Troll(new Color(0.6f, 0.5f, 0.65f));
            case "Golem": return Golem(new Color(0.55f, 0.5f, 0.45f), false);
            case "CrystalGolem": return Golem(new Color(0.35f, 0.5f, 0.8f), true);
            case "Turret": return Turret(new Color(0.55f, 0.58f, 0.66f));
            case "Sentry": return Sentry(new Color(0.65f, 0.3f, 0.9f));
            case "Charger": return Charger(new Color(0.55f, 0.36f, 0.24f));
            case "Shaman": return Shaman(new Color(0.25f, 0.55f, 0.4f));
            case "Blinker": return Blinker(new Color(0.85f, 0.3f, 0.85f));
            case "ShieldKnight": return ShieldKnight(new Color(0.25f, 0.45f, 0.9f));
            case "Mortar": return Mortar(Color.white);
            case "Marksman": return Marksman(new Color(0.32f, 0.45f, 0.3f));
            case "Ninja": return Ninja(new Color(0.16f, 0.16f, 0.22f));
            case "Assassin": return Ninja(new Color(0.5f, 0.2f, 0.55f));

            case "DarkKnight": return SpriteGenerator.CreateDarkKnight(32);
            case "Guardian": tint = new Color(1f, 0.9f, 0.55f); return SpriteGenerator.CreateDarkKnight(32);
            case "Shooter": return SpriteGenerator.CreateShooter(32);
            case "Archer": tint = new Color(0.7f, 1f, 0.65f); return SpriteGenerator.CreateShooter(32);
            case "Mage": return SpriteGenerator.CreateMage(32);
            case "IceMage": tint = new Color(0.65f, 0.9f, 1f); return SpriteGenerator.CreateMage(32);
            case "FireMage": tint = new Color(1f, 0.6f, 0.4f); return SpriteGenerator.CreateMage(32);
            case "NecroMage": tint = new Color(0.75f, 0.5f, 1f); return SpriteGenerator.CreateMage(32);
            case "Lich": tint = new Color(0.65f, 1f, 0.85f); return SpriteGenerator.CreateMage(32);
            case "Warlock": tint = new Color(1f, 0.5f, 0.9f); return SpriteGenerator.CreateMage(32);
            case "StormMage": tint = new Color(1f, 1f, 0.55f); return SpriteGenerator.CreateMage(32);
            case "Ghost": return SpriteGenerator.CreateGhost(32);
            case "Wraith": tint = new Color(0.6f, 0.7f, 1f, 0.85f); return SpriteGenerator.CreateGhost(32);
            case "Shadow": tint = new Color(0.4f, 0.35f, 0.5f, 0.9f); return SpriteGenerator.CreateGhost(32);
            case "Bomber": return SpriteGenerator.CreateBomber(32);
            case "FireElemental": tint = new Color(1f, 0.6f, 0.2f); return SpriteGenerator.CreateBomber(32);
            case "LivingBomb": tint = new Color(1f, 0.35f, 0.35f); return SpriteGenerator.CreateBomber(32);
            case "Tank": return SpriteGenerator.CreateTank(32);
            case "Flyer": return SpriteGenerator.CreateFlyer(32);
            case "Harpy": tint = new Color(1f, 0.7f, 0.85f); return SpriteGenerator.CreateFlyer(32);
            case "Spawner": return SpriteGenerator.CreateBoss(48, new Color(0.6f, 0.2f, 0.6f));
            case "Nightmare": return SpriteGenerator.CreateBoss(36, new Color(0.3f, 0.1f, 0.4f));

            case "CrownedBoar": return Charger(new Color(0.65f, 0.4f, 0.2f), true);
            case "Dragon": return Dragon();
            case "Necromancer": return Necromancer();
            default: return Slime(new Color(0.9f, 0.25f, 0.25f));
        }
    }
}
