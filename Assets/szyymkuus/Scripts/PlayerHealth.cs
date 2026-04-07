using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{

    [SerializeField] float maxHealth = 100f;
    [SerializeField] int hearts = 0;
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
