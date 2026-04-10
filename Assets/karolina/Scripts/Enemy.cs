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
    [SerializeField] GameObject playerObject;

    [SerializeField] public bool isInAfterlife = false;

    [SerializeField] public Color materialPlaneColor;
    [SerializeField] public Color afterlifeColor;

    public Animator animator;

    private void Awake()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        currentHealth = stats.health;
    }
    public void TakeDamage(float damage, float knockback = 1f)
    {
        currentHealth -= damage;
        Debug.Log("Enemy took " + damage);
        Knockback((Vector2)(gameObject.transform.position - playerObject.transform.position).normalized, stats.knockbackForce*knockback);
        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }

    public IEnumerator Die()
    {
        Debug.Log("Enemy died");
        animator.SetBool("isDead",true);
        yield return new WaitForSeconds(1.15f);
        Destroy(gameObject);
    }

    public void Knockback(Vector2 direction, float force)
    {
        rb.AddForce(direction * force, ForceMode2D.Impulse);
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

