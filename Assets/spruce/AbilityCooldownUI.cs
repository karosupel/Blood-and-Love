using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a pixel-art style cooldown overlay on an ability icon.
/// The dark bar moves downward in discrete pixel steps, then the icon
/// color snaps back to normal when the cooldown ends.
/// </summary>
public class AbilityCooldownUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The ability icon Image component.")]
    [SerializeField] private Image iconImage;

    [Tooltip("A child Image that acts as the dark overlay bar.")]
    [SerializeField] private Image overlayImage;

    [Header("Cooldown Visuals")]
    [Tooltip("Tint applied to the icon while on cooldown.")]
    [SerializeField] private Color cooldownTint = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Tooltip("Color of the sweeping overlay bar.")]
    [SerializeField] private Color overlayColor = new Color(0.1f, 0.1f, 0.1f, 0.80f);

    [Header("Pixel Art Settings")]
    [Tooltip("The icon's height in pixels. The bar snaps to steps of this size.")]
    [SerializeField] private int iconPixelHeight = 32;

    // ── Runtime state ──────────────────────────────────────────────────────────
    private RectTransform overlayRect;
    private float cooldownDuration;
    private float cooldownRemaining;
    private bool onCooldown;

    // Cache the last snapped step so we only update the RectTransform when needed
    private int lastSnappedStep = -1;

    // ── Unity lifecycle ────────────────────────────────────────────────────────
    private void Awake()
    {
        overlayRect = overlayImage.rectTransform;
        overlayImage.color = overlayColor;
        overlayImage.raycastTarget = false;

        // Ensure the overlay anchors are correct at runtime as a safety net
        overlayRect.anchorMin = new Vector2(0f, 1f);
        overlayRect.anchorMax = new Vector2(1f, 1f);
        overlayRect.pivot    = new Vector2(0.5f, 1f);

        HideOverlay();
    }

    private void Update()
    {
        if (!onCooldown) return;

        cooldownRemaining -= Time.deltaTime;

        if (cooldownRemaining <= 0f)
        {
            EndCooldown();
            return;
        }

        UpdateOverlay();
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Call this to trigger the cooldown animation.</summary>
    public void StartCooldown(float duration)
    {
        if (duration <= 0f) return;

        cooldownDuration  = duration;
        cooldownRemaining = duration;
        onCooldown        = true;
        lastSnappedStep   = -1; // force a refresh on the first Update

        iconImage.color = cooldownTint;
        UpdateOverlay();
    }

    /// <summary>Immediately cancels the cooldown and restores the icon.</summary>
    public void CancelCooldown()
    {
        EndCooldown();
    }

    /// <summary>Returns a 0–1 value representing cooldown progress (0 = ready).</summary>
    public float GetProgress() =>
        onCooldown ? cooldownRemaining / cooldownDuration : 0f;

    // ── Internal helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Snaps the overlay height to whole-pixel increments, so the bar
    /// moves in discrete steps instead of smoothly — matching pixel art.
    /// </summary>
    private void UpdateOverlay()
    {
        float progress = cooldownRemaining / cooldownDuration;   // 1 → 0

        // Map progress to an integer number of pixels (0 … iconPixelHeight)
        int snappedStep = Mathf.CeilToInt(progress * iconPixelHeight);
        snappedStep = Mathf.Clamp(snappedStep, 0, iconPixelHeight);

        // Only touch the RectTransform when the step actually changes
        if (snappedStep == lastSnappedStep) return;
        lastSnappedStep = snappedStep;

        float iconWorldHeight = overlayRect.parent.GetComponent<RectTransform>().rect.height;
        float pixelWorldSize  = iconWorldHeight / iconPixelHeight;
        float overlayHeight   = snappedStep * pixelWorldSize;

        // Drive the overlay by offsetting the bottom edge downward from the top
        overlayRect.offsetMin = new Vector2(0f, -overlayHeight);
        overlayRect.offsetMax = new Vector2(0f,  0f);

        overlayImage.enabled = snappedStep > 0;
    }

    private void EndCooldown()
    {
        onCooldown        = false;
        cooldownRemaining = 0f;

        iconImage.color = Color.white;  // restore full color
        HideOverlay();
    }

    private void HideOverlay()
    {
        overlayImage.enabled = false;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        lastSnappedStep = 0;
    }
}