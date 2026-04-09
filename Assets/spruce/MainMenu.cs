using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Setup")]
    [SerializeField] private bool createMenuAtRuntime = false;
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private GameObject gameUiRoot;
    [SerializeField] private string targetCanvasName = "MainMenuCanvas";

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private string startButtonName = "PlayButton";
    [SerializeField] private Button exitButton;
    [SerializeField] private string exitButtonName = "QuitButton";
    [SerializeField] private Button creditsButton;
    [SerializeField] private string creditsButtonName = "CreditsButton";
    [SerializeField] private GameObject creditsCanvasRoot;
    [SerializeField] private Button creditsBackButton;
    [SerializeField] private string creditsBackButtonName = "CreditsBackButton";

    [Header("Scenes")]
    [SerializeField] private bool menuInSeparateScene = true;
    [SerializeField] private string firstStageSceneName = "";
    [SerializeField] private int firstStageBuildIndex = 1;
    [SerializeField] private bool pauseGameWhileOpen = true;

    private float _originalTimeScale = 1f;

    private static readonly char[] _nameStripChars = { ' ', '_', '-' };

    void Start()
    {
        EnsureEventSystemExists();

        if (menuRoot == null)
        {
            Canvas existingCanvas = FindCanvasByName(targetCanvasName);
            if (existingCanvas != null)
            {
                menuRoot = existingCanvas.gameObject;
            }
            else if (createMenuAtRuntime)
            {
                menuRoot = BuildRuntimeMenu();
            }
        }

        if (menuRoot == null)
        {
            Debug.LogWarning("MainMenu: No Canvas named '" + targetCanvasName + "' was found. Assign Menu Root or enable Create Menu At Runtime.");
            LogAvailableCanvases();
            return;
        }

        HookStartButton();
        HookExitButton();
        HookCreditsButton();
        HookCreditsBackButton();

        ShowMenu();
    }

    public void ShowMenu()
    {
        if (creditsCanvasRoot != null)
        {
            creditsCanvasRoot.SetActive(false);
        }

        if (!menuInSeparateScene)
        {
            SetGameUiVisible(false);
        }

        if (menuRoot != null)
        {
            EnsureParentsActive(menuRoot.transform);
            menuRoot.SetActive(true);

            Canvas canvas = menuRoot.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = true;

                GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster != null)
                {
                    raycaster.enabled = true;
                }
            }

            CanvasGroup canvasGroup = menuRoot.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (pauseGameWhileOpen)
        {
            _originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    public void StartGame()
    {
        if (!menuInSeparateScene)
        {
            HideMenu();
            SetGameUiVisible(true);
        }

        if (pauseGameWhileOpen)
        {
            Time.timeScale = _originalTimeScale <= 0f ? 1f : _originalTimeScale;
        }

        string sceneToLoad = ResolveSceneToLoad();
        if (!string.IsNullOrWhiteSpace(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
            return;
        }

        int fallbackBuildIndex = ResolveSceneBuildIndexToLoad();
        if (fallbackBuildIndex >= 0)
        {
            SceneManager.LoadScene(fallbackBuildIndex);
            return;
        }

        if (SceneManager.sceneCountInBuildSettings == 0)
        {
            Debug.LogWarning("MainMenu: No scenes in Build Settings. Add your menu and first stage scenes to Build Settings.");
            return;
        }

        Debug.LogWarning("MainMenu: Could not resolve first stage. Set First Stage Scene Name or First Stage Build Index.");
    }

    public void HideMenu()
    {
        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }
    }

    private void HookStartButton()
    {
        if (startButton == null)
        {
            startButton = FindButtonOnMenu(startButtonName);
        }

        if (startButton == null)
        {
            Debug.LogWarning("MainMenu: Start button was not found. Assign Start Button in Inspector or rename the button to '" + startButtonName + "'.");
            return;
        }

        startButton.onClick.RemoveListener(OnStartButtonPressed);
        startButton.onClick.AddListener(OnStartButtonPressed);
    }

    private void OnStartButtonPressed()
    {
        Debug.Log("MainMenu: Start button pressed.");
        StartGame();
    }

    private void HookExitButton()
    {
        if (exitButton == null)
        {
            exitButton = FindButtonOnMenu(exitButtonName);
        }

        if (exitButton == null)
        {
            Debug.LogWarning("MainMenu: Exit button was not found. Assign Exit Button in Inspector or rename the button to '" + exitButtonName + "'.");
            return;
        }

        exitButton.onClick.RemoveListener(OnExitButtonPressed);
        exitButton.onClick.AddListener(OnExitButtonPressed);
    }

    private void OnExitButtonPressed()
    {
        Debug.Log("MainMenu: Exit button pressed.");
        QuitGame();
    }

    private void HookCreditsButton()
    {
        if (creditsButton == null)
        {
            creditsButton = FindButtonOnMenu(creditsButtonName);
        }

        if (creditsButton == null)
        {
            Debug.LogWarning("MainMenu: Credits button was not found. Assign Credits Button in Inspector or rename the button to '" + creditsButtonName + "'.");
            return;
        }

        creditsButton.onClick.RemoveListener(OnCreditsButtonPressed);
        creditsButton.onClick.AddListener(OnCreditsButtonPressed);
    }

    private void OnCreditsButtonPressed()
    {
        Debug.Log("MainMenu: Credits button pressed.");
        ShowCreditsCanvas();
    }

    public void ShowCreditsCanvas()
    {
        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }

        if (creditsCanvasRoot == null)
        {
            Canvas creditsCanvas = FindCanvasByName("CreditsCanvas");
            if (creditsCanvas != null)
            {
                creditsCanvasRoot = creditsCanvas.gameObject;
            }
        }

        if (creditsCanvasRoot == null)
        {
            Debug.LogWarning("MainMenu: CreditsCanvas was not found. Assign Credits Canvas Root in Inspector or name the canvas 'CreditsCanvas'.");
            return;
        }

        EnsureParentsActive(creditsCanvasRoot.transform);
        creditsCanvasRoot.SetActive(true);
    }

    public void ReturnToMainMenuFromCredits()
    {
        if (creditsCanvasRoot != null)
        {
            creditsCanvasRoot.SetActive(false);
        }

        ShowMenu();
    }

    private void HookCreditsBackButton()
    {
        if (creditsCanvasRoot == null)
        {
            Canvas creditsCanvas = FindCanvasByName("CreditsCanvas");
            if (creditsCanvas != null)
            {
                creditsCanvasRoot = creditsCanvas.gameObject;
            }
        }

        if (creditsCanvasRoot == null)
        {
            Debug.LogWarning("MainMenu: CreditsCanvas was not found for back-button hookup.");
            return;
        }

        if (creditsBackButton == null)
        {
            Button[] creditButtons = creditsCanvasRoot.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < creditButtons.Length; i++)
            {
                if (string.Equals(creditButtons[i].name, creditsBackButtonName, System.StringComparison.OrdinalIgnoreCase))
                {
                    creditsBackButton = creditButtons[i];
                    break;
                }
            }
        }

        if (creditsBackButton == null)
        {
            Debug.LogWarning("MainMenu: Credits back button was not found. Assign Credits Back Button in Inspector or rename it to '" + creditsBackButtonName + "'.");
            return;
        }

        creditsBackButton.onClick.RemoveListener(OnCreditsBackButtonPressed);
        creditsBackButton.onClick.AddListener(OnCreditsBackButtonPressed);
    }

    private void OnCreditsBackButtonPressed()
    {
        Debug.Log("MainMenu: Credits back button pressed.");
        ReturnToMainMenuFromCredits();
    }

    private Button FindButtonOnMenu(string buttonName)
    {
        if (menuRoot == null)
        {
            return null;
        }

        Button[] buttons = menuRoot.GetComponentsInChildren<Button>(true);
        if (buttons == null || buttons.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            if (string.Equals(buttons[i].name, buttonName, System.StringComparison.OrdinalIgnoreCase))
            {
                return buttons[i];
            }
        }

        return buttons[0];
    }

    private void SetGameUiVisible(bool isVisible)
    {
        if (gameUiRoot != null)
        {
            gameUiRoot.SetActive(isVisible);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private string ResolveSceneToLoad()
    {
        if (!string.IsNullOrWhiteSpace(firstStageSceneName))
        {
            if (Application.CanStreamedLevelBeLoaded(firstStageSceneName))
            {
                return firstStageSceneName;
            }

            Debug.LogWarning("MainMenu: First Stage Scene Name '" + firstStageSceneName + "' is not loadable from Build Settings.");
        }

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        if (sceneCount == 0)
        {
            return string.Empty;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (!string.Equals(sceneName, currentSceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return sceneName;
            }
        }

        return string.Empty;
    }

    private int ResolveSceneBuildIndexToLoad()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        if (sceneCount == 0)
        {
            return -1;
        }

        if (firstStageBuildIndex >= 0 && firstStageBuildIndex < sceneCount)
        {
            return firstStageBuildIndex;
        }

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        if (sceneCount > 1)
        {
            if (currentIndex == 0)
            {
                return 1;
            }

            return 0;
        }

        return -1;
    }

    private Canvas FindCanvasByName(string canvasName)
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        string normalizedTarget = NormalizeName(canvasName);

        for (int i = 0; i < canvases.Length; i++)
        {
            string candidateName = canvases[i].name;
            if (string.Equals(candidateName, canvasName, System.StringComparison.OrdinalIgnoreCase))
            {
                return canvases[i];
            }

            if (string.Equals(NormalizeName(candidateName), normalizedTarget, System.StringComparison.Ordinal))
            {
                return canvases[i];
            }
        }

        return null;
    }

    private string NormalizeName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.ToLowerInvariant().Trim(_nameStripChars);
    }

    private void EnsureParentsActive(Transform target)
    {
        Transform current = target;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                current.gameObject.SetActive(true);
            }

            current = current.parent;
        }
    }

    private void LogAvailableCanvases()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        if (canvases.Length == 0)
        {
            Debug.LogWarning("MainMenu: No Canvas objects exist in this loaded scene.");
            return;
        }

        string[] canvasNames = new string[canvases.Length];
        for (int i = 0; i < canvases.Length; i++)
        {
            canvasNames[i] = canvases[i].name;
        }

        Debug.Log("MainMenu: Available canvases: " + string.Join(", ", canvasNames));
    }

    private GameObject BuildRuntimeMenu()
    {
        EnsureEventSystemExists();

        GameObject canvasObject = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform panel = CreatePanel(canvasObject.transform);
        CreateLabel(panel, "BLOOD AND LOVE", new Vector2(0f, 120f), 48, FontStyle.Bold);

        Button playButton = CreateButton(panel, "Play", new Vector2(0f, 20f));
        playButton.onClick.AddListener(StartGame);

        Button quitButton = CreateButton(panel, "Quit", new Vector2(0f, -60f));
        quitButton.onClick.AddListener(QuitGame);

        return canvasObject;
    }

    private void EnsureEventSystemExists()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private RectTransform CreatePanel(Transform parent)
    {
        GameObject panelObject = new GameObject("MenuPanel");
        panelObject.transform.SetParent(parent, false);

        Image image = panelObject.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.65f);

        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return rect;
    }

    private void CreateLabel(Transform parent, string text, Vector2 anchoredPos, int fontSize, FontStyle style)
    {
        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(parent, false);

        Text label = labelObject.AddComponent<Text>();
        label.text = text;
        label.font = GetDefaultFont();
        label.fontStyle = style;
        label.fontSize = fontSize;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(900f, 100f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
    }

    private Button CreateButton(Transform parent, string title, Vector2 anchoredPos)
    {
        GameObject buttonObject = new GameObject(title + "Button");
        buttonObject.transform.SetParent(parent, false);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.85f, 0.2f, 0.2f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.35f, 0.35f, 1f);
        colors.pressedColor = new Color(0.7f, 0.1f, 0.1f, 1f);
        button.colors = colors;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(280f, 56f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;

        GameObject labelObject = new GameObject("Text");
        labelObject.transform.SetParent(buttonObject.transform, false);

        Text label = labelObject.AddComponent<Text>();
        label.text = title;
        label.font = GetDefaultFont();
        label.fontSize = 28;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;

        RectTransform textRect = labelObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private Font GetDefaultFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }
}
