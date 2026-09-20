using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 8f;
    public Vector3 offset = new Vector3(0, 0, -10);

    // камера держится за центр комнаты и лишь слегка тянется за игроком
    public bool useFocus;
    public Vector2 focus;
    public float focusFollow = 0.3f;

    private float minX = -100f;
    private float maxX = 100f;
    private float minY = -100f;
    private float maxY = 100f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        if (useFocus)
        {
            desiredPosition.x = Mathf.Lerp(focus.x, target.position.x, focusFollow);
            desiredPosition.y = Mathf.Lerp(focus.y, target.position.y, focusFollow);
        }
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
