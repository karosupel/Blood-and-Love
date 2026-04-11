using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneEntry
    {
        [Header("Content")]
        public string enterDialogueId;
        public Sprite image;
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
    [SerializeField] private Image cutsceneImage;
    [SerializeField] private Transform foregroundImageParent;
    [SerializeField] private Transform backgroundImageParent;

    [Header("Flow")]
    [SerializeField] private bool playOnStart = true;

    private Coroutine cutsceneRoutine;
    private bool waitingForFullscreenClick;
    private float cachedTimeScale = 1f;

    private void Awake()
    {
        SetCutsceneImageVisible(false);
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

            yield return PlayDialogueAndWait(entry.enterDialogueId);

            if (entry.image != null)
            {
                ShowFullscreenImage(entry.image);

                cachedTimeScale = Time.timeScale;
                waitingForFullscreenClick = true;
                Time.timeScale = 0f;

                yield return WaitForFreshLeftClick();

                Time.timeScale = cachedTimeScale;
                waitingForFullscreenClick = false;

                MoveImageToBackground(entry.image);
            }
            else
            {
                SetCutsceneImageVisible(false);
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

    private void ShowFullscreenImage(Sprite spriteToUse)
    {
        if (cutsceneImage != null)
        {
            cutsceneImage.sprite = spriteToUse;
        }

        SetForegroundLayerOrder();
        MoveCutsceneImageToParent(foregroundImageParent);
        SetCutsceneImageVisible(spriteToUse != null);
    }

    private void MoveImageToBackground(Sprite spriteToUse)
    {
        if (cutsceneImage != null)
        {
            cutsceneImage.sprite = spriteToUse;
        }

        SetBackgroundLayerOrder();
        MoveCutsceneImageToParent(backgroundImageParent);
        SetCutsceneImageVisible(spriteToUse != null);
    }

    private IEnumerator WaitForFreshLeftClick()
    {
        // Ignore the click that may have just advanced dialogue.
        yield return new WaitUntil(() => !Input.GetMouseButton(0));
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
    }

    private void MoveCutsceneImageToParent(Transform parent)
    {
        if (cutsceneImage == null || parent == null)
        {
            return;
        }

        RectTransform parentRect = parent as RectTransform;
        if (parentRect == null)
        {
            Debug.LogWarning("CutsceneManager: image parent should be a UI RectTransform under a Canvas.");
            return;
        }

        RectTransform imageRect = cutsceneImage.rectTransform;
        imageRect.SetParent(parentRect, false);
        imageRect.localScale = Vector3.one;
        imageRect.localRotation = Quaternion.identity;
        imageRect.anchoredPosition = Vector2.zero;

        // Stretch to fill the chosen layer parent so ordering is controlled by hierarchy.
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;
    }

    private void SetCutsceneImageVisible(bool isVisible)
    {
        if (cutsceneImage == null)
        {
            return;
        }

        cutsceneImage.gameObject.SetActive(isVisible);
        cutsceneImage.enabled = isVisible && cutsceneImage.sprite != null;
    }

    private void SetForegroundLayerOrder()
    {
        if (foregroundImageParent != null)
        {
            foregroundImageParent.SetAsLastSibling();
        }
    }

    private void SetBackgroundLayerOrder()
    {
        if (backgroundImageParent != null)
        {
            backgroundImageParent.SetAsFirstSibling();
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
