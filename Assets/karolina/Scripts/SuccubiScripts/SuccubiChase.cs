using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuccubiChase : EnemyBaseState
{   
    private GameObject player;
    private EnemyStats stats;
    public override void EnterState(EnemyStateManager enemy)
    {
        //Debug.Log("Entered Chase State");
        player = enemy.player;
        stats = enemy.stats;
    }

    public override void UpdateState(EnemyStateManager enemy)
    {
        if(player == null)
        {
            return;
        }
        Vector2 direction = (player.transform.position - enemy.transform.position).normalized;
        enemy.transform.position += (Vector3)direction * Time.deltaTime * stats.moveSpeed;
        if (direction[0] < 0)
        {
            enemy.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            enemy.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        if (Vector2.Distance(enemy.transform.position, player.transform.position) < stats.attackRange) //attacks when player is within attack range
        {
            enemy.currentState = enemy.attackState;
            enemy.currentState.EnterState(enemy);
        }

        if(Vector2.Distance(enemy.transform.position, player.transform.position) < stats.retreatRange) //if player is too close, switch to retreat state
        {
            enemy.currentState = enemy.retreatState;
            enemy.currentState.EnterState(enemy);
        }
    }


}
