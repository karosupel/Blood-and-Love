using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    // TODO: take damage, knockback, die, health bars

    [SerializeField] public EnemyStats stats;
    [SerializeField] public float currentHealth;
    public Rigidbody2D rb;
    [SerializeField] int playerLayerIndex = 6;

    private void Awake()
    {
        currentHealth = stats.health;
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemy took " + damage);
        Knockback(Vector2.up, stats.knockbackForce);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("Enemy died");
        Destroy(gameObject);
    }

    public void Knockback(Vector2 direction, float force)
    {
        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }

    public void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Mouse0))
        // {
        //     TakeDamage(10f);
        // }
    }

    public void DealDamage(GameObject player, float damage)
    {
        if (player.layer != playerLayerIndex)
        {
            Debug.Log("WrongLayer! player layer: " + player.layer + " , expected: " + playerLayerIndex);
            return;
        }
        Debug.Log("Enemy deals damage");
        player.GetComponent<IDamageable>()?.TakeDamage(damage);
    }
}

