using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaidStateManager : MonoBehaviour
{
    public MaidBaseState currentState;
    public MaidChase chaseState = new MaidChase();
    public MaidAttack attackState = new MaidAttack();

    public Animator animator;

    public GameObject player;
    public EnemyStats stats;
    Enemy enemy;

     void Awake()
    {
        enemy = GetComponent<Enemy>();
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
    }

    public void Start()
    {
        currentState = chaseState;
        currentState.EnterState(this);
    }

    public void Update()
    {
        if (enemy.IsDead())
        {
            return;
        }
        currentState.UpdateState(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(this.transform.position, this.transform.position + (player.transform.position - this.transform.position).normalized * stats.attackRange);
    }
}
