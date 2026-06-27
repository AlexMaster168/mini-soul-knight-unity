using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public static RoomGenerator Instance;

    void Awake()
    {
        Instance = this;
    }
}
