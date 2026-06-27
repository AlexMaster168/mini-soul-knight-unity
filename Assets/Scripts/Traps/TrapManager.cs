using UnityEngine;

public class TrapManager : MonoBehaviour
{
    public static TrapManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnRoomTraps(Vector3 roomCenter, float roomW, float roomH, bool isBossRoom)
    {
        int trapCount = Random.Range(0, 2);
        if (isBossRoom) trapCount = 0;

        float hw = roomW / 2f - 2f;
        float hh = roomH / 2f - 2f;

        for (int i = 0; i < trapCount; i++)
        {
            Vector3 pos = roomCenter + new Vector3(
                Random.Range(-hw, hw),
                Random.Range(-hh, hh),
                0
            );

            SpawnSpikeTrap(pos);
        }
    }

    void SpawnSpikeTrap(Vector3 pos)
    {
        GameObject trap = new GameObject("SpikeTrap");
        trap.transform.position = pos;

        SpriteRenderer sr = trap.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateSpikeTrap(16);
        sr.sortingOrder = 1;

        trap.transform.localScale = Vector3.one * 0.8f;
        trap.AddComponent<SpikeTrap>();
    }
}
