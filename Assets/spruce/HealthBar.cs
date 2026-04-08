using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image liquidFillImage;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 4f;     // lerp speed
    [SerializeField] private Color healthyColor  = new Color(0.2f, 0.8f, 0.3f);
    [SerializeField] private Color damagedColor  = new Color(0.9f, 0.6f, 0.1f);
    [SerializeField] private Color criticalColor = new Color(0.9f, 0.2f, 0.1f);
    [SerializeField] private float fullHpThreshold = 0.97f;
    [SerializeField] private float sparkleSpeed = 8f;
    [SerializeField] private float sparkleStrength = 0.35f;

    private Material _mat;          // instanced copy — never modify the shared asset
    private float    _targetFill;
    private float    _currentFill;
    private bool     _isAtFullHealth;

    // Shader property IDs (cached for performance)
    private static readonly int FillAmountID  = Shader.PropertyToID("_FillAmount");
    private static readonly int LiquidColorID = Shader.PropertyToID("_LiquidColor");
    private static readonly int WaveSpeedID   = Shader.PropertyToID("_WaveSpeed");

    void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }

        if (liquidFillImage == null)
        {
            Debug.LogError("HealthBar: liquidFillImage is not assigned.", this);
            enabled = false;
            return;
        }

        if (liquidFillImage.material == null)
        {
            Debug.LogError("HealthBar: liquidFillImage has no material assigned.", this);
            enabled = false;
            return;
        }

        // Create a per-instance material so multiple bars don't share state
        _mat = Instantiate(liquidFillImage.material);
        liquidFillImage.material = _mat;
        _currentFill = _targetFill = 1f;
    }

    void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += SyncWithPlayerHealth;
            SyncWithPlayerHealth();
        }
    }

    void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= SyncWithPlayerHealth;
        }
    }

    void Update()
    {
        // Smooth the liquid level toward the target
        _currentFill = Mathf.Lerp(_currentFill, _targetFill,
                                   Time.deltaTime * smoothSpeed);
        _mat.SetFloat(FillAmountID, _currentFill);

        // Colour shifts with health level
        Color liquidColor = _currentFill > 0.5f
            ? Color.Lerp(damagedColor,  healthyColor,  (_currentFill - 0.5f) * 2f)
            : Color.Lerp(criticalColor, damagedColor,  _currentFill * 2f);

        // Add a subtle shimmer when health is full.
        if (_isAtFullHealth)
        {
            float sparkle = (Mathf.Sin(Time.time * sparkleSpeed) * 0.5f + 0.5f) * sparkleStrength;
            liquidColor = Color.Lerp(liquidColor, Color.red, sparkle);
        }

        _mat.SetColor(LiquidColorID, liquidColor);

        // Speed up the sloshing when nearly empty for extra drama
        float waveSpeed = Mathf.Lerp(4f, 1.5f, _currentFill*0.02f);

        if (_isAtFullHealth)
        {
            waveSpeed += 0.35f;
        }

        _mat.SetFloat(WaveSpeedID, waveSpeed);
    }

    /// <summary>Set health from 0..maxHealth — call this from your game logic.</summary>
    public void SetHealth(float current, float max)
    {
        if (max <= 0f)
        {
            _targetFill = 0f;
            _isAtFullHealth = false;
            return;
        }

        _targetFill = Mathf.Clamp01(current / max);
        _isAtFullHealth = current >= max;
    }

    private void SyncWithPlayerHealth()
    {
        if (playerHealth == null)
        {
            return;
        }

        SetHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    void OnDestroy()
    {
        if (_mat) Destroy(_mat);
    }
}