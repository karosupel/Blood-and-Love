using UnityEngine;
using UnityEngine.UI;

public class HeartBeat : MonoBehaviour
{
    [Header("Beat Settings")]
    public float beatsPerMinute = 75f;
    public float beatScale = 1.25f;      // Peak scale during a beat
    public float normalScale = 1.0f;     // Resting scale

    [Header("Double-Beat Shape")]
    // A real heartbeat has two quick pulses (lub-dub).
    // These control the gap between the two pulses.
    public float secondBeatDelay = 0.12f;  // Seconds after first beat before second
    public float secondBeatStrength = 0.7f; // Second beat is slightly weaker

    private RectTransform rectTransform;
    private float beatInterval;
    private float timer;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        beatInterval = 60f / beatsPerMinute;
        timer = 0f;
    }

    void Update()
    {
        beatInterval = 60f / beatsPerMinute;
        timer += Time.deltaTime;

        float scale = GetHeartScale(timer % beatInterval, beatInterval);
        rectTransform.localScale = Vector3.one * scale;
    }

    float GetHeartScale(float t, float interval)
    {
        float scale = normalScale;

        // --- First beat (lub) ---
        scale = ApplyPulse(scale, t, 0f, beatScale);

        // --- Second beat (dub) ---
        float secondPeakScale = normalScale + (beatScale - normalScale) * secondBeatStrength;
        scale = ApplyPulse(scale, t, secondBeatDelay, secondPeakScale);

        return scale;
    }

    // Adds a smooth quick-expand / slow-return pulse at timeOffset
    float ApplyPulse(float currentScale, float t, float timeOffset, float peakScale)
    {
        float pulseRise  = 0.045f;   // Seconds to reach peak (snappy)
        float pulseFall  = 0.18f;    // Seconds to return to normal (relaxed)
        float pulseDuration = pulseRise + pulseFall;

        float localT = t - timeOffset;
        if (localT < 0f || localT > pulseDuration) return currentScale;

        float pulseValue;
        if (localT < pulseRise)
            pulseValue = localT / pulseRise;                        // Rise
        else
            pulseValue = 1f - (localT - pulseRise) / pulseFall;    // Fall

        pulseValue = Mathf.SmoothStep(0f, 1f, pulseValue);

        float pulseScale = Mathf.Lerp(normalScale, peakScale, pulseValue);
        return Mathf.Max(currentScale, pulseScale); // Blend both pulses
    }
}