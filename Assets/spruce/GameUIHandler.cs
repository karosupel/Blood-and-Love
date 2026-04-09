using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUIHandler : MonoBehaviour
{
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
    [SerializeField] private List<Button> roomChoiceButtons = new List<Button>();
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

        if (roomChoiceButtons == null || roomChoiceButtons.Count == 0)
        {
            Debug.LogWarning("Room choice buttons are not assigned on GameUIHandler.");
            return;
        }

        IsChoosingRoom = true;
        onRoomChoiceSelected = onSelected;

        if (roomChoicePromptText != null)
        {
            roomChoicePromptText.text = "Choose next room";
        }

        int shownOptionCount = Mathf.Min(options.Length, roomChoiceButtons.Count);
        if (options.Length > roomChoiceButtons.Count)
        {
            Debug.LogWarning("Not enough room choice buttons assigned for all options. Extra options will be hidden.");
        }

        for (int i = 0; i < roomChoiceButtons.Count; i++)
        {
            Button button = roomChoiceButtons[i];
            if (button == null)
            {
                continue;
            }

            bool active = i < shownOptionCount && options[i] != null;
            button.gameObject.SetActive(active);
            if (!active)
            {
                continue;
            }

            int index = i;
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