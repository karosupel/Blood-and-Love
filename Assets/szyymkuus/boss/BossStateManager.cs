using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStateManager : MonoBehaviour
{
    public BossBaseState currentState;
    public BossChase chaseState = new BossChase();
    public BossAttack attackState = new BossAttack();

    public GameObject player;
    public EnemyStats stats;

     void Awake()
    {
        Enemy enemy = GetComponent<Enemy>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void Start()
    {
        currentState = chaseState;
        currentState.EnterState(this);
    }

    public void Update()
    {
        currentState.UpdateState(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(this.transform.position, this.transform.position + (player.transform.position - this.transform.position).normalized * stats.attackRange);
    }
}
