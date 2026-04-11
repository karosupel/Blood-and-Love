using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Dialogue dialogue;

    [Header("Dialogue IDs")]
    [SerializeField] private string introDialogueId;
    [SerializeField] private string afterImageDialogueId;

    [Header("Cutscene Image")]
    [SerializeField] private Sprite cutsceneSprite;
    [SerializeField] private Image cutsceneImage;
    [SerializeField] private Transform foregroundImageParent;
    [SerializeField] private Transform backgroundImageParent;

    [Header("Scene Transition")]
    [SerializeField] private int nextSceneBuildIndex = -1;
    [SerializeField, Min(0f)] private float sceneTransitionDelaySeconds = 1f;

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
            StartCutscene();
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
        if (dialogue == null)
        {
            Debug.LogWarning("CutsceneManager: Dialogue reference is missing.");
            return;
        }

        if (cutsceneRoutine != null)
        {
            StopCoroutine(cutsceneRoutine);
        }

        cutsceneRoutine = StartCoroutine(CutsceneRoutine());
    }

    private IEnumerator CutsceneRoutine()
    {
        yield return PlayDialogueAndWait(introDialogueId);

        ShowFullscreenImage();

        cachedTimeScale = Time.timeScale;
        waitingForFullscreenClick = true;
        Time.timeScale = 0f;

        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        Time.timeScale = cachedTimeScale;
        waitingForFullscreenClick = false;

        MoveImageToBackground();

        yield return PlayDialogueAndWait(afterImageDialogueId);

        if (sceneTransitionDelaySeconds > 0f)
        {
            yield return new WaitForSecondsRealtime(sceneTransitionDelaySeconds);
        }

        TransitionToNextScene();
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

    private void ShowFullscreenImage()
    {
        Sprite spriteToUse = ResolveCutsceneSprite();

        if (spriteToUse == null)
        {
            Debug.LogWarning("CutsceneManager: no cutscene sprite assigned. Set Cutscene Sprite or assign a sprite on Fullscreen Image.");
        }

        if (cutsceneImage != null)
        {
            cutsceneImage.sprite = spriteToUse;
        }

        MoveCutsceneImageToParent(foregroundImageParent);
        SetCutsceneImageVisible(spriteToUse != null);
    }

    private void MoveImageToBackground()
    {
        Sprite spriteToUse = ResolveCutsceneSprite();

        if (cutsceneImage != null)
        {
            cutsceneImage.sprite = spriteToUse;
        }

        MoveCutsceneImageToParent(backgroundImageParent);
        SetCutsceneImageVisible(spriteToUse != null);
    }

    private Sprite ResolveCutsceneSprite()
    {
        if (cutsceneSprite != null)
        {
            return cutsceneSprite;
        }

        if (cutsceneImage != null)
        {
            return cutsceneImage.sprite;
        }

        return null;
    }

    private void MoveCutsceneImageToParent(Transform parent)
    {
        if (cutsceneImage == null || parent == null)
        {
            return;
        }

        cutsceneImage.rectTransform.SetParent(parent, false);
    }

    private void SetCutsceneImageVisible(bool isVisible)
    {
        if (cutsceneImage == null)
        {
            return;
        }

        cutsceneImage.gameObject.SetActive(isVisible);
        cutsceneImage.enabled = isVisible && ResolveCutsceneSprite() != null;
    }

    private void TransitionToNextScene()
    {
        if (nextSceneBuildIndex >= 0 && nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneBuildIndex);
            return;
        }

        Debug.LogWarning("CutsceneManager: next scene build index is out of range.");
    }
}
