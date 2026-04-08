using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour, IDamageable
{

    public event Action OnHealthChanged;

    [SerializeField] public float maxHealth = 100f;
    [SerializeField] float panicMaxHealth = 0.25f;
    [SerializeField] int hearts = 0;
    [SerializeField] public float currentHealth;
    PlayerAbilities playerAbilities;

    public float MaxHealth => maxHealth;

    public float CurrentHealth => currentHealth;


    void Awake()
    {
        playerAbilities = GetComponent<PlayerAbilities>();
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();
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

        OnHealthChanged?.Invoke();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        OnHealthChanged?.Invoke();

        if (currentHealth <= MaxHealth * panicMaxHealth)
        {
            playerAbilities.LesbianPanic();
        }

        Debug.Log("Player took damage, current health: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeHeart()
    {
        hearts--;
        Debug.Log("Gracz trafiony w zaświatach! pozostało " + hearts + " serc!");
        if (hearts < 0) //do zmiany, zależnie czy gracz kiedy ma 0 serc nadal może żyć
        {
            Annihilate();
        } 
    }

    public void AddHeart()
    {
        hearts++;
        Debug.Log("Gracz zdobył serce! Aktualnie posiada " + hearts);
    }

    public void Annihilate()
    {
        Debug.Log("Player's soul got annihilated!");
        Destroy(gameObject);
    }








    void Update()
    {
        //Debug.Log("Current Health: " + currentHealth);
    }
}
