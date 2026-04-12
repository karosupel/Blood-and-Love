using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuccubiRetreat : EnemyBaseState
{
    public GameObject player;
    public EnemyStats stats;
    public override void EnterState(EnemyStateManager enemy)
    {
        //Debug.Log("Entered Retreat State");
        player = enemy.player;
        stats = enemy.stats;
    }

    public override void UpdateState(EnemyStateManager enemy)
    {
        Vector2 direction = (enemy.transform.position - player.transform.position).normalized;
        enemy.transform.position += (Vector3)direction * Time.deltaTime * enemy.stats.moveSpeed;
        if (direction[0] < 0)
        {
            enemy.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            enemy.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        if (Vector2.Distance(enemy.transform.position, player.transform.position) > enemy.stats.retreatDistance)
        {
            enemy.currentState = enemy.chaseState;
            enemy.currentState.EnterState(enemy);
        }
    }
}
