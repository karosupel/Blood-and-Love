using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossUIHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TMP_Text heartsText;
    [SerializeField] private GameObject firstPhaseUiRoot;
    [SerializeField] private GameObject secondPhaseUiRoot;

    private void Awake()
    {
        if (bossHealth == null)
        {
            bossHealth = FindObjectOfType<BossHealth>();
        }
    }

    private void OnEnable()
    {
        if (bossHealth == null)
        {
            return;
        }

        bossHealth.OnHealthChanged += HandleHealthChanged;
        bossHealth.OnHeartsChanged += HandleHeartsChanged;
        bossHealth.OnAfterlifeStateChanged += HandleAfterlifeStateChanged;

        HandleHealthChanged();
        HandleHeartsChanged(bossHealth.CurrentHearts);
        HandleAfterlifeStateChanged(bossHealth.IsInAfterlife);
    }

    private void OnDisable()
    {
        if (bossHealth == null)
        {
            return;
        }

        bossHealth.OnHealthChanged -= HandleHealthChanged;
        bossHealth.OnHeartsChanged -= HandleHeartsChanged;
        bossHealth.OnAfterlifeStateChanged -= HandleAfterlifeStateChanged;
    }

    private void HandleHealthChanged()
    {
        if (bossHealth == null)
        {
            return;
        }

        float currentHealth = Mathf.Max(0f, bossHealth.CurrentHealth);
        float maxHealth = Mathf.Max(1f, bossHealth.MaxHealth);

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        }
    }

    private void HandleHeartsChanged(int currentHearts)
    {
        if (heartsText == null || bossHealth == null)
        {
            return;
        }

        heartsText.text = $"{Mathf.Max(0, currentHearts)}";
    }

    private void HandleAfterlifeStateChanged(bool isInAfterlife)
    {
        if (firstPhaseUiRoot != null)
        {
            firstPhaseUiRoot.SetActive(!isInAfterlife);
        }

        if (secondPhaseUiRoot != null)
        {
            secondPhaseUiRoot.SetActive(isInAfterlife);
        }

        // Fallback behavior when dedicated phase roots are not assigned.
        if (firstPhaseUiRoot == null)
        {
            if (healthText != null)
            {
                healthText.gameObject.SetActive(!isInAfterlife);
            }

            if (healthFillImage != null)
            {
                healthFillImage.gameObject.SetActive(!isInAfterlife);
            }
        }

        if (secondPhaseUiRoot == null && heartsText != null)
        {
            heartsText.gameObject.SetActive(isInAfterlife);
        }
    }
}
