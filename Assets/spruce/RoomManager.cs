using System.Collections.Generic;
using UnityEngine;

public enum RoomDirection { Up, Down, Left, Right }

[System.Serializable]
public class RoomChoiceOption
{
    public string optionId = "Red";
    public Color color = Color.red;
    public PolygonCollider2D targetRoomBoundary;
    public Transform targetSpawnPoint;
}

[System.Serializable]
public class RoomConnection
{
    public string fromRoomInstanceId;
    public string fromRoomTypeId;
    public RoomDirection direction;
    public string toRoomInstanceId;
    public string toRoomTypeId;
    public bool hasTargetPosition;
    public Vector3 targetPosition;
    public PolygonCollider2D targetRoomBoundary;
}

[System.Serializable]
public class RoomInstance
{
    public string instanceId;
    public string roomTypeId;
}

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [SerializeField] List<RoomChoiceOption> availableRooms = new List<RoomChoiceOption>();
    [SerializeField] int choicesPerTransition = 3;
    [SerializeField] string bossRoomOptionId = "Boss";
    [SerializeField] float arrivalOffset = 0.75f;
    [SerializeField] List<RoomInstance> roomInstances = new List<RoomInstance>();
    [SerializeField] List<RoomConnection> connections = new List<RoomConnection>();
    [SerializeField] public List<string> roomVisitStack = new List<string>();

    GameUIHandler gameUIHandler;

    PendingTransition pendingTransition;
    bool isChoosing;
    string currentRoomInstanceId;
    int nextRoomInstanceNumber = 1;

    struct PendingTransition
    {
        public GameObject player;
        public string fromRoomInstanceId;
        public string fromRoomTypeId;
        public RoomDirection direction;
        public RoomChoiceOption[] options;
        public float fallbackDistance;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameUIHandler = FindObjectOfType<GameUIHandler>();
        if (gameUIHandler == null)
        {
            Debug.LogWarning("RoomManager could not find GameUIHandler. Room choice UI will not be shown.");
        }
    }

    public void RequestTransition(GameObject player, string fromRoomTypeId, RoomDirection direction, float fallbackDistance)
    {
        if (player == null)
        {
            return;
        }

        if (isChoosing || (gameUIHandler != null && gameUIHandler.IsChoosingRoom))
        {
            return;
        }

        string fromRoomInstanceId = EnsureCurrentRoomInstance(fromRoomTypeId);

        // Down transitions are strictly backtracking and never generate new choices.
        if (direction == RoomDirection.Down)
        {
            RoomConnection downConnection = FindConnection(fromRoomInstanceId, RoomDirection.Down);
            if (downConnection != null)
            {
                currentRoomInstanceId = downConnection.toRoomInstanceId;
                PopCurrentFromVisitStack(fromRoomInstanceId);
                ApplyConnectionTransition(player, downConnection, direction, fallbackDistance);
                return;
            }

            if (TryMoveToPreviousRoom(player, fallbackDistance))
            {
                return;
            }

            Debug.LogWarning("No previous room found in visit stack for this Down transition.");
            return;
        }

        RoomConnection existing = FindConnection(fromRoomInstanceId, direction);
        if (existing != null)
        {
            currentRoomInstanceId = existing.toRoomInstanceId;
            PushVisitedRoom(existing.toRoomInstanceId);
            ApplyConnectionTransition(player, existing, direction, fallbackDistance);
            return;
        }

        if (availableRooms.Count == 0)
        {
            Debug.LogWarning("RoomManager has no available rooms configured.");
            return;
        }

        RoomChoiceOption[] options = BuildTransitionOptions();
        if (options.Length == 0)
        {
            Debug.LogWarning("No valid destination rooms available for transition.");
            return;
        }

        pendingTransition = new PendingTransition
        {
            player = player,
            fromRoomInstanceId = fromRoomInstanceId,
            fromRoomTypeId = fromRoomTypeId,
            direction = direction,
            options = options,
            fallbackDistance = fallbackDistance
        };

        ShowChoices();
    }

    public IReadOnlyList<RoomConnection> GetConnections()
    {
        return connections;
    }

    public void SetConfinerForCurrentRoomVariant(bool useHellVariant)
    {
        Cinemachine.CinemachineConfiner confiner = FindObjectOfType<Cinemachine.CinemachineConfiner>();
        if (confiner == null)
        {
            Debug.LogWarning("Could not set confiner bounds because no CinemachineConfiner was found.");
            return;
        }

        PolygonCollider2D currentBoundary = confiner.m_BoundingShape2D as PolygonCollider2D;
        if (currentBoundary == null)
        {
            Debug.LogWarning("Could not set confiner bounds because current confiner boundary is not a PolygonCollider2D.");
            return;
        }

        string currentName = currentBoundary.gameObject.name;
        if (string.IsNullOrEmpty(currentName))
        {
            Debug.LogWarning("Could not set confiner bounds because current boundary has no name.");
            return;
        }

        string targetName = GetRoomVariantId(currentName, useHellVariant);
        PolygonCollider2D[] allBoundaries = FindObjectsOfType<PolygonCollider2D>(true);
        PolygonCollider2D targetBoundary = null;
        for (int i = 0; i < allBoundaries.Length; i++)
        {
            PolygonCollider2D candidate = allBoundaries[i];
            if (candidate == null)
            {
                continue;
            }

            if (!candidate.gameObject.scene.IsValid())
            {
                continue;
            }

            if (string.Equals(candidate.gameObject.name, targetName, System.StringComparison.OrdinalIgnoreCase))
            {
                targetBoundary = candidate;
                break;
            }
        }

        if (targetBoundary == null)
        {
            Debug.LogWarning("Could not set confiner bounds. Missing boundary object: " + targetName);
            return;
        }

        confiner.m_BoundingShape2D = targetBoundary;
        confiner.InvalidatePathCache();
    }

    RoomConnection FindConnection(string fromRoomInstanceId, RoomDirection direction)
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].fromRoomInstanceId == fromRoomInstanceId && connections[i].direction == direction)
            {
                return connections[i];
            }
        }

        return null;
    }

    string GetCurrentRoomTypeId()
    {
        if (string.IsNullOrEmpty(currentRoomInstanceId))
        {
            return null;
        }

        RoomInstance currentRoom = FindRoomInstance(currentRoomInstanceId);
        if (currentRoom == null)
        {
            return null;
        }

        return currentRoom.roomTypeId;
    }

    string GetRoomVariantId(string roomTypeId, bool useHellVariant)
    {
        bool isHellId = roomTypeId.EndsWith("H");
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

    RoomChoiceOption FindAvailableRoomById(string roomId)
    {
        for (int i = 0; i < availableRooms.Count; i++)
        {
            RoomChoiceOption room = availableRooms[i];
            if (room != null && string.Equals(room.optionId, roomId, System.StringComparison.OrdinalIgnoreCase))
            {
                return room;
            }
        }

        return null;
    }

    RoomChoiceOption[] BuildRandomRoomOptions(int count)
    {
        List<RoomChoiceOption> candidates = new List<RoomChoiceOption>();
        HashSet<string> usedIds = new HashSet<string>();

        for (int i = 0; i < availableRooms.Count; i++)
        {
            RoomChoiceOption room = availableRooms[i];
            if (room == null || string.IsNullOrEmpty(room.optionId))
            {
                continue;
            }

            if (string.Equals(room.optionId, bossRoomOptionId, System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!usedIds.Add(room.optionId))
            {
                continue;
            }

            candidates.Add(room);
        }

        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            RoomChoiceOption temp = candidates[i];
            candidates[i] = candidates[swapIndex];
            candidates[swapIndex] = temp;
        }

        int resultCount = Mathf.Min(Mathf.Max(1, count), candidates.Count);
        RoomChoiceOption[] result = new RoomChoiceOption[resultCount];
        for (int i = 0; i < resultCount; i++)
        {
            result[i] = candidates[i];
        }

        return result;
    }

    RoomChoiceOption[] BuildTransitionOptions()
    {
        if (roomVisitStack.Count >= 7)
        {
            RoomChoiceOption bossRoom = FindAvailableRoomById(bossRoomOptionId);
            if (bossRoom == null)
            {
                Debug.LogWarning("Boss room option is not configured or missing from available rooms.");
                return new RoomChoiceOption[0];
            }

            return new[] { bossRoom };
        }

        return BuildRandomRoomOptions(choicesPerTransition);
    }

    RoomInstance FindRoomInstance(string instanceId)
    {
        for (int i = 0; i < roomInstances.Count; i++)
        {
            if (roomInstances[i].instanceId == instanceId)
            {
                return roomInstances[i];
            }
        }

        return null;
    }

    string EnsureCurrentRoomInstance(string currentRoomTypeId)
    {
        if (!string.IsNullOrEmpty(currentRoomInstanceId) && FindRoomInstance(currentRoomInstanceId) != null)
        {
            if (roomVisitStack.Count == 0)
            {
                roomVisitStack.Add(currentRoomInstanceId);
            }

            return currentRoomInstanceId;
        }

        RoomInstance startingRoom = new RoomInstance
        {
            instanceId = string.Concat(currentRoomTypeId, "_0"),
            roomTypeId = currentRoomTypeId
        };

        roomInstances.Add(startingRoom);
        currentRoomInstanceId = startingRoom.instanceId;

        roomVisitStack.Clear();
        roomVisitStack.Add(currentRoomInstanceId);

        return currentRoomInstanceId;
    }

    RoomInstance CreateRoomInstance(string roomTypeId)
    {
        RoomInstance instance = new RoomInstance
        {
            instanceId = string.Concat(roomTypeId, "_", nextRoomInstanceNumber),
            roomTypeId = roomTypeId
        };

        nextRoomInstanceNumber++;
        roomInstances.Add(instance);
        return instance;
    }

    bool TryMoveToPreviousRoom(GameObject player, float fallbackDistance)
    {
        if (roomVisitStack.Count == 0)
        {
            return false;
        }

        if (roomVisitStack[roomVisitStack.Count - 1] != currentRoomInstanceId)
        {
            int index = roomVisitStack.LastIndexOf(currentRoomInstanceId);
            if (index >= 0)
            {
                roomVisitStack.RemoveRange(index + 1, roomVisitStack.Count - index - 1);
            }
            else
            {
                roomVisitStack.Add(currentRoomInstanceId);
            }
        }

        if (roomVisitStack.Count < 2)
        {
            return false;
        }

        roomVisitStack.RemoveAt(roomVisitStack.Count - 1);
        string previousInstanceId = roomVisitStack[roomVisitStack.Count - 1];
        currentRoomInstanceId = previousInstanceId;

        RoomInstance previousInstance = FindRoomInstance(previousInstanceId);
        RoomChoiceOption previousRoomOption = null;
        if (previousInstance != null)
        {
            previousRoomOption = FindAvailableRoomById(previousInstance.roomTypeId);
        }

        ApplyTransition(player, RoomDirection.Down, previousRoomOption, fallbackDistance);
        return true;
    }

    void PushVisitedRoom(string roomInstanceId)
    {
        if (string.IsNullOrEmpty(roomInstanceId))
        {
            return;
        }

        if (roomVisitStack.Count > 0 && roomVisitStack[roomVisitStack.Count - 1] == roomInstanceId)
        {
            return;
        }

        roomVisitStack.Add(roomInstanceId);
    }

    void PopCurrentFromVisitStack(string currentInstanceId)
    {
        if (roomVisitStack.Count == 0)
        {
            return;
        }

        if (roomVisitStack[roomVisitStack.Count - 1] == currentInstanceId)
        {
            roomVisitStack.RemoveAt(roomVisitStack.Count - 1);
        }
    }

    void ShowChoices()
    {
        if (gameUIHandler == null)
        {
            Debug.LogWarning("Cannot show room choices because GameUIHandler is missing.");
            return;
        }

        isChoosing = true;
        TogglePlayerControls(pendingTransition.player, false);

        gameUIHandler.ShowRoomChoices(pendingTransition.options, SelectOption);
    }

    void SelectOption(int optionIndex)
    {
        if (optionIndex < 0 || optionIndex >= pendingTransition.options.Length)
        {
            return;
        }

        RoomChoiceOption option = pendingTransition.options[optionIndex];
        if (option == null)
        {
            return;
        }

        Cinemachine.CinemachineConfiner confiner = FindObjectOfType<Cinemachine.CinemachineConfiner>();
        PolygonCollider2D sourceBoundary = null;
        if (confiner != null)
        {
            sourceBoundary = confiner.m_BoundingShape2D as PolygonCollider2D;
        }

        Vector3 sourcePosition = pendingTransition.player.transform.position;
        Vector3 destinationPosition = sourcePosition;
        if (option.targetSpawnPoint != null)
        {
            destinationPosition = AddArrivalOffset(option.targetSpawnPoint.position, pendingTransition.direction);
        }
        else
        {
            destinationPosition = AddArrivalOffset(
                GetFallbackPosition(sourcePosition, pendingTransition.direction, pendingTransition.fallbackDistance),
                pendingTransition.direction
            );
        }

        RoomDirection reverseDirection = GetOppositeDirection(pendingTransition.direction);
        Vector3 reverseTargetPosition = AddArrivalOffset(sourcePosition, reverseDirection);

        RoomInstance destinationRoom = CreateRoomInstance(option.optionId);
        SaveConnection(
            pendingTransition.fromRoomInstanceId,
            pendingTransition.fromRoomTypeId,
            pendingTransition.direction,
            destinationRoom.instanceId,
            destinationRoom.roomTypeId,
            destinationPosition,
            option.targetRoomBoundary
        );
        SaveConnection(
            destinationRoom.instanceId,
            destinationRoom.roomTypeId,
            reverseDirection,
            pendingTransition.fromRoomInstanceId,
            pendingTransition.fromRoomTypeId,
            reverseTargetPosition,
            sourceBoundary
        );

        currentRoomInstanceId = destinationRoom.instanceId;
        PushVisitedRoom(destinationRoom.instanceId);

        ApplyTransition(pendingTransition.player, pendingTransition.direction, option, pendingTransition.fallbackDistance);
        CloseChoices();
    }

    void SaveConnection(
        string fromRoomInstanceId,
        string fromRoomTypeId,
        RoomDirection direction,
        string toRoomInstanceId,
        string toRoomTypeId,
        Vector3 targetPosition,
        PolygonCollider2D targetBoundary
    )
    {
        RoomConnection existing = FindConnection(fromRoomInstanceId, direction);
        if (existing != null)
        {
            existing.fromRoomTypeId = fromRoomTypeId;
            existing.toRoomInstanceId = toRoomInstanceId;
            existing.toRoomTypeId = toRoomTypeId;
            existing.hasTargetPosition = true;
            existing.targetPosition = targetPosition;
            existing.targetRoomBoundary = targetBoundary;
            return;
        }

        connections.Add(new RoomConnection
        {
            fromRoomInstanceId = fromRoomInstanceId,
            fromRoomTypeId = fromRoomTypeId,
            direction = direction,
            toRoomInstanceId = toRoomInstanceId,
            toRoomTypeId = toRoomTypeId,
            hasTargetPosition = true,
            targetPosition = targetPosition,
            targetRoomBoundary = targetBoundary
        });
    }

    void ApplyConnectionTransition(GameObject player, RoomConnection connection, RoomDirection direction, float fallbackDistance)
    {
        if (player == null || connection == null)
        {
            return;
        }

        Cinemachine.CinemachineConfiner confiner = FindObjectOfType<Cinemachine.CinemachineConfiner>();
        if (confiner != null && connection.targetRoomBoundary != null)
        {
            confiner.m_BoundingShape2D = connection.targetRoomBoundary;
        }

        if (connection.hasTargetPosition)
        {
            player.transform.position = connection.targetPosition;
            return;
        }

        RoomChoiceOption targetOption = FindAvailableRoomById(connection.toRoomTypeId);
        ApplyTransition(player, direction, targetOption, fallbackDistance);
    }

    Vector3 GetFallbackPosition(Vector3 sourcePosition, RoomDirection direction, float fallbackDistance)
    {
        Vector3 position = sourcePosition;
        switch (direction)
        {
            case RoomDirection.Up:
                position.y += fallbackDistance;
                break;
            case RoomDirection.Down:
                position.y -= fallbackDistance;
                break;
            case RoomDirection.Left:
                position.x -= fallbackDistance;
                break;
            case RoomDirection.Right:
                position.x += fallbackDistance;
                break;
        }

        return position;
    }

    Vector3 AddArrivalOffset(Vector3 position, RoomDirection direction)
    {
        float offset = Mathf.Max(0f, arrivalOffset);
        switch (direction)
        {
            case RoomDirection.Up:
                position.y += offset;
                break;
            case RoomDirection.Down:
                position.y -= offset;
                break;
            case RoomDirection.Left:
                position.x -= offset;
                break;
            case RoomDirection.Right:
                position.x += offset;
                break;
        }

        return position;
    }

    RoomDirection GetOppositeDirection(RoomDirection direction)
    {
        switch (direction)
        {
            case RoomDirection.Up:
                return RoomDirection.Down;
            case RoomDirection.Down:
                return RoomDirection.Up;
            case RoomDirection.Left:
                return RoomDirection.Right;
            case RoomDirection.Right:
                return RoomDirection.Left;
            default:
                return direction;
        }
    }

    void ApplyTransition(GameObject player, RoomDirection direction, RoomChoiceOption option, float fallbackDistance)
    {
        if (player == null)
        {
            return;
        }

        Cinemachine.CinemachineConfiner confiner = FindObjectOfType<Cinemachine.CinemachineConfiner>();
        if (confiner != null && option != null && option.targetRoomBoundary != null)
        {
            confiner.m_BoundingShape2D = option.targetRoomBoundary;
        }

        if (option != null && option.targetSpawnPoint != null)
        {
            player.transform.position = AddArrivalOffset(option.targetSpawnPoint.position, direction);
            return;
        }

        Vector3 position = AddArrivalOffset(
            GetFallbackPosition(player.transform.position, direction, fallbackDistance),
            direction
        );

        player.transform.position = position;
    }

    void CloseChoices()
    {
        isChoosing = false;
        if (gameUIHandler != null)
        {
            gameUIHandler.HideRoomChoices();
        }

        TogglePlayerControls(pendingTransition.player, true);
    }

    void TogglePlayerControls(GameObject player, bool enabledState)
    {
        if (player == null)
        {
            return;
        }

        MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
        for (int i = 0; i < scripts.Length; i++)
        {
            if (scripts[i] == null)
            {
                continue;
            }

            if (scripts[i].GetType().Name == "PlayerController")
            {
                scripts[i].enabled = enabledState;
            }
        }
    }

}
