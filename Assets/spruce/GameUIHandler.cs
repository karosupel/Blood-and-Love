using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUIHandler : MonoBehaviour
{
    public PlayerHealth PlayerHealth;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private GameObject healthBarRoot;
    [SerializeField] private GameObject afterlifeHeart;
    [SerializeField] private TMP_Text heartText;
    [SerializeField] private GameObject borders;
    [SerializeField] private Color materialPlaneBorderColor = new Color32(14, 25, 17, 255);
    [SerializeField] private Color afterlifeBorderColor = Color.red;

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

        if (heartText == null && afterlifeHeart != null)
        {
            heartText = afterlifeHeart.GetComponentInChildren<TMP_Text>(true);
        }

    }

    private void OnEnable()
    {
        if (PlayerHealth != null)
        {
            PlayerHealth.OnHealthChanged += HealthChanged;
            PlayerHealth.OnAfterlifeStateChanged += AfterlifeStateChanged;
            PlayerHealth.OnHeartsChanged += HeartsChanged;
        }

        HealthChanged();
        HeartsChanged(PlayerHealth != null ? PlayerHealth.Hearts : 0);
        AfterlifeStateChanged(PlayerHealth != null && PlayerHealth.IsInAfterlife);
    }

    private void OnDisable()
    {
        if (PlayerHealth != null)
        {
            PlayerHealth.OnHealthChanged -= HealthChanged;
            PlayerHealth.OnAfterlifeStateChanged -= AfterlifeStateChanged;
            PlayerHealth.OnHeartsChanged -= HeartsChanged;
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

        SetBorderChildrenColor(isInAfterlife ? afterlifeBorderColor : materialPlaneBorderColor);
        

        if (healthBarRoot == null && healthText != null)
        {
            SetUiVisible(healthText.gameObject, !isInAfterlife);
        }

        SetUiVisible(afterlifeHeart, isInAfterlife);
        if (heartText != null)
        {
            SetUiVisible(heartText.gameObject, isInAfterlife);
        }
    }

    void HeartsChanged(int hearts)
    {
        if (heartText != null)
        {
            heartText.text = hearts.ToString();
        }
    }

    void SetBorderChildrenColor(Color targetColor)
    {
        if (borders == null)
        {
            return;
        }

        Transform borderRoot = borders.transform;
        for (int i = 0; i < borderRoot.childCount; i++)
        {
            Transform child = borderRoot.GetChild(i);

            if (child.TryGetComponent<Image>(out Image image))
            {
                image.color = targetColor;
                continue;
            }

            if (child.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.color = targetColor;
            }
        }
    }

    void SetUiVisible(GameObject target, bool isVisible)
    {
        if (target != null && target.activeSelf != isVisible)
        {
            target.SetActive(isVisible);
        }
    }
}