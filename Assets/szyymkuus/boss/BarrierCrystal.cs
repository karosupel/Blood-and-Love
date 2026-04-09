using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierCrystal : MonoBehaviour, IDamageable
{
    [SerializeField] float maxHealth = 25f;
    float currentHealth;
    BossAbilities owner;
    //bool deathHandled;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void Initialize(BossAbilities boss)
    {
        owner = boss;
    }

    public void TakeDamage(float damage, float knockback = 1f)
    {
        currentHealth -= damage;
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        // if (deathHandled)
        // {
        //     return;
        // }
        // deathHandled = true;
        owner?.OnCrystalDestroyed();
        Destroy(gameObject);
    }
}
