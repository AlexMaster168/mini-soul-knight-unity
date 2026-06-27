using UnityEngine;

public static class SpriteGenerator
{
    public static Sprite CreateSquare(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateCircle(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        float center = size / 2f;
        float radius = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                pixels[y * size + x] = (dx * dx + dy * dy <= radius * radius) ? color : clear;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateKnight(int size, Color bodyColor, Color helmetColor)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        // Helmet (top 40%)
        int helmetH = (int)(size * 0.4f);
        for (int y = size - helmetH; y < size; y++)
            for (int x = 0; x < size; x++)
                pixels[y * size + x] = helmetColor;

        // Visor slit
        int slitY = size - (int)(helmetH * 0.5f);
        for (int x = (int)(size * 0.25f); x < (int)(size * 0.75f); x++)
            pixels[slitY * size + x] = new Color(0.2f, 0.8f, 1f, 1f);

        // Body (middle 40%)
        int bodyTop = size - helmetH;
        int bodyBot = (int)(size * 0.2f);
        for (int y = bodyBot; y < bodyTop; y++)
            for (int x = (int)(size * 0.15f); x < (int)(size * 0.85f); x++)
                pixels[y * size + x] = bodyColor;

        // Arms
        int armW = (int)(size * 0.15f);
        for (int y = (int)(size * 0.35f); y < (int)(size * 0.65f); y++)
        {
            for (int x = 0; x < armW; x++)
                pixels[y * size + x] = bodyColor;
            for (int x = size - armW; x < size; x++)
                pixels[y * size + x] = bodyColor;
        }

        // Legs (bottom 20%)
        int legW = (int)(size * 0.2f);
        int legGap = (int)(size * 0.1f);
        for (int y = 0; y < bodyBot; y++)
        {
            for (int x = (int)(size * 0.2f); x < (int)(size * 0.2f) + legW; x++)
                pixels[y * size + x] = helmetColor;
            for (int x = (int)(size * 0.5f) + legGap; x < (int)(size * 0.5f) + legGap + legW; x++)
                if (x < size) pixels[y * size + x] = helmetColor;
        }

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateSlime(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        float center = size / 2f;
        // Body blob
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center * 0.6f) / (center * 0.6f);
                if (dx * dx + dy * dy < 1f)
                {
                    // Add slight darker bottom
                    float shade = Mathf.Lerp(0.7f, 1f, (float)y / size);
                    pixels[y * size + x] = color * shade;
                    pixels[y * size + x].a = 1f;
                }
            }
        }

