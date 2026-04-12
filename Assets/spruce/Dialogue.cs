using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Dialogue : MonoBehaviour
{
    [System.Serializable]
    public class DialogueEntry
    {
        public string speaker;
        public Sprite portrait;

        [TextArea(2, 6)]
        public string text;
    }

    [System.Serializable]
    public class DialogueSequence
    {
        public string id;
        public DialogueEntry[] entries;
    }

    [Header("References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject portraitFrame;
    [SerializeField] private GameObject portraitPanel;
    [SerializeField] private GameObject dialogueVisualRoot;
    [SerializeField] private GameObject[] additionalDialogueObjectsToToggle;
    [SerializeField] private BossUIHandler bossUIHandler;
    [SerializeField] private BossUIHandler[] additionalBossUiHandlers;
    [SerializeField] private GameObject[] additionalUiRootsToHideDuringDialogue;

    [Header("Dialogue Library")]
    [SerializeField] private DialogueSequence[] dialogueLibrary;
    [SerializeField] private string defaultSequenceId;

    [Header("Boss Scene Dialogue Selection")]
    [SerializeField] private bool useBossSceneDialogueSelection;
    [SerializeField] private string bossSceneName;
    [SerializeField] private int bossSceneBuildIndex = -1;
    [SerializeField] private bool playBossSceneFirstDialogueOnStart = true;
    [SerializeField] private bool autoPlayBossSceneSecondDialogueAfterFirst = true;
    [SerializeField] private bool forceShowBossUiAfterSecondBossDialogue = true;
    [SerializeField] private string bossSceneFirstDialogueId;
    [SerializeField] private string bossSceneSecondDialogueId;
    [SerializeField, Min(-1)] private int bossSceneSecondDialogueUiRootIndex = -1;

    [Header("Boss Phase Transition Dialogue")]
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private bool playDialogueAfterBossFirstPhaseEnds;
    [SerializeField] private string bossFirstPhaseEndedDialogueId;
    [SerializeField, Min(-1)] private int bossFirstPhaseEndedUiRootIndex = -1;
    [SerializeField] private bool triggerBossFirstPhaseEndedDialogueOnlyOnce = true;

    [Header("Typewriter")]
    [SerializeField, Min(0.001f)] private float letterDelaySeconds = 0.03f;

    [Header("Flow")]
    [SerializeField] private bool disableDialoguePanelObjectWhenHidden = true;

    private Coroutine playRoutine;
    private float cachedTimeScale = 1f;
    private readonly List<BossUIHandler> bossUiHandlersToRestore = new List<BossUIHandler>();
    private readonly Dictionary<GameObject, bool> uiRootActiveStateBeforeDialogue = new Dictionary<GameObject, bool>();
    private readonly HashSet<GameObject> uiRootsAllowedDuringDialogue = new HashSet<GameObject>();
    private string activeSequenceId;
    private bool isBossHealthSubscribed;
    private bool bossFirstPhaseDialogueTriggered;
    private bool pendingBossFirstPhaseEndedDialogue;

    public bool IsPlaying { get; private set; }

    private void Awake()
    {

        if (bossUIHandler == null)
        {
            bossUIHandler = FindObjectOfType<BossUIHandler>(true);
        }

        if (bossHealth == null)
        {
            bossHealth = FindObjectOfType<BossHealth>(true);
        }


        SetDialogueUiVisible(false);

        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
            dialogueText.maxVisibleCharacters = 0;
        }

        if (speakerText != null)
        {
            speakerText.text = string.Empty;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
        }

        SetPortraitUiVisible(false);
    }

    private void OnEnable()
    {
        TryBindBossHealthEvents();
    }

    private void Start()
    {
        if (useBossSceneDialogueSelection && playBossSceneFirstDialogueOnStart && IsBossSceneActive())
        {
            PlayBossSceneFirstDialogue();
            return;
        }

        PlayDefaultDialogue();
    }

    


    private void OnDisable()
    {
        UnbindBossHealthEvents();

        if (IsPlaying)
        {
            StopDialogue();
        }
    }

    private void Update()
    {
        if (!IsPlaying)
        {
            if (pendingBossFirstPhaseEndedDialogue)
            {
                pendingBossFirstPhaseEndedDialogue = false;
                PlayBossFirstPhaseEndedDialogue();
            }

            return;
        }

        // Keep gameplay paused for the entire dialogue.
        if (!Mathf.Approximately(Time.timeScale, 0f))
        {
            Time.timeScale = 0f;
        }

        ForceHideBossUiWhileDialogue();
    }

    public void PlayDialogue(string sequenceId)
    {
        uiRootsAllowedDuringDialogue.Clear();
        PlayDialogueInternal(sequenceId);
    }

    public void PlayDialogueKeepingUiVisible(string sequenceId, GameObject uiRootToKeepVisible)
    {
        uiRootsAllowedDuringDialogue.Clear();

        if (uiRootToKeepVisible != null)
        {
            uiRootsAllowedDuringDialogue.Add(uiRootToKeepVisible);
        }

        PlayDialogueInternal(sequenceId);
    }

    public void PlayDialogueKeepingUiVisible(string sequenceId, int uiRootIndexInHideList)
    {
        uiRootsAllowedDuringDialogue.Clear();

        if (additionalUiRootsToHideDuringDialogue == null || uiRootIndexInHideList < 0 || uiRootIndexInHideList >= additionalUiRootsToHideDuringDialogue.Length)
        {
            Debug.LogWarning($"Dialogue: ui root index {uiRootIndexInHideList} is out of range.");
        }
        else
        {
            GameObject uiRoot = additionalUiRootsToHideDuringDialogue[uiRootIndexInHideList];
            if (uiRoot != null)
            {
                uiRootsAllowedDuringDialogue.Add(uiRoot);
            }
        }

        PlayDialogueInternal(sequenceId);
    }

    private void PlayDialogueInternal(string sequenceId)
    {
        if (string.IsNullOrWhiteSpace(sequenceId))
        {
            Debug.LogWarning("Dialogue: sequence id is empty.");
            return;
        }

        DialogueSequence sequence = FindSequenceById(sequenceId);
        if (sequence == null)
        {
            Debug.LogWarning($"Dialogue: no sequence with id '{sequenceId}' found.");
            return;
        }

        if (sequence.entries == null || sequence.entries.Length == 0)
        {
            Debug.LogWarning($"Dialogue: sequence '{sequenceId}' has no dialogue entries.");
            return;
        }

        activeSequenceId = sequenceId;
        StartSequence(sequence);
    }

    public void PlayDefaultDialogue()
    {
        PlayDialogue(defaultSequenceId);
    }

    public void PlayBossSceneFirstDialogue()
    {
        PlayDialogue(bossSceneFirstDialogueId);
    }

    public void PlayBossSceneSecondDialogue()
    {
        if (bossSceneSecondDialogueUiRootIndex >= 0)
        {
            PlayDialogueKeepingUiVisible(bossSceneSecondDialogueId, bossSceneSecondDialogueUiRootIndex);
            return;
        }

        PlayDialogue(bossSceneSecondDialogueId);
    }

    public void PlayBossFirstPhaseEndedDialogue()
    {
        if (bossFirstPhaseEndedUiRootIndex >= 0)
        {
            PlayDialogueKeepingUiVisible(bossFirstPhaseEndedDialogueId, bossFirstPhaseEndedUiRootIndex);
            return;
        }

        PlayDialogue(bossFirstPhaseEndedDialogueId);
    }

    private bool IsBossSceneActive()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        bool hasSceneNameRule = !string.IsNullOrWhiteSpace(bossSceneName);
        bool hasBuildIndexRule = bossSceneBuildIndex >= 0;

        if (!hasSceneNameRule && !hasBuildIndexRule)
        {
            return false;
        }

        bool matchesName = hasSceneNameRule && string.Equals(activeScene.name, bossSceneName, System.StringComparison.Ordinal);
        bool matchesBuildIndex = hasBuildIndexRule && activeScene.buildIndex == bossSceneBuildIndex;
        return matchesName || matchesBuildIndex;
    }

    public void StopDialogue()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        EndDialogueState(false);
    }

    private DialogueSequence FindSequenceById(string sequenceId)
    {
        if (dialogueLibrary == null)
        {
            return null;
        }

        for (int i = 0; i < dialogueLibrary.Length; i++)
        {
            DialogueSequence sequence = dialogueLibrary[i];
            if (sequence != null && sequence.id == sequenceId)
            {
                return sequence;
            }
        }

        return null;
    }

    private void StartSequence(DialogueSequence sequence)
    {
        if (IsPlaying)
        {
            StopDialogue();
        }

        if (dialoguePanel == null || dialogueText == null)
        {
            Debug.LogWarning("Dialogue: assign both Dialogue Panel and Dialogue Text in inspector.");
            return;
        }

        IsPlaying = true;
        cachedTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        HideBossUiForDialogue();

        SetDialogueUiVisible(true);
        playRoutine = StartCoroutine(PlaySequenceRoutine(sequence));
    }

    private IEnumerator PlaySequenceRoutine(DialogueSequence sequence)
    {
        for (int i = 0; i < sequence.entries.Length; i++)
        {
            DialogueEntry entry = sequence.entries[i];
            yield return StartCoroutine(TypeLineRoutine(entry));

            // Advance only after the current line has finished revealing.
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        }

        playRoutine = null;
        EndDialogueState(true);
    }

    private IEnumerator TypeLineRoutine(DialogueEntry entry)
    {
        if (entry == null)
        {
            entry = new DialogueEntry();
        }

        if (speakerText != null)
        {
            speakerText.text = entry.speaker ?? string.Empty;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = entry.portrait;
            portraitImage.enabled = !string.IsNullOrWhiteSpace(entry.speaker) && entry.portrait != null;
        }

        SetPortraitUiVisible(!string.IsNullOrWhiteSpace(entry.speaker));

        dialogueText.text = entry.text ?? string.Empty;
        dialogueText.ForceMeshUpdate();

        int totalCharacters = dialogueText.textInfo.characterCount;
        dialogueText.maxVisibleCharacters = 0;

        for (int visible = 1; visible <= totalCharacters; visible++)
        {
            dialogueText.maxVisibleCharacters = visible;
            yield return new WaitForSecondsRealtime(letterDelaySeconds);
        }
    }

    private void EndDialogueState(bool completedNaturally)
    {
        string finishedSequenceId = activeSequenceId;
        activeSequenceId = null;

        Time.timeScale = cachedTimeScale;
        IsPlaying = false;

        RestoreBossUiAfterDialogue();

        SetDialogueUiVisible(false);

        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
            dialogueText.maxVisibleCharacters = 0;
        }

        if (speakerText != null)
        {
            speakerText.text = string.Empty;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
        }

        SetPortraitUiVisible(false);

        if (completedNaturally && ShouldForceShowBossUiAfterSecondDialogue(finishedSequenceId))
        {
            ForceShowAllBossUi();
        }

        if (completedNaturally && ShouldAutoPlaySecondBossDialogue(finishedSequenceId))
        {
            StartCoroutine(PlayBossSceneSecondDialogueNextFrame());
        }
    }

    private bool ShouldAutoPlaySecondBossDialogue(string finishedSequenceId)
    {
        if (!autoPlayBossSceneSecondDialogueAfterFirst)
        {
            return false;
        }

        if (!useBossSceneDialogueSelection || !IsBossSceneActive())
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(bossSceneFirstDialogueId) || string.IsNullOrWhiteSpace(bossSceneSecondDialogueId))
        {
            return false;
        }

        if (string.Equals(bossSceneFirstDialogueId, bossSceneSecondDialogueId, System.StringComparison.Ordinal))
        {
            return false;
        }

        return string.Equals(finishedSequenceId, bossSceneFirstDialogueId, System.StringComparison.Ordinal);
    }

    private void TryBindBossHealthEvents()
    {
        if (isBossHealthSubscribed)
        {
            return;
        }

        if (bossHealth == null)
        {
            bossHealth = FindObjectOfType<BossHealth>(true);
        }

        if (bossHealth == null)
        {
            return;
        }

        bossHealth.OnAfterlifeStateChanged += HandleBossAfterlifeStateChanged;
        isBossHealthSubscribed = true;
    }

    private void UnbindBossHealthEvents()
    {
        if (!isBossHealthSubscribed || bossHealth == null)
        {
            return;
        }

        bossHealth.OnAfterlifeStateChanged -= HandleBossAfterlifeStateChanged;
        isBossHealthSubscribed = false;
    }

    private void HandleBossAfterlifeStateChanged(bool isInAfterlife)
    {
        if (!playDialogueAfterBossFirstPhaseEnds || !isInAfterlife)
        {
            return;
        }

        if (triggerBossFirstPhaseEndedDialogueOnlyOnce && bossFirstPhaseDialogueTriggered)
        {
            return;
        }

        bossFirstPhaseDialogueTriggered = true;

        if (IsPlaying)
        {
            pendingBossFirstPhaseEndedDialogue = true;
            return;
        }

        PlayBossFirstPhaseEndedDialogue();
    }

    private bool ShouldForceShowBossUiAfterSecondDialogue(string finishedSequenceId)
    {
        if (!forceShowBossUiAfterSecondBossDialogue)
        {
            return false;
        }

        if (!useBossSceneDialogueSelection || !IsBossSceneActive())
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(bossSceneSecondDialogueId))
        {
            return false;
        }

        return string.Equals(finishedSequenceId, bossSceneSecondDialogueId, System.StringComparison.Ordinal);
    }

    private void ForceShowAllBossUi()
    {
        foreach (BossUIHandler handler in GetAllBossUiHandlers())
        {
            if (handler != null)
            {
                handler.SetBossUiVisible(true);
            }
        }
    }

    private IEnumerator PlayBossSceneSecondDialogueNextFrame()
    {
        // Let cleanup finish before starting the next sequence.
        yield return null;

        if (!IsPlaying)
        {
            PlayBossSceneSecondDialogue();
        }
    }

    private void HideBossUiForDialogue()
    {
        bossUiHandlersToRestore.Clear();
        uiRootActiveStateBeforeDialogue.Clear();

        foreach (BossUIHandler handler in GetAllBossUiHandlers())
        {
            if (handler == null)
            {
                continue;
            }

            if (handler.IsBossUiVisible)
            {
                bossUiHandlersToRestore.Add(handler);
            }

            handler.SetBossUiVisible(false);
        }

        if (additionalUiRootsToHideDuringDialogue == null)
        {
            return;
        }

        for (int i = 0; i < additionalUiRootsToHideDuringDialogue.Length; i++)
        {
            GameObject uiRoot = additionalUiRootsToHideDuringDialogue[i];
            if (uiRoot == null)
            {
                continue;
            }

            if (!uiRootActiveStateBeforeDialogue.ContainsKey(uiRoot))
            {
                uiRootActiveStateBeforeDialogue.Add(uiRoot, uiRoot.activeSelf);
            }

            if (ShouldKeepUiRootVisible(uiRoot))
            {
                uiRoot.SetActive(true);
                continue;
            }

            uiRoot.SetActive(false);
        }
    }

    private void RestoreBossUiAfterDialogue()
    {
        for (int i = 0; i < bossUiHandlersToRestore.Count; i++)
        {
            BossUIHandler handler = bossUiHandlersToRestore[i];
            if (handler != null)
            {
                handler.SetBossUiVisible(true);
            }
        }

        bossUiHandlersToRestore.Clear();

        foreach (KeyValuePair<GameObject, bool> uiRootState in uiRootActiveStateBeforeDialogue)
        {
            GameObject uiRoot = uiRootState.Key;
            if (uiRoot != null)
            {
                uiRoot.SetActive(uiRootState.Value);
            }
        }

        uiRootActiveStateBeforeDialogue.Clear();
    }

    private void ForceHideBossUiWhileDialogue()
    {
        foreach (BossUIHandler handler in GetAllBossUiHandlers())
        {
            if (handler != null)
            {
                handler.SetBossUiVisible(false);
            }
        }

        if (additionalUiRootsToHideDuringDialogue == null)
        {
            return;
        }

        for (int i = 0; i < additionalUiRootsToHideDuringDialogue.Length; i++)
        {
            GameObject uiRoot = additionalUiRootsToHideDuringDialogue[i];
            if (uiRoot == null)
            {
                continue;
            }

            if (ShouldKeepUiRootVisible(uiRoot))
            {
                uiRoot.SetActive(true);
                continue;
            }

            uiRoot.SetActive(false);
        }
    }

    private bool ShouldKeepUiRootVisible(GameObject uiRoot)
    {
        return uiRoot != null && uiRootsAllowedDuringDialogue.Contains(uiRoot);
    }

    private IEnumerable<BossUIHandler> GetAllBossUiHandlers()
    {
        HashSet<BossUIHandler> uniqueHandlers = new HashSet<BossUIHandler>();

        if (bossUIHandler != null)
        {
            uniqueHandlers.Add(bossUIHandler);
        }

        if (additionalBossUiHandlers != null)
        {
            for (int i = 0; i < additionalBossUiHandlers.Length; i++)
            {
                BossUIHandler extraHandler = additionalBossUiHandlers[i];
                if (extraHandler != null)
                {
                    uniqueHandlers.Add(extraHandler);
                }
            }
        }

        BossUIHandler[] discoveredHandlers = FindObjectsOfType<BossUIHandler>(true);
        for (int i = 0; i < discoveredHandlers.Length; i++)
        {
            BossUIHandler discoveredHandler = discoveredHandlers[i];
            if (discoveredHandler != null)
            {
                uniqueHandlers.Add(discoveredHandler);
            }
        }

        foreach (BossUIHandler handler in uniqueHandlers)
        {
            yield return handler;
        }
    }

    private void SetPortraitUiVisible(bool isVisible)
    {
        if (portraitFrame != null)
        {
            portraitFrame.SetActive(isVisible);
        }

        if (portraitPanel != null)
        {
            portraitPanel.SetActive(isVisible);
        }
    }

    private void SetDialogueUiVisible(bool isVisible)
    {
        if (disableDialoguePanelObjectWhenHidden)
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(isVisible);
            }

            return;
        }

        if (dialogueVisualRoot != null)
        {
            dialogueVisualRoot.SetActive(isVisible);
        }
        else
        {
            if (speakerText != null)
            {
                speakerText.gameObject.SetActive(isVisible);
            }

            if (dialogueText != null)
            {
                dialogueText.gameObject.SetActive(isVisible);
            }
        }

        if (additionalDialogueObjectsToToggle != null)
        {
            for (int i = 0; i < additionalDialogueObjectsToToggle.Length; i++)
            {
                GameObject targetObject = additionalDialogueObjectsToToggle[i];
                if (targetObject != null)
                {
                    targetObject.SetActive(isVisible);
                }
            }
        }
    }
}
