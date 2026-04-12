using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatStateManager : MonoBehaviour
{
    public BatBaseState currentState;
    public BatReposition repositionState = new BatReposition();
    public BatAttacking attackState = new BatAttacking();

    public GameObject player;
    public EnemyStats stats;

    public bool isPlayerAttacked = false;
    Enemy enemy;
    private bool hasStarted = false;

     void Awake()
    {
        enemy = GetComponent<Enemy>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void Start()
    {
        hasStarted = true;
        ResetToRepositionState();
    }

    private void OnEnable()
    {
        if (!hasStarted)
        {
            return;
        }

        if (enemy == null)
        {
            enemy = GetComponent<Enemy>();
        }

        if (enemy != null && enemy.IsDead())
        {
            return;
        }

        ResetToRepositionState();
    }

    public void Update()
    {
        if (enemy.IsDead() || enemy.IsFrozen())
        {
            return;
        }

        currentState.UpdateState(this);
    }

    // Rysowanie range'a odbywa się teraz w UpdateState() przy użyciu LineRenderer
    // zamiast w OnDrawGizmos()

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == 6 && isPlayerAttacked == false)
        {
            StartCoroutine(CollisionAttack());
        }
    }

    public IEnumerator CollisionAttack()
    {
        if(isPlayerAttacked == false)
        {
            isPlayerAttacked = true;
            player.GetComponent<PlayerHealth>().TakeDamage(stats.damage);
            yield return new WaitForSeconds(1f);
            isPlayerAttacked = false;
        }
    }

    private void ResetToRepositionState()
    {
        isPlayerAttacked = false;
        currentState = repositionState;
        currentState.EnterState(this);
    }

    // void OnCollisionEnter2D(Collision2D collision)
    // {
    //     currentState = attackState;
    //     currentState.OnCollisionEnter2DState(this, collision);
    // }
}
