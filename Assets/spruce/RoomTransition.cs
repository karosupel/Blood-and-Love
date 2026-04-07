using UnityEngine;

public class RoomTransition : MonoBehaviour
{
    [SerializeField] string fromRoomTypeId = "Enter";
    [SerializeField] RoomDirection direction = RoomDirection.Up;
    [SerializeField] float fallbackTeleportDistance = 3f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (RoomManager.Instance == null)
        {
            Debug.LogError("RoomManager is missing from the scene.");
            return;
        }

        RoomManager.Instance.RequestTransition(
            other.gameObject,
            fromRoomTypeId,
            direction,
            fallbackTeleportDistance
        );
    }
}
