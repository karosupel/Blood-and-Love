using TMPro;
using UnityEngine;

public class BossUIHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private GameObject healthbar;
    [SerializeField] private GameObject hearts;
    [SerializeField] private GameObject bossUiRoot;

    private bool forceHidden;

    public bool IsBossUiVisible
    {
        get
        {
            if (bossUiRoot != null)
            {
                return bossUiRoot.activeSelf;
            }

            if (!HasExplicitUiReferences())
            {
                return gameObject.activeSelf;
            }

            bool isHealthTextVisible = healthText != null && healthText.gameObject.activeSelf;
            bool isHealthbarVisible = healthbar != null && healthbar.activeSelf;
            bool isHeartsVisible = hearts != null && hearts.activeSelf;
            return isHealthTextVisible || isHealthbarVisible || isHeartsVisible;
        }
    }

    public void SetBossUiVisible(bool isVisible)
    {
        forceHidden = !isVisible;
        ApplyVisibility();
    }

    private void Start()
    {
        ApplyVisibility();
    }

    private void Update()
    {
        if (forceHidden)
        {
            ApplyVisibility();
            return;
        }

        if (bossHealth != null)
        {
            float currentHealth = bossHealth.CurrentHealth;
            float maxHealth = bossHealth.MaxHealth;

            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
            }
        }

        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        bool shouldShow = !forceHidden;

        if (bossUiRoot != null)
        {
            bossUiRoot.SetActive(shouldShow);
        }
        else if (!HasExplicitUiReferences())
        {
            gameObject.SetActive(shouldShow);
        }

        if (!shouldShow)
        {
            if (hearts != null)
            {
                hearts.SetActive(false);
            }

            if (healthbar != null)
            {
                healthbar.SetActive(false);
            }

            if (healthText != null)
            {
                healthText.gameObject.SetActive(false);
            }

            return;
        }

        if (bossHealth == null)
        {
            return;
        }

        bool isInAfterlife = bossHealth.IsInAfterlife;

        if (hearts != null)
        {
            hearts.SetActive(isInAfterlife);
        }

        if (healthbar != null)
        {
            healthbar.SetActive(!isInAfterlife);
        }

        if (healthText != null)
        {
            healthText.gameObject.SetActive(!isInAfterlife);
        }
    }

    private bool HasExplicitUiReferences()
    {
        return healthText != null || healthbar != null || hearts != null;
    }
}
