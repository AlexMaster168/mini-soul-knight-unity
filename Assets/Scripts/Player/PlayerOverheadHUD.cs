using UnityEngine;

// Над головой игрока: значки здоровья, энергии и брони с полосками и числами
public class PlayerOverheadHUD : MonoBehaviour
{
    private class Row
    {
        public Transform fill;
        public TextMesh label;
    }

    private Row health, energy, armor;
    private const float BarW = 0.7f;

    void Start()
    {
        health = MakeRow(0, SpriteGenerator.CreateDropHealth(16), new Color(0.95f, 0.25f, 0.3f));
        energy = MakeRow(1, SpriteGenerator.CreateDropEnergy(16), new Color(0.3f, 0.6f, 1f));
        armor = MakeRow(2, SpriteGenerator.CreateDropArmor(16), new Color(0.65f, 0.85f, 1f));
    }

    Row MakeRow(int index, Sprite icon, Color color)
    {
        float y = 1.22f - index * 0.17f;
        Transform root = new GameObject("OverheadRow" + index).transform;
        root.SetParent(transform, false);
        root.localPosition = new Vector3(0, y, 0);

        SpriteRenderer isr = new GameObject("Icon").AddComponent<SpriteRenderer>();
        isr.transform.SetParent(root, false);
        isr.transform.localPosition = new Vector3(-0.5f, 0, 0);
        isr.transform.localScale = Vector3.one * 0.28f;
        isr.sprite = icon;
        isr.sortingOrder = 32;

        Sprite sq = SpriteGenerator.CreateSquare(4, Color.white);

        SpriteRenderer bg = new GameObject("Bg").AddComponent<SpriteRenderer>();
        bg.transform.SetParent(root, false);
        bg.transform.localPosition = new Vector3(-0.05f, 0, 0);
        bg.transform.localScale = new Vector3(BarW + 0.04f, 0.1f, 1f);
        bg.sprite = sq;
        bg.color = new Color(0f, 0f, 0f, 0.7f);
        bg.sortingOrder = 30;

        SpriteRenderer fill = new GameObject("Fill").AddComponent<SpriteRenderer>();
        fill.transform.SetParent(root, false);
        fill.sprite = sq;
        fill.color = color;
        fill.sortingOrder = 31;

        TextMesh tm = new GameObject("Num").AddComponent<TextMesh>();
        tm.transform.SetParent(root, false);
        tm.transform.localPosition = new Vector3(0.38f, 0, 0);
        tm.characterSize = 0.02f;
        tm.fontSize = 64;
        tm.anchor = TextAnchor.MiddleLeft;
        tm.color = Color.white;
        tm.fontStyle = FontStyle.Bold;
        tm.GetComponent<MeshRenderer>().sortingOrder = 33;

        return new Row { fill = fill.transform, label = tm };
    }

    void Set(Row r, float value, float max)
    {
        float k = max > 0f ? Mathf.Clamp01(value / max) : 0f;
        r.fill.localScale = new Vector3(BarW * k, 0.07f, 1f);
        r.fill.localPosition = new Vector3(-0.05f - BarW * (1f - k) * 0.5f, 0, 0);
        r.label.text = Mathf.CeilToInt(value).ToString();
    }

    void LateUpdate()
    {
        PlayerController pc = PlayerController.Instance;
        if (pc == null || health == null) return;
        Set(health, pc.currentHealth, pc.maxHealth);
        Set(energy, pc.currentEnergy, pc.maxEnergy);
        Set(armor, pc.armor, pc.maxArmor);
    }
}
