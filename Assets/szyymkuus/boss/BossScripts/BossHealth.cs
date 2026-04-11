using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BossHealth : MonoBehaviour, IDamageable
{

    public event Action OnHealthChanged;
    public event Action<int> OnHeartsChanged;
    public event Action<bool> OnAfterlifeStateChanged;

    [SerializeField] public float maxHealth = 200f;
    public float currentHealth;
    [SerializeField] public int maxHearts = 5;
    [SerializeField] public float afterlifeInvincibilityDuration = 2f;
    int currentHearts;
    public bool isInvincible;
    GameObject player;
    PlayerHealth playerHealth;
    BossController bossController;
    BossAbilities bossAbilities;
    Animator animator;



    bool isInAfterlife = false;
    Vector3 hellOffset = new Vector3(-30f, 0f, 0f);

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public int MaxHearts => maxHearts;
    public int CurrentHearts => currentHearts;
    public bool IsInAfterlife => isInAfterlife;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
        bossController = GetComponent<BossController>();
        animator = GetComponent<Animator>();
        bossAbilities = GetComponent<BossAbilities>();
        currentHealth = maxHealth;
        currentHearts = maxHearts;
        OnHealthChanged?.Invoke();
        OnHeartsChanged?.Invoke(currentHearts);
        OnAfterlifeStateChanged?.Invoke(isInAfterlife);
    }

    public void TakeDamage(float damage, float knockback)
    {
        if (isInvincible)
        {
            return;
        }
        if (!isInAfterlife)
        {
            currentHealth -= damage;
            OnHealthChanged?.Invoke();
            Debug.Log("Boss HP: " + currentHealth + "/" + maxHealth);
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        else
        {
            TakeHeart();
            //StartCoroutine(InvincibilityCoroutine(afterlifeInvincibilityDuration));
            bossController.ForceNewBarrier();
        }
    }

    void TakeHeart()
    {
        currentHearts--;
        OnHeartsChanged?.Invoke(currentHearts);
        Debug.Log("Boss Hearts left: " + currentHearts + "/" + maxHearts);
        if (currentHearts < 0)
        {
            BeginAnnihilate();
        }
    }

    void BeginAnnihilate()
    {
        animator.SetBool("isAnnihilated", true);
        animator.SetBool("isCastingProjectileStorm", false);
        animator.SetBool("isCastingMeteorStorm", false);
        bossController.pauseCasting = true;
        bossAbilities.BarrierDestroyed();
        bossAbilities.StopAllCoroutines();
        
    }



    void Annihilate()
    {
        Debug.Log("Boss has been annihilated! Now, you can live happily, sure that he won't ever come back!");
        Destroy(gameObject);
    }



     IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

    public void Die()
    {

        Debug.Log("Boss died! But is he really gone?");
        if (playerHealth != null)
        {
            playerHealth.GoToHell();
        }
        GoToHell();
        bossController.EnterSecondPhase();
    }

    void GoToHell()
    {
        if(isInAfterlife)
        {
            Debug.Log("Boss is already in Hell! He can't go there again!");
            return;
        }

        Vector3 hellPosition = transform.position + hellOffset;
        transform.position = hellPosition;
        isInAfterlife = true;
        OnAfterlifeStateChanged?.Invoke(isInAfterlife);
        Debug.Log("Boss has been sent to Hell!");
    }
}
