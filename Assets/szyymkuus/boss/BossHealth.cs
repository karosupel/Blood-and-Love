using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealth : MonoBehaviour, IDamageable
{

    [SerializeField] public float maxHealth = 200f;
    public float currentHealth;
    float isInvincible;
    GameObject player;


    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage, float knockback)
    {
        currentHealth -= damage;
        Debug.Log("Boss HP: " + currentHealth + "/" + maxHealth);
        if (currentHealth <= damage)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("Boss died! Remember to add second phase");
        Destroy(gameObject);
    }
}
