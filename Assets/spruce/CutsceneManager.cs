using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneEntry
    {
        [Header("Content")]
        public string enterDialogueId;
        public Sprite backgroundImage;
        [FormerlySerializedAs("image")] public Sprite foregroundImage;
        public string exitDialogueId;

        [Header("Optional Scene Transition")]
        public bool transitionToScene;
        public int nextSceneBuildIndex = -1;
        [Min(0f)] public float sceneTransitionDelaySeconds = 1f;
    }

    [Header("References")]
    [SerializeField] private Dialogue dialogue;

    [Header("Cutscene List")]
    [SerializeField] private List<CutsceneEntry> cutscenes = new List<CutsceneEntry>();
    [SerializeField, Min(0)] private int startCutsceneIndex;

    [Header("Cutscene Image")]
    [FormerlySerializedAs("cutsceneImage")]
    [SerializeField] private Image foregroundCutsceneImage;
    [SerializeField] private Image backgroundCutsceneImage;
    [SerializeField] private Transform foregroundImageParent;
    [SerializeField] private Transform backgroundImageParent;

    [Header("Flow")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool skipRepeatedBoundaryDialogues = true;

    private Coroutine cutsceneRoutine;
    private bool waitingForFullscreenClick;
    private float cachedTimeScale = 1f;

    private void Awake()
    {
        AttachImageToParent(foregroundCutsceneImage, foregroundImageParent, "foreground");
        AttachImageToParent(backgroundCutsceneImage, backgroundImageParent, "background");

        SetForegroundImageVisible(false);
        SetBackgroundImageVisible(false);
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartCutsceneSequence(startCutsceneIndex);
        }
    }

    private void OnDisable()
    {
        if (cutsceneRoutine != null)
        {
            StopCoroutine(cutsceneRoutine);
            cutsceneRoutine = null;
        }

        if (waitingForFullscreenClick)
        {
            Time.timeScale = cachedTimeScale;
            waitingForFullscreenClick = false;
        }
    }

    public void StartCutscene()
    {
        StartCutsceneSequence(startCutsceneIndex);
    }

    public void StartCutsceneSequence(int fromIndex = 0)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("CutsceneManager: Dialogue reference is missing.");
            return;
        }

        if (cutscenes == null || cutscenes.Count == 0)
        {
            Debug.LogWarning("CutsceneManager: cutscene list is empty.");
            return;
        }

        if (fromIndex < 0 || fromIndex >= cutscenes.Count)
        {
            Debug.LogWarning($"CutsceneManager: cutscene start index {fromIndex} is out of range.");
            return;
        }

        if (cutsceneRoutine != null)
        {
            StopCoroutine(cutsceneRoutine);
        }

        cutsceneRoutine = StartCoroutine(CutsceneRoutine(fromIndex));
    }

    private IEnumerator CutsceneRoutine(int fromIndex)
    {
        for (int i = fromIndex; i < cutscenes.Count; i++)
        {
            CutsceneEntry entry = cutscenes[i];
            if (entry == null)
            {
                continue;
            }

            bool skipEnterDialogue = ShouldSkipEnterDialogueAtBoundary(i, fromIndex, entry);
            if (!skipEnterDialogue)
            {
                yield return PlayDialogueAndWait(entry.enterDialogueId);
            }

            ShowBackgroundImage(entry.backgroundImage);

            ShowForegroundImage(entry.foregroundImage);

            bool hasAnyCutsceneImage = entry.backgroundImage != null || entry.foregroundImage != null;
            if (hasAnyCutsceneImage)
            {
                MoveAllImagesToForegroundLayer();

                cachedTimeScale = Time.timeScale;
                waitingForFullscreenClick = true;
                Time.timeScale = 0f;

                yield return WaitForFreshLeftClick();

                Time.timeScale = cachedTimeScale;
                waitingForFullscreenClick = false;

                MoveAllImagesToBackgroundLayer();
            }
            else
            {
                SetForegroundImageVisible(false);
                SetBackgroundImageVisible(false);
            }

            yield return PlayDialogueAndWait(entry.exitDialogueId);

            if (entry.transitionToScene)
            {
                if (entry.sceneTransitionDelaySeconds > 0f)
                {
                    yield return new WaitForSecondsRealtime(entry.sceneTransitionDelaySeconds);
                }

                if (TryLoadScene(entry.nextSceneBuildIndex))
                {
                    cutsceneRoutine = null;
                    yield break;
                }
            }
        }

        cutsceneRoutine = null;
    }

    private bool ShouldSkipEnterDialogueAtBoundary(int cutsceneIndex, int startIndex, CutsceneEntry currentEntry)
    {
        if (!skipRepeatedBoundaryDialogues)
        {
            return false;
        }

        if (currentEntry == null || cutsceneIndex <= startIndex)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(currentEntry.enterDialogueId))
        {
            return false;
        }

        CutsceneEntry previousEntry = cutscenes[cutsceneIndex - 1];
        if (previousEntry == null || string.IsNullOrWhiteSpace(previousEntry.exitDialogueId))
        {
            return false;
        }

        return string.Equals(previousEntry.exitDialogueId, currentEntry.enterDialogueId, System.StringComparison.Ordinal);
    }

    private IEnumerator PlayDialogueAndWait(string dialogueId)
    {
        if (string.IsNullOrWhiteSpace(dialogueId))
        {
            yield break;
        }

        dialogue.PlayDialogue(dialogueId);

        // Wait one frame for Dialogue.IsPlaying to update if needed.
        yield return null;

        if (!dialogue.IsPlaying)
        {
            Debug.LogWarning($"CutsceneManager: dialogue '{dialogueId}' did not start.");
            yield break;
        }

        yield return new WaitUntil(() => !dialogue.IsPlaying);
    }

    private void ShowForegroundImage(Sprite spriteToUse)
    {
        if (foregroundCutsceneImage != null)
        {
            foregroundCutsceneImage.sprite = spriteToUse;
        }

        SetForegroundImageVisible(spriteToUse != null);
    }

    private void ShowBackgroundImage(Sprite spriteToUse)
    {
        if (backgroundCutsceneImage != null)
        {
            backgroundCutsceneImage.sprite = spriteToUse;
        }

        SetBackgroundImageVisible(spriteToUse != null);
    }

    private IEnumerator WaitForFreshLeftClick()
    {
        // Ignore the click that may have just advanced dialogue.
        yield return new WaitUntil(() => !Input.GetMouseButton(0)||!Input.GetKey(KeyCode.Space));
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0)||Input.GetKey(KeyCode.Space));
    }

    private void AttachImageToParent(Image image, Transform parent, string imageKind)
    {
        if (image == null || parent == null)
        {
            return;
        }

        RectTransform parentRect = parent as RectTransform;
        if (parentRect == null)
        {
            Debug.LogWarning($"CutsceneManager: {imageKind} image parent should be a UI RectTransform under a Canvas.");
            return;
        }

        image.rectTransform.SetParent(parentRect, false);
    }

    private void SetForegroundImageVisible(bool isVisible)
    {
        if (foregroundCutsceneImage == null)
        {
            return;
        }

        foregroundCutsceneImage.gameObject.SetActive(isVisible);
        foregroundCutsceneImage.enabled = isVisible && foregroundCutsceneImage.sprite != null;
    }

    private void SetBackgroundImageVisible(bool isVisible)
    {
        if (backgroundCutsceneImage == null)
        {
            return;
        }

        backgroundCutsceneImage.gameObject.SetActive(isVisible);
        backgroundCutsceneImage.enabled = isVisible && backgroundCutsceneImage.sprite != null;
    }

    private void MoveAllImagesToForegroundLayer()
    {
        AttachImageToParent(backgroundCutsceneImage, foregroundImageParent, "background");
        AttachImageToParent(foregroundCutsceneImage, foregroundImageParent, "foreground");

        EnsureForegroundDrawnAboveBackground();

        if (foregroundImageParent != null)
        {
            foregroundImageParent.SetAsLastSibling();
        }
    }

    private void MoveAllImagesToBackgroundLayer()
    {
        AttachImageToParent(backgroundCutsceneImage, backgroundImageParent, "background");
        AttachImageToParent(foregroundCutsceneImage, backgroundImageParent, "foreground");

        EnsureForegroundDrawnAboveBackground();

        if (backgroundImageParent != null)
        {
            backgroundImageParent.SetAsFirstSibling();
        }
    }

    private void EnsureForegroundDrawnAboveBackground()
    {
        if (backgroundCutsceneImage != null)
        {
            backgroundCutsceneImage.rectTransform.SetAsFirstSibling();
        }

        if (foregroundCutsceneImage != null)
        {
            foregroundCutsceneImage.rectTransform.SetAsLastSibling();
        }
    }

    private bool TryLoadScene(int sceneBuildIndex)
    {
        if (sceneBuildIndex >= 0 && sceneBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneBuildIndex);
            return true;
        }

        Debug.LogWarning($"CutsceneManager: scene build index {sceneBuildIndex} is out of range.");
        return false;
    }
}
