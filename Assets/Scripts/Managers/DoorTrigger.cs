using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Vector2Int direction;
    private bool triggered;

    void OnTriggerStay2D(Collider2D other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            RoomManager rm = GetComponentInParent<RoomManager>();
            if (rm != null && rm.roomData.cleared)
            {
                triggered = true;
                DungeonGenerator.Instance.EnterRoom(direction);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            triggered = false;
    }
}
