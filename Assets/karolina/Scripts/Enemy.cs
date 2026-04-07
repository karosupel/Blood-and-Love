using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    // TODO: take damage, knockback, die, health bars

    [SerializeField] public EnemyStats stats;
    [SerializeField] public float currentHealth;

    private void Awake()
    {
        currentHealth = stats.health;
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemy took " + damage);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("Enemy died");
        //Destroy(gameObject);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            TakeDamage(10f);
        }
    }
}
