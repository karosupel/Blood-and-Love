using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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

    [Header("Help UI")]
    [SerializeField] private Button HelpButton;
    [SerializeField] private GameObject HelpCanvas;
    [SerializeField] private Button HelpCanvasButton;
    [SerializeField] private GameObject HelpCanvasHell;
    [SerializeField] private Button HelpCanvasHellButton;
    [SerializeField] private Vector4 helpButtonRaycastPadding = new Vector4(24f, 24f, 24f, 24f);
    [SerializeField] private float helpHideDelaySeconds = 0.08f;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverRoot;
    [SerializeField] private Button gameOverMainMenuButton;
    [SerializeField] private int mainMenuBuildIndex = 0;
    [SerializeField] private bool freezeTimeOnGameOver = true;
    [SerializeField] private float gameOverFadeDuration = 0.3f;
    [SerializeField] private float gameOverScaleDuration = 0.22f;
    [SerializeField] private float gameOverStartScale = 0.9f;
    [SerializeField] private AudioClip gameOverMusicClip;
    [Range(0f, 1f)]
    [SerializeField] private float gameOverMusicVolume = 1f;
    [SerializeField] private bool loopGameOverMusic = false;


    private Action<int> onRoomChoiceSelected;
    private int helpHoverCounter;
    private Coroutine helpHideCoroutine;
    private bool isGameOverShown;
    private float previousTimeScale = 1f;
    private bool previousAudioPaused;
    private Coroutine gameOverAnimationCoroutine;
    private AudioSource gameOverAudioSource;

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

        if (HelpCanvas != null)
        {
            HelpCanvas.SetActive(false);
        }

        if (HelpCanvasHell != null)
        {
            HelpCanvasHell.SetActive(false);
        }

        if (gameOverRoot != null)
        {
            gameOverRoot.SetActive(false);
        }

        EnsureGameOverAudioSource();
        BindGameOverButton();

        ConfigureHelpButtonHitZone();
        ConfigureHelpButtonHover();
        ConfigureHelpCanvasButtonHover(HelpCanvasButton);
        ConfigureHelpCanvasButtonHover(HelpCanvasHellButton);

    }

    private void EnsureGameOverAudioSource()
    {
        if (gameOverAudioSource != null)
        {
            return;
        }

        gameOverAudioSource = GetComponent<AudioSource>();
        if (gameOverAudioSource == null)
        {
            gameOverAudioSource = gameObject.AddComponent<AudioSource>();
        }

        gameOverAudioSource.playOnAwake = false;
        gameOverAudioSource.loop = false;
        gameOverAudioSource.ignoreListenerPause = true;
    }

    private void BindGameOverButton()
    {
        if (gameOverMainMenuButton == null)
        {
            return;
        }

        gameOverMainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
        gameOverMainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void ConfigureHelpButtonHover()
    {
        if (HelpButton == null)
        {
            return;
        }

        EventTrigger trigger = HelpButton.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = HelpButton.gameObject.AddComponent<EventTrigger>();
        }

        if (trigger.triggers == null)
        {
            trigger.triggers = new List<EventTrigger.Entry>();
        }

        RemoveHelpHoverEntries(trigger, EventTriggerType.PointerEnter);
        RemoveHelpHoverEntries(trigger, EventTriggerType.PointerExit);

        EventTrigger.Entry onEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        onEnter.callback.AddListener(_ => HandleHelpPointerEnter());
        trigger.triggers.Add(onEnter);

        EventTrigger.Entry onExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        onExit.callback.AddListener(_ => HandleHelpPointerExit());
        trigger.triggers.Add(onExit);
    }

    private void ConfigureHelpCanvasButtonHover(Button hoverButton)
    {
        if (hoverButton == null)
        {
            return;
        }

        EventTrigger trigger = hoverButton.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = hoverButton.gameObject.AddComponent<EventTrigger>();
        }

        if (trigger.triggers == null)
        {
            trigger.triggers = new List<EventTrigger.Entry>();
        }

        RemoveHelpHoverEntries(trigger, EventTriggerType.PointerEnter);
        RemoveHelpHoverEntries(trigger, EventTriggerType.PointerExit);

        EventTrigger.Entry onEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        onEnter.callback.AddListener(_ => HandleHelpPointerEnter());
        trigger.triggers.Add(onEnter);

        EventTrigger.Entry onExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        onExit.callback.AddListener(_ => HandleHelpPointerExit());
        trigger.triggers.Add(onExit);
    }

    private void HandleHelpPointerEnter()
    {
        if (PauseMenuManager.IsPaused)
        {
            HideAllHelpCanvases();
            return;
        }

        helpHoverCounter++;

        if (helpHideCoroutine != null)
        {
            StopCoroutine(helpHideCoroutine);
            helpHideCoroutine = null;
        }

        ShowActiveHelpCanvas();
    }

    private void Update()
    {
        if (PauseMenuManager.IsPaused)
        {
            HideAllHelpCanvases();
        }
    }

    private void HandleHelpPointerExit()
    {
        helpHoverCounter = Mathf.Max(0, helpHoverCounter - 1);
        if (helpHoverCounter > 0)
        {
            return;
        }

        if (helpHideCoroutine != null)
        {
            StopCoroutine(helpHideCoroutine);
        }

        helpHideCoroutine = StartCoroutine(HideHelpCanvasAfterDelay());
    }

    private IEnumerator HideHelpCanvasAfterDelay()
    {
        if (helpHideDelaySeconds > 0f)
        {
            yield return new WaitForSeconds(helpHideDelaySeconds);
        }

        if (helpHoverCounter == 0)
        {
            HideAllHelpCanvases();
        }

        helpHideCoroutine = null;
    }

    private void ConfigureHelpButtonHitZone()
    {
        if (HelpButton == null)
        {
            return;
        }

        Graphic targetGraphic = HelpButton.targetGraphic;
        if (targetGraphic == null)
        {
            targetGraphic = HelpButton.GetComponent<Graphic>();
        }

        if (targetGraphic != null)
        {
            targetGraphic.raycastPadding = helpButtonRaycastPadding;
        }
    }

    private GameObject GetActiveHelpCanvas()
    {
        bool isInAfterlife = PlayerHealth != null && PlayerHealth.IsInAfterlife;
        if (isInAfterlife && HelpCanvasHell != null)
        {
            return HelpCanvasHell;
        }

        return HelpCanvas;
    }

    private void ShowActiveHelpCanvas()
    {
        GameObject activeCanvas = GetActiveHelpCanvas();
        SetUiVisible(HelpCanvas, activeCanvas == HelpCanvas);
        SetUiVisible(HelpCanvasHell, activeCanvas == HelpCanvasHell);
    }

    private void HideAllHelpCanvases()
    {
        SetUiVisible(HelpCanvas, false);
        SetUiVisible(HelpCanvasHell, false);
    }

    private static void RemoveHelpHoverEntries(EventTrigger trigger, EventTriggerType eventType)
    {
        for (int i = trigger.triggers.Count - 1; i >= 0; i--)
        {
            EventTrigger.Entry entry = trigger.triggers[i];
            if (entry != null && entry.eventID == eventType)
            {
                trigger.triggers.RemoveAt(i);
            }
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
        HeartsChanged(PlayerHealth != null ? PlayerHealth.hearts : 0);
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

        RestoreTimeAndAudioIfNeeded();
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

        if (helpHoverCounter > 0)
        {
            ShowActiveHelpCanvas();
        }
    }

    private void HeartsChanged(int hearts)
    {
        if (heartText != null)
        {
            heartText.text = hearts.ToString();
        }

        if (hearts < 0)
        {
            ShowGameOver();
        }
    }

    private void ShowGameOver()
    {
        if (isGameOverShown)
        {
            return;
        }

        isGameOverShown = true;
        IsChoosingRoom = false;
        onRoomChoiceSelected = null;

        HideAllHelpCanvases();

        if (roomChoiceRoot != null)
        {
            roomChoiceRoot.SetActive(false);
        }

        if (freezeTimeOnGameOver)
        {
            previousTimeScale = Time.timeScale;
            previousAudioPaused = AudioListener.pause;
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }

        if (gameOverRoot != null)
        {
            SetUiVisible(gameOverRoot, true);

            if (gameOverAnimationCoroutine != null)
            {
                StopCoroutine(gameOverAnimationCoroutine);
            }

            gameOverAnimationCoroutine = StartCoroutine(AnimateGameOverUi());
        }

        PlayGameOverMusic();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private IEnumerator AnimateGameOverUi()
    {
        if (gameOverRoot == null)
        {
            gameOverAnimationCoroutine = null;
            yield break;
        }

        CanvasGroup canvasGroup = gameOverRoot.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameOverRoot.AddComponent<CanvasGroup>();
        }

        RectTransform rectTransform = gameOverRoot.GetComponent<RectTransform>();

        float initialScale = Mathf.Max(0.01f, gameOverStartScale);
        float fadeDuration = Mathf.Max(0.01f, gameOverFadeDuration);
        float scaleDuration = Mathf.Max(0.01f, gameOverScaleDuration);
        float totalDuration = Mathf.Max(fadeDuration, scaleDuration);

        canvasGroup.alpha = 0f;
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one * initialScale;
        }

        float elapsed = 0f;
        while (elapsed < totalDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float fadeT = Mathf.Clamp01(elapsed / fadeDuration);
            canvasGroup.alpha = fadeT;

            if (rectTransform != null)
            {
                float scaleT = Mathf.Clamp01(elapsed / scaleDuration);
                float easedScaleT = 1f - Mathf.Pow(1f - scaleT, 3f);
                float scale = Mathf.Lerp(initialScale, 1f, easedScaleT);
                rectTransform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        canvasGroup.alpha = 1f;
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
        }

        gameOverAnimationCoroutine = null;
    }

    private void PlayGameOverMusic()
    {
        if (gameOverMusicClip == null)
        {
            return;
        }

        EnsureGameOverAudioSource();
        if (gameOverAudioSource == null)
        {
            return;
        }

        gameOverAudioSource.clip = gameOverMusicClip;
        gameOverAudioSource.volume = Mathf.Clamp01(gameOverMusicVolume);
        gameOverAudioSource.loop = loopGameOverMusic;
        gameOverAudioSource.ignoreListenerPause = true;
        gameOverAudioSource.Play();
    }

    private void ReturnToMainMenu()
    {
        RestoreTimeAndAudioIfNeeded();

        if (mainMenuBuildIndex >= 0 && mainMenuBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(mainMenuBuildIndex);
            return;
        }

        Debug.LogWarning("GameUIHandler: Main menu build index is out of range. Set Main Menu Build Index on GameUIHandler.");
    }

    private void RestoreTimeAndAudioIfNeeded()
    {
        if (!freezeTimeOnGameOver || !isGameOverShown)
        {
            return;
        }

        Time.timeScale = previousTimeScale;
        AudioListener.pause = previousAudioPaused;
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