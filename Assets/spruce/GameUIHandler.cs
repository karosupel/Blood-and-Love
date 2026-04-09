using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUIHandler : MonoBehaviour
{
    [Serializable]
    private class RoomTypeButtonBinding
    {
        public string roomTypeId;
        public Button button;
    }

    public PlayerHealth PlayerHealth;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private GameObject healthBarRoot;
    [SerializeField] private GameObject afterlifeHeart;
    [SerializeField] private TMP_Text heartText;
    [SerializeField] private GameObject borders;
    [SerializeField] private Color materialPlaneBorderColor = new Color32(14, 25, 17, 255);
    [SerializeField] private Color afterlifeBorderColor = Color.red;

    [Header("Room Choice UI")]
    [SerializeField] private GameObject roomChoiceRoot;
    [SerializeField] private TMP_Text roomChoicePromptText;
    [SerializeField] private List<RoomTypeButtonBinding> roomTypeButtons = new List<RoomTypeButtonBinding>();
    [SerializeField] private string bossRoomTypeId = "Boss";
    [SerializeField] private Vector3 leftButtonPosition = new Vector3(-500f, -40f, 0f);
    [SerializeField] private Vector3 middleButtonPosition = new Vector3(0f, -40f, 0f);
    [SerializeField] private Vector3 rightButtonPosition = new Vector3(500f, -40f, 0f);
    private Action<int> onRoomChoiceSelected;

    public bool IsChoosingRoom { get; private set; }

    private void Awake()
    {
        // Try auto-wiring text references from the same GameObject.
        if (healthText == null)
        {
            healthText = GetComponent<TMP_Text>();
        }

        // If not explicitly assigned, use the health text object as fallback root.
        if (healthBarRoot == null && healthText != null)
        {
            healthBarRoot = healthText.gameObject;
        }

        if (heartText == null && afterlifeHeart != null)
        {
            heartText = afterlifeHeart.GetComponentInChildren<TMP_Text>(true);
        }

        if (roomChoiceRoot != null)
        {
            roomChoiceRoot.SetActive(false);
        }

    }

    public void ShowRoomChoices(RoomChoiceOption[] options, Action<int> onSelected)
    {
        if (options == null || options.Length == 0)
        {
            return;
        }

        if (roomChoiceRoot == null)
        {
            Debug.LogWarning("Room choice UI root is not assigned on GameUIHandler.");
            return;
        }

        if (roomTypeButtons == null || roomTypeButtons.Count == 0)
        {
            Debug.LogWarning("Room type button bindings are not assigned on GameUIHandler.");
            return;
        }

        IsChoosingRoom = true;
        onRoomChoiceSelected = onSelected;

        if (roomChoicePromptText != null)
        {
            roomChoicePromptText.text = "Choose your next room";
        }

        const int maxVisibleChoices = 3;
        int shownOptionCount = Mathf.Min(options.Length, maxVisibleChoices);
        if (options.Length > maxVisibleChoices)
        {
            Debug.LogWarning("Room choice UI supports up to 3 visible choices. Extra options will be hidden.");
        }

        HideAllRoomTypeButtons();

        for (int i = 0; i < shownOptionCount; i++)
        {
            RoomChoiceOption option = options[i];
            if (option == null)
            {
                continue;
            }

            Button button = FindButtonForRoomType(option.optionId);
            if (button == null)
            {
                Debug.LogWarning("No UI button mapped for room type: " + option.optionId);
                continue;
            }

            int index = i;
            button.gameObject.SetActive(true);

            // Force boss choice to center when it is the only visible choice.
            int slotIndex = i;
            if (shownOptionCount == 1 &&
                !string.IsNullOrEmpty(option.optionId) &&
                string.Equals(option.optionId, bossRoomTypeId, StringComparison.OrdinalIgnoreCase))
            {
                slotIndex = 1;
            }

            SetButtonSlotPosition(button, slotIndex);

            Image image = button.GetComponent<Image>();
            TMP_Text buttonTmpText = button.GetComponentInChildren<TMP_Text>(true);
            Text buttonLegacyText = buttonTmpText == null ? button.GetComponentInChildren<Text>(true) : null;

            if (image != null)
            {
                image.color = options[i].color;
            }

            if (buttonTmpText != null)
            {
                buttonTmpText.text = options[i].optionId;
            }
            else if (buttonLegacyText != null)
            {
                buttonLegacyText.text = options[i].optionId;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => HandleRoomChoiceSelected(index));
        }

        roomChoiceRoot.SetActive(true);
    }

    private void HideAllRoomTypeButtons()
    {
        for (int i = 0; i < roomTypeButtons.Count; i++)
        {
            RoomTypeButtonBinding binding = roomTypeButtons[i];
            if (binding == null || binding.button == null)
            {
                continue;
            }

            binding.button.gameObject.SetActive(false);
        }
    }

    private Button FindButtonForRoomType(string roomTypeId)
    {
        for (int i = 0; i < roomTypeButtons.Count; i++)
        {
            RoomTypeButtonBinding binding = roomTypeButtons[i];
            if (binding == null || binding.button == null || string.IsNullOrEmpty(binding.roomTypeId))
            {
                continue;
            }

            if (string.Equals(binding.roomTypeId, roomTypeId, StringComparison.OrdinalIgnoreCase))
            {
                return binding.button;
            }
        }

        return null;
    }

    private void SetButtonSlotPosition(Button button, int slotIndex)
    {
        if (button == null)
        {
            return;
        }

        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect == null)
        {
            return;
        }

        Vector3 targetPosition = middleButtonPosition;
        if (slotIndex == 0)
        {
            targetPosition = leftButtonPosition;
        }
        else if (slotIndex == 2)
        {
            targetPosition = rightButtonPosition;
        }

        rect.anchoredPosition3D = targetPosition;
    }

    public void HideRoomChoices()
    {
        IsChoosingRoom = false;
        onRoomChoiceSelected = null;

        if (roomChoiceRoot != null)
        {
            roomChoiceRoot.SetActive(false);
        }
    }

    private void HandleRoomChoiceSelected(int optionIndex)
    {
        Action<int> callback = onRoomChoiceSelected;
        if (callback != null)
        {
            callback(optionIndex);
        }
    }

    private void OnEnable()
    {
        if (PlayerHealth != null)
        {
            PlayerHealth.OnHealthChanged += HealthChanged;
            PlayerHealth.OnAfterlifeStateChanged += AfterlifeStateChanged;
            PlayerHealth.OnHeartsChanged += HeartsChanged;
        }

        HealthChanged();
        HeartsChanged(PlayerHealth != null ? PlayerHealth.Hearts : 0);
        AfterlifeStateChanged(PlayerHealth != null && PlayerHealth.IsInAfterlife);
    }

    private void OnDisable()
    {
        if (PlayerHealth != null)
        {
            PlayerHealth.OnHealthChanged -= HealthChanged;
            PlayerHealth.OnAfterlifeStateChanged -= AfterlifeStateChanged;
            PlayerHealth.OnHeartsChanged -= HeartsChanged;
        }
    }


    private void HealthChanged()
    {
        if (PlayerHealth == null)
        {
            return;
        }

        string hpText = $"{Mathf.CeilToInt(PlayerHealth.CurrentHealth)}/{Mathf.CeilToInt(PlayerHealth.MaxHealth)}";

        if (healthText != null)
        {
            healthText.text = hpText;
        }
    }

    private void AfterlifeStateChanged(bool isInAfterlife)
    {
        SetUiVisible(healthBarRoot, !isInAfterlife);

        SetBorderChildrenColor(isInAfterlife ? afterlifeBorderColor : materialPlaneBorderColor);
        

        if (healthBarRoot == null && healthText != null)
        {
            SetUiVisible(healthText.gameObject, !isInAfterlife);
        }

        SetUiVisible(afterlifeHeart, isInAfterlife);
        if (heartText != null)
        {
            SetUiVisible(heartText.gameObject, isInAfterlife);
        }
    }

    private void HeartsChanged(int hearts)
    {
        if (heartText != null)
        {
            heartText.text = hearts.ToString();
        }
    }

    private void SetBorderChildrenColor(Color targetColor)
    {
        if (borders == null)
        {
            return;
        }

        Transform borderRoot = borders.transform;
        for (int i = 0; i < borderRoot.childCount; i++)
        {
            Transform child = borderRoot.GetChild(i);

            if (child.TryGetComponent<Image>(out Image image))
            {
                image.color = targetColor;
                continue;
            }

            if (child.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.color = targetColor;
            }
        }
    }

    private void SetUiVisible(GameObject target, bool isVisible)
    {
        if (target != null && target.activeSelf != isVisible)
        {
            target.SetActive(isVisible);
        }
    }
}