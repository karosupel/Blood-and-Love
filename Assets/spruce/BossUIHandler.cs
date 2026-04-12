using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossUIHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private GameObject healthbar;
    [SerializeField] private GameObject hearts;
    [SerializeField] private GameObject bossUiRoot;
    private void Start()
    {
        bossUiRoot.SetActive(true);
        
        hearts.SetActive(bossHealth.IsInAfterlife);
        healthbar.SetActive(!bossHealth.IsInAfterlife);
        healthText.gameObject.SetActive(!bossHealth.IsInAfterlife);

    }
    
    private void Update()
    {
        float currentHealth = bossHealth.CurrentHealth;
        float maxHealth = bossHealth.MaxHealth;

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }

        hearts.SetActive(bossHealth.IsInAfterlife);
        healthbar.SetActive(!bossHealth.IsInAfterlife);
        healthText.gameObject.SetActive(!bossHealth.IsInAfterlife);}       

    }
