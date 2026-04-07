using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaidChase : MaidBaseState
{
    public GameObject player;
    public EnemyStats stats;

    public override void EnterState(MaidStateManager enemy)
    {
        Debug.Log("Entered Chase State");
        player = enemy.player;
        stats = enemy.stats;
    }

    public override void UpdateState(MaidStateManager enemy)
    {
        Vector2 direction = (player.transform.position - enemy.transform.position).normalized;
        enemy.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        enemy.transform.position += (Vector3)direction * Time.deltaTime * enemy.stats.moveSpeed;

        if (Vector2.Distance(enemy.transform.position, player.transform.position) < enemy.stats.attackRange)
        {
            enemy.currentState = enemy.attackState;
            enemy.currentState.EnterState(enemy);
        }
    }
}
