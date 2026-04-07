using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaidAttack : MaidBaseState
{
    public GameObject player;
    public EnemyStats stats;

    public override void EnterState(MaidStateManager enemy)
    {
        Debug.Log("Entered Attack State");
        player = enemy.player;
        stats = enemy.stats;
    }

    public override void UpdateState(MaidStateManager enemy)
    {
        Vector2 direction = (enemy.transform.position - player.transform.position).normalized;
        enemy.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);

        if (Vector2.Distance(enemy.transform.position, player.transform.position) > enemy.stats.attackRange)
        {
            enemy.currentState = enemy.chaseState;
            enemy.currentState.EnterState(enemy);
        }
    }
}
