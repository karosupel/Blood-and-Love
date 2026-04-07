using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    // TODO: take damage, knockback, die, health bars

    [SerializeField] private float health; //will be overwritten in scripts later

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log("Enemy took " + damage);
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("Enemy died");
        //Destroy(gameObject);
    }

}
