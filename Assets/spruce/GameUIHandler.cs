using UnityEngine;
using UnityEngine.UIElements;

public class GameUIHandler : MonoBehaviour
{
    public PlayerHealth PlayerHealth;
    public UIDocument UIDoc;
    private Label m_HealthLabel;


    private void Start()
    {
        m_HealthLabel = UIDoc.rootVisualElement.Q<Label>("HealthLabel");

        if (PlayerHealth != null)
        {
            PlayerHealth.OnHealthChanged += HealthChanged;
        }

        HealthChanged();
    }

    private void OnDestroy()
    {
        if (PlayerHealth != null)
        {
            PlayerHealth.OnHealthChanged -= HealthChanged;
        }
    }


    void HealthChanged()
    {
        if (m_HealthLabel == null || PlayerHealth == null)
        {
            return;
        }

        m_HealthLabel.text = $"{PlayerHealth.CurrentHealth}/{PlayerHealth.MaxHealth}";
    }
}