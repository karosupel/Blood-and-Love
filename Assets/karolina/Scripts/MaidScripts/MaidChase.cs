using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaidChase : MaidBaseState
{
    public GameObject player;
    public EnemyStats stats;

    public override void EnterState(MaidStateManager enemy)
    {
        //Debug.Log("Entered Chase State");
        player = enemy.player;
        stats = enemy.stats;
    }

    public override void UpdateState(MaidStateManager enemy)
    {
        if(player == null)
        {
            return;
        }
        Vector2 direction = (player.transform.position - enemy.transform.position).normalized;
        //enemy.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        if(enemy.transform.position.x < player.transform.position.x)
        {
            enemy.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            enemy.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        enemy.transform.position += (Vector3)direction * Time.deltaTime * enemy.stats.moveSpeed;

        if (Vector2.Distance(enemy.transform.position, player.transform.position) < enemy.stats.attackRange)
        {
            enemy.currentState = enemy.attackState;
            enemy.currentState.EnterState(enemy);
        }
    }
}
