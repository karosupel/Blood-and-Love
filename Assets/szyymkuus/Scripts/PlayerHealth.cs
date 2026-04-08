using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{

    [SerializeField] float maxHealth = 100f;
    [SerializeField] float panicMaxHealth = 0.25f;
    [SerializeField] int hearts = 0;
    [SerializeField] float currentHealth;
    [SerializeField] Vector3 hellOffset = new Vector3(-30f, 0f, 0f);
    PlayerAbilities playerAbilities;
    bool isInAfterlife = false;

    public float MaxHealth => maxHealth;

    public float CurrentHealth => currentHealth;
    private Vector3 deathPlace;


    void Awake()
    {
        playerAbilities = GetComponent<PlayerAbilities>();
        currentHealth = maxHealth;
    }

    public void Die()
    {
        Debug.Log("Player has died! Fight for your life!");
        GoToHell();
    }

    public void GoToHell()
    {
        deathPlace = transform.position;
        isInAfterlife = true;
        transform.position = transform.position + hellOffset;

    }
    public void GoToMaterialPlane()
    {
        currentHealth = 0.3f * maxHealth;
        isInAfterlife = false;
        transform.position = deathPlace;
        playerAbilities.UseUltimate(false);
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
        if (!isInAfterlife)
        {
                currentHealth -= damage;
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
        else
        {
            TakeHeart();
            //implement temporary invulnerability
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