        // Eyes
        int eyeSize = Mathf.Max(2, size / 8);
        DrawCircle(pixels, size, (int)(center - size * 0.15f), (int)(center * 0.75f), eyeSize, Color.white);
        DrawCircle(pixels, size, (int)(center + size * 0.15f), (int)(center * 0.75f), eyeSize, Color.white);
        // Pupils
        int pupilSize = Mathf.Max(1, eyeSize / 2);
        DrawCircle(pixels, size, (int)(center - size * 0.12f), (int)(center * 0.75f), pupilSize, Color.black);
        DrawCircle(pixels, size, (int)(center + size * 0.18f), (int)(center * 0.75f), pupilSize, Color.black);

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateBoss(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        float center = size / 2f;
        // Large menacing body
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center * 0.5f) / (center * 0.7f);
                if (dx * dx + dy * dy < 1f)
                {
                    float shade = Mathf.Lerp(0.5f, 1f, (float)y / size);
                    pixels[y * size + x] = color * shade;
                    pixels[y * size + x].a = 1f;
                }
            }
        }

        // Horns
        int hornW = size / 6;
        for (int i = 0; i < size / 3; i++)
        {
            int hx = (int)(center - size * 0.3f) - i / 2;
            int hy = size - i;
            if (hx >= 0 && hx < size && hy >= 0 && hy < size)
                for (int w = 0; w < hornW; w++)
                    if (hx + w < size) pixels[hy * size + hx + w] = new Color(0.3f, 0.3f, 0.3f, 1f);

            hx = (int)(center + size * 0.3f) + i / 2;
            if (hx >= 0 && hx < size && hy >= 0 && hy < size)
                for (int w = 0; w < hornW; w++)
                    if (hx + w < size) pixels[hy * size + hx + w] = new Color(0.3f, 0.3f, 0.3f, 1f);
        }

        // Glowing eyes
        int eyeSize = Mathf.Max(3, size / 6);
        DrawCircle(pixels, size, (int)(center - size * 0.18f), (int)(center * 0.6f), eyeSize, new Color(1f, 0.3f, 0f, 1f));
        DrawCircle(pixels, size, (int)(center + size * 0.18f), (int)(center * 0.6f), eyeSize, new Color(1f, 0.3f, 0f, 1f));

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateTile(int size, Color color, Color lineColor)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;

        // Grid lines
        for (int y = 0; y < size; y++)
        {
            pixels[y * size] = lineColor;
            pixels[y * size + size - 1] = lineColor;
        }
        for (int x = 0; x < size; x++)
        {
            pixels[x] = lineColor;
            pixels[(size - 1) * size + x] = lineColor;
        }

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateWall(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noise = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.3f;
                Color c = color + new Color(noise, noise, noise, 0);
                c.a = 1f;
                pixels[y * size + x] = c;
            }
        }

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateSkeleton(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        float c = size / 2f;
        Color bone = new Color(0.9f, 0.9f, 0.8f);

        // Skull
        for (int y = (int)(size * 0.6f); y < size; y++)
            for (int x = (int)(size * 0.2f); x < (int)(size * 0.8f); x++)
                pixels[y * size + x] = bone;

        // Eye sockets
        DrawCircle(pixels, size, (int)(c - size * 0.12f), (int)(size * 0.78f), 2, Color.black);
        DrawCircle(pixels, size, (int)(c + size * 0.12f), (int)(size * 0.78f), 2, Color.black);

        // Ribcage
        for (int y = (int)(size * 0.3f); y < (int)(size * 0.6f); y++)
            for (int x = (int)(size * 0.3f); x < (int)(size * 0.7f); x++)
                if ((x + y) % 3 == 0) pixels[y * size + x] = bone;

        // Spine
        for (int y = (int)(size * 0.1f); y < (int)(size * 0.6f); y++)
            pixels[y * size + (int)c] = bone;

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateDarkKnight(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color armor = new Color(0.15f, 0.15f, 0.2f);
        Color accent = new Color(0.6f, 0.1f, 0.1f);

        // Helmet
        for (int y = (int)(size * 0.7f); y < size; y++)
            for (int x = (int)(size * 0.15f); x < (int)(size * 0.85f); x++)
                pixels[y * size + x] = armor;

        // Red visor
        for (int x = (int)(size * 0.3f); x < (int)(size * 0.7f); x++)
            pixels[(int)(size * 0.78f) * size + x] = accent;

        // Body armor
        for (int y = (int)(size * 0.25f); y < (int)(size * 0.7f); y++)
            for (int x = (int)(size * 0.1f); x < (int)(size * 0.9f); x++)
                pixels[y * size + x] = armor;

        // Shoulder pads
        for (int y = (int)(size * 0.55f); y < (int)(size * 0.7f); y++)
        {
            for (int x = 0; x < (int)(size * 0.15f); x++)
                pixels[y * size + x] = accent;
            for (int x = (int)(size * 0.85f); x < size; x++)
                pixels[y * size + x] = accent;
        }

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateShooter(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color body = new Color(0.4f, 0.5f, 0.3f);
        Color gun = new Color(0.3f, 0.3f, 0.35f);

        // Body
        for (int y = (int)(size * 0.3f); y < (int)(size * 0.8f); y++)
            for (int x = (int)(size * 0.2f); x < (int)(size * 0.65f); x++)
                pixels[y * size + x] = body;

        // Gun arm (right side)
        for (int y = (int)(size * 0.45f); y < (int)(size * 0.55f); y++)
            for (int x = (int)(size * 0.65f); x < size; x++)
                pixels[y * size + x] = gun;

        // Eye
        DrawCircle(pixels, size, (int)(size * 0.35f), (int)(size * 0.7f), 2, Color.white);

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateMage(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color robe = new Color(0.3f, 0.1f, 0.5f);
        Color hat = new Color(0.25f, 0.05f, 0.4f);

        // Pointed hat
        for (int y = (int)(size * 0.7f); y < size; y++)
        {
            float w = Mathf.Lerp(0.05f, 0.35f, (float)(y - size * 0.7f) / (size * 0.3f));
            for (int x = (int)(size * (0.5f - w)); x < (int)(size * (0.5f + w)); x++)
                if (x >= 0 && x < size) pixels[y * size + x] = hat;
        }

        // Robe body
        for (int y = (int)(size * 0.2f); y < (int)(size * 0.7f); y++)
            for (int x = (int)(size * 0.15f); x < (int)(size * 0.85f); x++)
                pixels[y * size + x] = robe;

        // Glowing eyes
        DrawCircle(pixels, size, (int)(size * 0.38f), (int)(size * 0.65f), 2, new Color(0.5f, 0.2f, 1f));
        DrawCircle(pixels, size, (int)(size * 0.58f), (int)(size * 0.65f), 2, new Color(0.5f, 0.2f, 1f));

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateGhost(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        float center = size / 2f;
        Color ghost = new Color(0.8f, 0.85f, 1f, 0.7f);

        // Ghost body
        for (int y = (int)(size * 0.2f); y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                if (dx * dx + dy * dy < 0.8f)
                    pixels[y * size + x] = ghost;
            }
        }

        // Wavy bottom
        for (int x = 0; x < size; x++)
        {
            float wave = Mathf.Sin(x * 0.8f) * 2f;
            int cutY = (int)(size * 0.2f + wave);
            for (int y = 0; y < cutY; y++)
                pixels[y * size + x] = clear;
        }

        // Eyes
        DrawCircle(pixels, size, (int)(center - size * 0.12f), (int)(center * 0.8f), 3, Color.black);
        DrawCircle(pixels, size, (int)(center + size * 0.12f), (int)(center * 0.8f), 3, Color.black);

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateBomber(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        float center = size / 2f;
        Color bomb = new Color(0.2f, 0.2f, 0.2f);

        // Round bomb body
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center * 0.7f) / (center * 0.7f);
                if (dx * dx + dy * dy < 0.9f)
                    pixels[y * size + x] = bomb;
            }

        // Fuse
        for (int i = 0; i < size / 4; i++)
        {
            int fx = (int)(center + i * 0.5f);
            int fy = size - i;
            if (fx >= 0 && fx < size && fy >= 0 && fy < size)
                pixels[fy * size + fx] = new Color(0.6f, 0.4f, 0.1f);
        }

        // Angry eyes
        DrawCircle(pixels, size, (int)(center - size * 0.13f), (int)(center * 0.7f), 2, Color.red);
        DrawCircle(pixels, size, (int)(center + size * 0.13f), (int)(center * 0.7f), 2, Color.red);

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateTank(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color armor = new Color(0.3f, 0.35f, 0.3f);

        // Large rectangular body
        for (int y = (int)(size * 0.15f); y < (int)(size * 0.85f); y++)
            for (int x = (int)(size * 0.05f); x < (int)(size * 0.95f); x++)
                pixels[y * size + x] = armor;

        // Armor plates
        for (int y = (int)(size * 0.3f); y < (int)(size * 0.7f); y += 4)
            for (int x = (int)(size * 0.1f); x < (int)(size * 0.9f); x++)
                pixels[y * size + x] = armor * 0.8f;

        // Eyes
        DrawCircle(pixels, size, (int)(size * 0.35f), (int)(size * 0.6f), 3, Color.yellow);
        DrawCircle(pixels, size, (int)(size * 0.6f), (int)(size * 0.6f), 3, Color.yellow);

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateFlyer(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        float center = size / 2f;
        Color wing = new Color(0.6f, 0.3f, 0.7f);

        // Body
        for (int y = (int)(size * 0.3f); y < (int)(size * 0.7f); y++)
            for (int x = (int)(size * 0.35f); x < (int)(size * 0.65f); x++)
                pixels[y * size + x] = wing * 1.2f;

        // Wings
        for (int y = (int)(size * 0.4f); y < (int)(size * 0.6f); y++)
        {
            for (int x = 0; x < (int)(size * 0.35f); x++)
                pixels[y * size + x] = wing;
            for (int x = (int)(size * 0.65f); x < size; x++)
                pixels[y * size + x] = wing;
        }

        // Eyes
        DrawCircle(pixels, size, (int)(center - size * 0.08f), (int)(size * 0.55f), 2, Color.cyan);
        DrawCircle(pixels, size, (int)(center + size * 0.08f), (int)(size * 0.55f), 2, Color.cyan);

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    static void DrawCircle(Color[] pixels, int size, int cx, int cy, int r, Color color)
    {
        for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
                if (x >= 0 && x < size && y >= 0 && y < size)
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                        pixels[y * size + x] = color;
    }

    public static Sprite CreateDropHealth(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color outline = new Color(0.15f, 0.05f, 0.05f);
        Color heart = new Color(0.9f, 0.15f, 0.15f);
        Color highlight = new Color(1f, 0.45f, 0.45f);

        float c = size / 2f;
        int h = (int)(size * 0.35f);
        int w = (int)(size * 0.38f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - c) / c;
                float ny = (y - c) / c;
                float ax = Mathf.Abs(nx);
                float ay = ny + 0.15f;

                bool inHeart = (ax * ax + ay * ay - 1f) * (ax * ax + ay * ay - 1f) * (ax * ax + ay * ay - 1f)
                               - ax * ax * ay * ay * ay < 0.02f;

                if (inHeart)
                {
                    bool isEdge = false;
                    for (int dy = -1; dy <= 1 && !isEdge; dy++)
                        for (int dx = -1; dx <= 1 && !isEdge; dx++)
                        {
                            float nx2 = (x + dx - c) / c;
                            float ny2 = (y + dy - c) / c;
                            float ax2 = Mathf.Abs(nx2);
                            float ay2 = ny2 + 0.15f;
                            bool neighbor = (ax2 * ax2 + ay2 * ay2 - 1f) * (ax2 * ax2 + ay2 * ay2 - 1f) * (ax2 * ax2 + ay2 * ay2 - 1f)
                                            - ax2 * ax2 * ay2 * ay2 * ay2 < 0.02f;
                            if (!neighbor) isEdge = true;
                        }

                    pixels[y * size + x] = isEdge ? outline : (ny > 0.1f ? highlight : heart);
                }
            }
        }

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateDropEnergy(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color outline = new Color(0.05f, 0.1f, 0.3f);
        Color bolt = new Color(0.2f, 0.5f, 1f);
        Color glow = new Color(0.5f, 0.8f, 1f);

        float c = size / 2f;
        int s = (int)(size * 0.8f);
        int start = (int)(c - s / 2f);

        int[,] boltShape = {
            {0,0,0,0,1,1,1,1,1,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0},
            {0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0},
            {0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0},
            {0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,0},
            {0,0,1,1,1,1,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
        };

        for (int y = 0; y < size && y < boltShape.GetLength(0); y++)
            for (int x = 0; x < size && x < boltShape.GetLength(1); x++)
            {
                if (boltShape[y, x] == 1)
                {
                    pixels[y * size + x] = outline;
                    if (x + 1 < size && boltShape[y, Mathf.Min(x + 1, boltShape.GetLength(1) - 1)] == 1)
                        pixels[y * size + x] = bolt;
                    if (y > 0 && boltShape[y - 1, x] == 1)
                        pixels[y * size + x] = bolt;
                    if (x < 3 && boltShape[y, x] == 1)
                        pixels[y * size + x] = glow;
                }
            }

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateDropWeapon(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color outline = new Color(0.15f, 0.1f, 0.05f);
        Color blade = new Color(0.75f, 0.78f, 0.82f);
        Color bladeEdge = new Color(0.55f, 0.58f, 0.62f);
        Color guard = new Color(0.85f, 0.65f, 0.15f);
        Color handle = new Color(0.45f, 0.25f, 0.1f);
        Color pommel = new Color(0.9f, 0.75f, 0.2f);

        int cx = size / 2;
        int cy = size / 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (float)(x - cx) / size;
                float ny = (float)(y - cy) / size;

                bool isBlade = (Mathf.Abs(nx) < 0.08f && ny > 0.05f && ny < 0.38f);
                bool isBladeEdge = isBlade && (Mathf.Abs(nx) > 0.04f);
                bool isGuard = (Mathf.Abs(ny - 0.05f) < 0.04f && Mathf.Abs(nx) < 0.2f);
                bool isHandle = (Mathf.Abs(nx) < 0.05f && ny > -0.25f && ny < 0.02f);
                bool isPommel = (Mathf.Abs(nx) < 0.07f && Mathf.Abs(ny + 0.25f) < 0.04f);

                if (isBladeEdge) pixels[y * size + x] = bladeEdge;
                else if (isBlade) pixels[y * size + x] = blade;
                else if (isGuard) pixels[y * size + x] = guard;
                else if (isHandle) pixels[y * size + x] = handle;
                else if (isPommel) pixels[y * size + x] = pommel;
            }
        }

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                if (pixels[y * size + x] == clear)
                {
                    bool hasNeighbor = false;
                    for (int dy = -1; dy <= 1 && !hasNeighbor; dy++)
                        for (int dx = -1; dx <= 1 && !hasNeighbor; dx++)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (nx >= 0 && nx < size && ny >= 0 && ny < size && pixels[ny * size + nx] != clear)
                                hasNeighbor = true;
                        }
                    if (hasNeighbor)
                    {
                        bool isEdgePixel = false;
                        for (int dy = -1; dy <= 1 && !isEdgePixel; dy++)
                            for (int dx = -1; dx <= 1 && !isEdgePixel; dx++)
                            {
                                int nx = x + dx, ny = y + dy;
                                if (nx >= 0 && nx < size && ny >= 0 && ny < size && pixels[ny * size + nx] == clear)
                                    isEdgePixel = true;
                            }
                        if (isEdgePixel)
                            pixels[y * size + x] = outline;
                    }
                }
            }

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateDropDamage(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color outline = new Color(0.3f, 0.02f, 0.02f);
        Color flame = new Color(0.95f, 0.25f, 0.1f);
        Color core = new Color(1f, 0.75f, 0.15f);

        float c = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - c) / c;
                float ny = (y - c) / c;

                float flameR = 0.35f + 0.15f * Mathf.Sin(ny * 6f + nx * 2f);
                float dist = Mathf.Sqrt(nx * nx + ny * ny);

                if (ny > -0.5f && ny < 0.7f && dist < flameR && Mathf.Abs(nx) < flameR)
                {
                    float tipFade = 1f - Mathf.Max(0, (ny - 0.2f) / 0.5f);
                    pixels[y * size + x] = tipFade > 0.7f ? core : flame;

                    bool isEdge = false;
                    for (int dy = -1; dy <= 1 && !isEdge; dy++)
                        for (int dx = -1; dx <= 1 && !isEdge; dx++)
                        {
                            float nx2 = (x + dx - c) / c;
                            float ny2 = (y + dy - c) / c;
                            float flameR2 = 0.35f + 0.15f * Mathf.Sin(ny2 * 6f + nx2 * 2f);
                            float dist2 = Mathf.Sqrt(nx2 * nx2 + ny2 * ny2);
                            bool neighbor = ny2 > -0.5f && ny2 < 0.7f && dist2 < flameR2 && Mathf.Abs(nx2) < flameR2;
                            if (!neighbor) isEdge = true;
                        }
                    if (isEdge) pixels[y * size + x] = outline;
                }
            }
        }

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateDropSpeed(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color outline = new Color(0.05f, 0.2f, 0.25f);
        Color wing = new Color(0.3f, 0.85f, 0.9f);
        Color highlight = new Color(0.65f, 1f, 1f);

        float c = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - c) / c;
                float ny = (y - c) / c;

                bool inWing = false;

                float wingY = 0.1f - Mathf.Abs(nx) * 0.4f;
                if (ny > wingY - 0.3f && ny < wingY + 0.15f && Mathf.Abs(nx) < 0.7f - ny * 0.3f)
                    inWing = true;

                if (ny > -0.15f && ny < 0.05f && Mathf.Abs(nx) < 0.15f)
                    inWing = true;

                if (ny > -0.5f && ny < -0.1f && Mathf.Abs(nx) < 0.1f + (ny + 0.3f) * 0.5f)
                    inWing = true;

                if (inWing)
                {
                    pixels[y * size + x] = wing;
                    if (ny > wingY) pixels[y * size + x] = highlight;

                    bool isEdge = false;
                    for (int dy = -1; dy <= 1 && !isEdge; dy++)
                        for (int dx = -1; dx <= 1 && !isEdge; dx++)
                        {
                            float nx2 = (x + dx - c) / c;
                            float ny2 = (y + dy - c) / c;
                            float wingY2 = 0.1f - Mathf.Abs(nx2) * 0.4f;
                            bool neighbor = false;
                            if (ny2 > wingY2 - 0.3f && ny2 < wingY2 + 0.15f && Mathf.Abs(nx2) < 0.7f - ny2 * 0.3f)
                                neighbor = true;
                            if (ny2 > -0.15f && ny2 < 0.05f && Mathf.Abs(nx2) < 0.15f)
                                neighbor = true;
                            if (ny2 > -0.5f && ny2 < -0.1f && Mathf.Abs(nx2) < 0.1f + (ny2 + 0.3f) * 0.5f)
                                neighbor = true;
                            if (!neighbor) isEdge = true;
                        }
                    if (isEdge) pixels[y * size + x] = outline;
                }
            }
        }

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateDropArmor(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color outline = new Color(0.05f, 0.1f, 0.25f);
        Color shield = new Color(0.25f, 0.55f, 0.9f);
        Color shieldDark = new Color(0.15f, 0.35f, 0.65f);
        Color shieldLight = new Color(0.5f, 0.75f, 1f);

        float c = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - c) / c;
                float ny = (y - c) / c;

                bool inShield = false;
                float topEdge = 0.35f;
                float botEdge = -0.35f;
                float w = 0.38f;

                if (ny > botEdge && ny < topEdge)
                {
                    float maxX = w * (1f - (ny - botEdge) / (topEdge - botEdge) * 0.5f);
                    if (Mathf.Abs(nx) < maxX)
                        inShield = true;
                }

                float tipY = botEdge - 0.05f;
                if (ny > tipY && ny < botEdge + 0.05f)
                {
                    float maxX = w * 0.5f * (1f - (ny - tipY) / (botEdge + 0.05f - tipY));
                    if (Mathf.Abs(nx) < maxX)
                        inShield = true;
                }

                if (inShield)
                {
                    pixels[y * size + x] = shield;
                    if (nx < -0.05f) pixels[y * size + x] = shieldDark;
                    if (nx > 0.05f && ny > 0.1f) pixels[y * size + x] = shieldLight;

                    bool isEdge = false;
                    for (int dy = -1; dy <= 1 && !isEdge; dy++)
                        for (int dx = -1; dx <= 1 && !isEdge; dx++)
                        {
                            float nx2 = (x + dx - c) / c;
                            float ny2 = (y + dy - c) / c;
                            bool neighbor = false;
                            float topEdge2 = 0.35f;
                            float botEdge2 = -0.35f;
                            float w2 = 0.38f;
                            if (ny2 > botEdge2 && ny2 < topEdge2)
                            {
                                float maxX2 = w2 * (1f - (ny2 - botEdge2) / (topEdge2 - botEdge2) * 0.5f);
                                if (Mathf.Abs(nx2) < maxX2) neighbor = true;
                            }
                            float tipY2 = botEdge2 - 0.05f;
                            if (ny2 > tipY2 && ny2 < botEdge2 + 0.05f)
                            {
                                float maxX2 = w2 * 0.5f * (1f - (ny2 - tipY2) / (botEdge2 + 0.05f - tipY2));
                                if (Mathf.Abs(nx2) < maxX2) neighbor = true;
                            }
                            if (!neighbor) isEdge = true;
                        }
                    if (isEdge) pixels[y * size + x] = outline;
                }
            }
        }

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateCoin(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color outline = new Color(0.4f, 0.3f, 0.0f);
        Color coin = new Color(1f, 0.85f, 0.1f);
        Color highlight = new Color(1f, 1f, 0.5f);
        Color shadow = new Color(0.7f, 0.55f, 0.05f);

        float c = size / 2f;
        float r = size * 0.38f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - c;
                float dy = y - c;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= r)
                {
                    if (dist > r - 1.5f)
                        pixels[y * size + x] = outline;
                    else if (dx < -r * 0.2f && dy > r * 0.1f)
                        pixels[y * size + x] = highlight;
                    else if (dx > r * 0.2f && dy < -r * 0.1f)
                        pixels[y * size + x] = shadow;
                    else
                        pixels[y * size + x] = coin;
                }
            }
        }

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateMerchant(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color robe = new Color(0.55f, 0.2f, 0.65f);
        Color robeDark = new Color(0.35f, 0.1f, 0.45f);
        Color hat = new Color(0.45f, 0.15f, 0.55f);
        Color skin = new Color(0.9f, 0.75f, 0.6f);
        Color eyes = new Color(0.1f, 0.8f, 0.3f);
        float c = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - c) / c;
                float ny = (y - c) / c;

                bool inHat = ny > 0.3f && ny < 0.9f && Mathf.Abs(nx) < 0.15f + (0.9f - ny) * 0.3f;
                bool inHead = Mathf.Abs(nx) < 0.25f && ny > 0.05f && ny < 0.35f;
                bool inBody = Mathf.Abs(nx) < 0.35f && ny > -0.55f && ny < 0.1f;
                bool inRobeBottom = ny > -0.9f && ny < -0.5f && Mathf.Abs(nx) < 0.4f + (ny + 0.9f) * 0.3f;

                if (inHat) pixels[y * size + x] = hat;
                if (inHead) pixels[y * size + x] = skin;
                if (inBody) pixels[y * size + x] = robe;
                if (inRobeBottom) pixels[y * size + x] = robeDark;

                if (inHead && Mathf.Abs(nx + 0.1f) < 0.06f && Mathf.Abs(ny - 0.2f) < 0.06f)
                    pixels[y * size + x] = eyes;
                if (inHead && Mathf.Abs(nx - 0.1f) < 0.06f && Mathf.Abs(ny - 0.2f) < 0.06f)
                    pixels[y * size + x] = eyes;

                if (inBody && Mathf.Abs(nx) < 0.08f && ny > -0.15f && ny < 0.05f)
                    pixels[y * size + x] = new Color(1f, 0.85f, 0.1f);
            }
        }

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateSpikeTrap(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color baseplate = new Color(0.4f, 0.38f, 0.35f, 0.8f);
        Color spike = new Color(0.75f, 0.75f, 0.78f);
        Color spikeTip = new Color(0.9f, 0.9f, 0.92f);
        Color outline = new Color(0.25f, 0.25f, 0.28f);

        float c = size / 2f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float nx = (x - c) / c;
                float ny = (y - c) / c;

                bool inBase = Mathf.Abs(nx) < 0.85f && Mathf.Abs(ny) < 0.85f;
                if (inBase)
                {
                    bool isEdge = Mathf.Abs(nx) > 0.75f || Mathf.Abs(ny) > 0.75f;
                    pixels[y * size + x] = isEdge ? outline : baseplate;
                }
            }

        float[,] spikes = {
            {0.35f, 0.35f}, {0.65f, 0.35f}, {0.35f, 0.65f}, {0.65f, 0.65f}, {0.5f, 0.5f}
        };

        for (int s = 0; s < spikes.GetLength(0); s++)
        {
            float sx = spikes[s, 0] * size;
            float sy = spikes[s, 1] * size;
            float spikeR = size * 0.12f;

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x - sx;
                    float dy = y - sy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist < spikeR)
                    {
                        float tipness = 1f - dist / spikeR;
                        pixels[y * size + x] = tipness > 0.6f ? spikeTip : spike;
                    }
                    else if (dist < spikeR + 1f)
                    {
                        if (pixels[y * size + x].a < 0.1f)
                            pixels[y * size + x] = outline;
                    }
                }
        }

        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
