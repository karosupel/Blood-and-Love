using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{

    [SerializeField] float maxHealth = 100f;
    [SerializeField] float currentHealth;

    public float MaxHealth => maxHealth;

    public float CurrentHealth => currentHealth;


    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void Die()
    {
        Debug.Log("Player has died!");
        Destroy(gameObject);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log("Player took damage, current health: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Update()
    {
        //Debug.Log("Current Health: " + currentHealth);
    }
}
