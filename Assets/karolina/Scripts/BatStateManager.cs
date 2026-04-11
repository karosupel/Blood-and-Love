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

     void Awake()
    {
        Enemy enemy = GetComponent<Enemy>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void Start()
    {
        currentState = repositionState;
        currentState.EnterState(this);
    }

    public void Update()
    {
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

    // void OnCollisionEnter2D(Collision2D collision)
    // {
    //     currentState = attackState;
    //     currentState.OnCollisionEnter2DState(this, collision);
    // }
}
