using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class RoomTransition : MonoBehaviour
{
    [SerializeField] string fromRoomTypeId = "Enter";
    [SerializeField] RoomDirection direction = RoomDirection.Up;
    [SerializeField] float fallbackTeleportDistance = 3f;

    [SerializeField] GameObject spawnerManager;
    private SpawnerManagerScript spawnerManagerScript;

    [SerializeField] public List<GameObject> ActiveEnemiesInScene = new List<GameObject>();

    private void Awake()
    {
        spawnerManager = GameObject.FindWithTag("SpawnerManager");
        spawnerManagerScript = spawnerManager.GetComponent<SpawnerManagerScript>();
    }

    private void Update()
    {
        ActiveEnemiesInScene = spawnerManagerScript.GetActiveEnemiesInScene();

        if (!ActiveEnemiesInScene.Any())
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
        }
        else if (ActiveEnemiesInScene.Any())
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

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

        string resolvedFromRoomTypeId = fromRoomTypeId;
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            resolvedFromRoomTypeId = GetRoomVariantId(fromRoomTypeId, playerHealth.IsInAfterlife);
        }

        RoomManager.Instance.RequestTransition(
            other.gameObject,
            resolvedFromRoomTypeId,
            direction,
            fallbackTeleportDistance
        );
    }

    string GetRoomVariantId(string roomTypeId, bool useHellVariant)
    {
        if (string.IsNullOrEmpty(roomTypeId))
        {
            return roomTypeId;
        }

        bool isHellId = roomTypeId.EndsWith("H", StringComparison.OrdinalIgnoreCase);
        if (useHellVariant)
        {
            return isHellId ? roomTypeId : roomTypeId + "H";
        }

        if (!isHellId)
        {
            return roomTypeId;
        }

        return roomTypeId.Substring(0, roomTypeId.Length - 1);
    }

}
