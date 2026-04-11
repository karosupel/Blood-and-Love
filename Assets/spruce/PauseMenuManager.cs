using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("Pause UI")]
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private Button returnToGameButton;
    [SerializeField] private Button quitButton;

    [Header("Input")]
    [SerializeField] private KeyCode pauseToggleKey = KeyCode.Escape;

    [Header("Scene Build Index Navigation")]
    [SerializeField] private int mainMenuBuildIndex = 0;

    [Header("Quit Warning")]
    [SerializeField] private GameObject quitWarningPanel;
    [SerializeField] private GameObject[] quitWarningElements;
    [SerializeField] private Button warningBackButton;
    [SerializeField] private Button warningQuitButton;
    [SerializeField] private TMP_Text quitWarningText;
    [TextArea]
    [SerializeField] private string quitWarningMessage = "Quit to Main Menu? Progress will not be saved.";

    private float previousTimeScale = 1f;
    private bool previousAudioPaused;
    private bool isQuitWarningOpen;

    private void Awake()
    {
        if (pauseMenuCanvas == null)
        {
            pauseMenuCanvas = GameObject.Find("PauseMenuCanvas");
        }

        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }

        BindPauseButtons();
        BindWarningButtons();

        HideQuitWarning();
    }

    private void BindPauseButtons()
    {
        if (returnToGameButton != null)
        {
            returnToGameButton.onClick.RemoveListener(ResumeGame);
            returnToGameButton.onClick.AddListener(ResumeGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitToMainMenu);
            quitButton.onClick.AddListener(QuitToMainMenu);
        }
    }

    private void BindWarningButtons()
    {
        if (warningBackButton != null)
        {
            warningBackButton.onClick.RemoveListener(CancelQuitToMainMenu);
            warningBackButton.onClick.AddListener(CancelQuitToMainMenu);
        }

        if (warningQuitButton != null)
        {
            warningQuitButton.onClick.RemoveListener(ConfirmQuitToMainMenu);
            warningQuitButton.onClick.AddListener(ConfirmQuitToMainMenu);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseToggleKey))
        {
            if (IsPaused && isQuitWarningOpen)
            {
                CancelQuitToMainMenu();
                return;
            }

            TogglePause();
        }
    }

    private void OnDisable()
    {
        if (IsPaused)
        {
            ResumeGame();
        }
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            ResumeGame();
            return;
        }

        PauseGame();
    }

    public void PauseGame()
    {
        if (IsPaused)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        previousAudioPaused = AudioListener.pause;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        IsPaused = true;

        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(true);
        }

        HideQuitWarning();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        if (!IsPaused)
        {
            return;
        }

        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }

        HideQuitWarning();

        Time.timeScale = previousTimeScale;
        AudioListener.pause = previousAudioPaused;
        IsPaused = false;
    }

    public void QuitToMainMenu()
    {
        ShowQuitWarning();
    }

    public void ConfirmQuitToMainMenu()
    {
        ResumeGame();

        if (mainMenuBuildIndex >= 0 && mainMenuBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(mainMenuBuildIndex);
            return;
        }

        Debug.LogWarning("PauseMenuManager: Main menu build index is out of range. Set Main Menu Build Index in the inspector.");
    }

    public void CancelQuitToMainMenu()
    {
        HideQuitWarning();
    }

    private void ShowQuitWarning()
    {
        if (quitWarningText != null)
        {
            quitWarningText.text = quitWarningMessage;
        }

        SetWarningUiVisible(true);

        isQuitWarningOpen = true;
    }

    private void HideQuitWarning()
    {
        SetWarningUiVisible(false);

        isQuitWarningOpen = false;
    }

    private void SetWarningUiVisible(bool visible)
    {
        if (quitWarningPanel != null)
        {
            quitWarningPanel.SetActive(visible);

            Canvas canvas = quitWarningPanel.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = visible;
            }

            GraphicRaycaster raycaster = quitWarningPanel.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                raycaster.enabled = visible;
            }

            CanvasGroup canvasGroup = quitWarningPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
        }

        if (quitWarningElements == null)
        {
            return;
        }

        for (int i = 0; i < quitWarningElements.Length; i++)
        {
            if (quitWarningElements[i] != null)
            {
                quitWarningElements[i].SetActive(visible);
            }
        }
    }
}
