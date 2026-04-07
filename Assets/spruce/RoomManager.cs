using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    [SerializeField] float arrivalOffset = 0.75f;
    [SerializeField] List<RoomInstance> roomInstances = new List<RoomInstance>();
    [SerializeField] List<RoomConnection> connections = new List<RoomConnection>();
    [SerializeField] List<string> roomVisitStack = new List<string>();

    Canvas choiceCanvas;
    Text promptText;
    readonly List<Button> optionButtons = new List<Button>();

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
        CreateChoiceUI();
        EnsureEventSystem();
    }

    public void RequestTransition(GameObject player, string fromRoomTypeId, RoomDirection direction, float fallbackDistance)
    {
        if (isChoosing || player == null)
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

        RoomChoiceOption[] options = BuildRandomRoomOptions(choicesPerTransition);
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

    RoomChoiceOption FindAvailableRoomById(string roomId)
    {
        for (int i = 0; i < availableRooms.Count; i++)
        {
            RoomChoiceOption room = availableRooms[i];
            if (room != null && room.optionId == roomId)
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
        isChoosing = true;
        TogglePlayerControls(pendingTransition.player, false);

        promptText.text = "Choose next room";
        int optionCount = pendingTransition.options.Length;
        for (int i = 0; i < optionButtons.Count; i++)
        {
            bool active = i < optionCount && pendingTransition.options[i] != null;
            optionButtons[i].gameObject.SetActive(active);
            if (!active)
            {
                continue;
            }

            int index = i;
            Button button = optionButtons[i];
            Image image = button.GetComponent<Image>();
            Text buttonText = button.GetComponentInChildren<Text>();

            image.color = pendingTransition.options[i].color;
            buttonText.text = pendingTransition.options[i].optionId;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectOption(index));
        }

        choiceCanvas.gameObject.SetActive(true);
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
        choiceCanvas.gameObject.SetActive(false);
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

    void CreateChoiceUI()
    {
        GameObject canvasObject = new GameObject("RoomChoiceCanvas");
        choiceCanvas = canvasObject.AddComponent<Canvas>();
        choiceCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject choicePanel = new GameObject("Panel");
        choicePanel.transform.SetParent(canvasObject.transform, false);
        Image panelImage = choicePanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.6f);

        RectTransform panelRect = choicePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.2f, 0.35f);
        panelRect.anchorMax = new Vector2(0.8f, 0.65f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject promptObject = CreateTextObject("Prompt", choicePanel.transform, 30, TextAnchor.UpperCenter);
        promptText = promptObject.GetComponent<Text>();
        RectTransform promptRect = promptObject.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.1f, 0.6f);
        promptRect.anchorMax = new Vector2(0.9f, 0.95f);
        promptRect.offsetMin = Vector2.zero;
        promptRect.offsetMax = Vector2.zero;

        for (int i = 0; i < 3; i++)
        {
            Button button = CreateOptionButton(choicePanel.transform, i);
            optionButtons.Add(button);
        }

        choiceCanvas.gameObject.SetActive(false);
    }

    Button CreateOptionButton(Transform parent, int index)
    {
        GameObject buttonObject = new GameObject("Option" + (index + 1));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = Color.white;

        Button button = buttonObject.AddComponent<Button>();
        RectTransform rect = buttonObject.GetComponent<RectTransform>();

        float width = 0.22f;
        float xPadding = 0.08f;
        float startX = xPadding + index * (width + xPadding);

        rect.anchorMin = new Vector2(startX, 0.08f);
        rect.anchorMax = new Vector2(startX + width, 0.53f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GameObject textObject = CreateTextObject("Label", buttonObject.transform, 20, TextAnchor.MiddleCenter);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    GameObject CreateTextObject(string name, Transform parent, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;

        return textObject;
    }

    void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }
}
