using System.Collections;
using TMPro;
using UnityEngine;
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

    [Header("Dialogue Library")]
    [SerializeField] private DialogueSequence[] dialogueLibrary;
    [SerializeField] private string defaultSequenceId;

    [Header("Typewriter")]
    [SerializeField, Min(0.001f)] private float letterDelaySeconds = 0.03f;

    [Header("Flow")]
    [SerializeField] private bool disableDialoguePanelObjectWhenHidden = true;

    private Coroutine playRoutine;
    private float cachedTimeScale = 1f;

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        if (dialoguePanel != null)
        {
            if (disableDialoguePanelObjectWhenHidden)
            {
                dialoguePanel.SetActive(false);
            }
        }

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


    private void OnDisable()
    {
        if (IsPlaying)
        {
            StopDialogue();
        }
    }

    public void PlayDialogue(string sequenceId)
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

        StartSequence(sequence);
    }

    public void PlayDefaultDialogue()
    {
        PlayDialogue(defaultSequenceId);
    }

    public void StopDialogue()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        EndDialogueState();
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

        dialoguePanel.SetActive(true);
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

        EndDialogueState();
        playRoutine = null;
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

    private void EndDialogueState()
    {
        Time.timeScale = cachedTimeScale;
        IsPlaying = false;

        if (dialoguePanel != null)
        {
            if (disableDialoguePanelObjectWhenHidden)
            {
                dialoguePanel.SetActive(false);
            }
        }

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
}
