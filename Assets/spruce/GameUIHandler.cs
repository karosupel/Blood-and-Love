using UnityEngine;
using TMPro;

public class GameUIHandler : MonoBehaviour
{
    public PlayerHealth PlayerHealth;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private GameObject healthBarRoot;
    [SerializeField] private GameObject afterlifeHeart;

    private void Awake()
    {
        // Try auto-wiring text references from the same GameObject.
        if (healthText == null)
        {
            healthText = GetComponent<TMP_Text>();
        }

        // If not explicitly assigned, use the health text object as fallback root.
        if (healthBarRoot == null && healthText != null)
        {
            healthBarRoot = healthText.gameObject;
        }

    }

    private void OnEnable()
    {
        if (PlayerHealth != null)
        {
            PlayerHealth.OnHealthChanged += HealthChanged;
            PlayerHealth.OnAfterlifeStateChanged += AfterlifeStateChanged;
        }

        HealthChanged();
        AfterlifeStateChanged(PlayerHealth != null && PlayerHealth.IsInAfterlife);
    }

    private void OnDisable()
    {
        if (PlayerHealth != null)
        {
            PlayerHealth.OnHealthChanged -= HealthChanged;
            PlayerHealth.OnAfterlifeStateChanged -= AfterlifeStateChanged;
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

    void AfterlifeStateChanged(bool isInAfterlife)
    {
        SetUiVisible(healthBarRoot, !isInAfterlife);

        if (healthBarRoot == null && healthText != null)
        {
            SetUiVisible(healthText.gameObject, !isInAfterlife);
        }

        SetUiVisible(afterlifeHeart, isInAfterlife);
    }

    void SetUiVisible(GameObject target, bool isVisible)
    {
        if (target != null && target.activeSelf != isVisible)
        {
            target.SetActive(isVisible);
        }
    }
}