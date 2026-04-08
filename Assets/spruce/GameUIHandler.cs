using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIHandler : MonoBehaviour
{
    public PlayerHealth PlayerHealth;
    [SerializeField] private TMP_Text healthText;
    private void Awake()
    {
        // Try auto-wiring text references from the same GameObject.
        if (healthText == null)
        {
            healthText = GetComponent<TMP_Text>();
        }

    }

    private void OnEnable()
    {
        if (PlayerHealth != null)
        {
            PlayerHealth.OnHealthChanged += HealthChanged;
        }

        HealthChanged();
    }

    private void OnDisable()
    {
        if (PlayerHealth != null)
        {
            PlayerHealth.OnHealthChanged -= HealthChanged;
        }
    }


    void HealthChanged()
    {
        if (PlayerHealth == null)
        {
            return;
        }

        string hpText = $"{Mathf.CeilToInt(PlayerHealth.CurrentHealth)}/{Mathf.CeilToInt(PlayerHealth.MaxHealth)}";

        if (healthText != null)
        {
            healthText.text = hpText;
        }
    }
}