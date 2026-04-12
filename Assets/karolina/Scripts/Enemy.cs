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
    Animator animator;
    bool isDead = false;
    bool isFrozen = false;
    private bool hasScheduledUnfreeze = false;
    private float scheduledUnfreezeRealtime = -1f;

    //public Animator animator;

    private void Awake()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        currentHealth = stats.health;
        animator = GetComponent<Animator>();
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }
    public void TakeDamage(float damage, float knockback = 1f)
    {
        if (isDead)
        {
            return;
        }
        currentHealth -= damage;
        StartCoroutine(QuickRecolor());
        Debug.Log("Enemy took " + damage);
        Knockback((Vector2)(gameObject.transform.position - playerObject.transform.position).normalized, stats.knockbackForce*knockback);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public IEnumerator QuickRecolor()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            yield break;
        }

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;


    }

    public void Die()
    {
        
        isDead = true;
        //Debug.Log("Enemy died");
        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }
        else
        {
            Destroy(gameObject);
        }

    }
    public void DestroyEnemy() // przeniesione z Die() do animacji, żeby można było dodać efekt pośmiertny
    {
        Destroy(gameObject);
    }

    public void Knockback(Vector2 direction, float force)
    {
        if (isDead)
        {
            return;
        }
        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }

    public void Update()
    {
        TryUnfreezeIfDue();
    }

    private void OnEnable()
    {
        TryUnfreezeIfDue();
    }

    public void DealDamage(GameObject player, float damage)
    {
        if (isDead)        {
            return;
        }
        if (player.layer != playerLayerIndex)
        {
            //Debug.Log("WrongLayer! player layer: " + player.layer + " , expected: " + playerLayerIndex);
            return;
        }
        //Debug.Log("Enemy deals damage");
        player.GetComponent<IDamageable>()?.TakeDamage(damage);
    }

    public bool IsDead()
    {
        return isDead;
    }


    public void Freeze(bool freeze)
    {
        isFrozen = freeze;

        if (freeze && rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (!freeze)
        {
            hasScheduledUnfreeze = false;
            scheduledUnfreezeRealtime = -1f;
        }
    }

    public void FreezeFor(float duration)
    {
        Freeze(true);

        hasScheduledUnfreeze = true;
        scheduledUnfreezeRealtime = Time.realtimeSinceStartup + Mathf.Max(0f, duration);
        TryUnfreezeIfDue();
    }

    private void TryUnfreezeIfDue()
    {
        if (!isFrozen || !hasScheduledUnfreeze)
        {
            return;
        }

        if (Time.realtimeSinceStartup >= scheduledUnfreezeRealtime)
        {
            Freeze(false);
        }
    }

    public bool IsFrozen()
    {
        return isFrozen;
    }
}


