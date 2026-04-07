using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuccubiRetreat : EnemyBaseState
{
    public GameObject player;
    public EnemyStats stats;
    public override void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("Entered Retreat State");
        player = enemy.player;
        stats = enemy.stats;
    }

    public override void UpdateState(EnemyStateManager enemy)
    {
        Vector2 direction = (enemy.transform.position - player.transform.position).normalized;
        enemy.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        enemy.transform.position += (Vector3)direction * Time.deltaTime * enemy.stats.moveSpeed;

        if (Vector2.Distance(enemy.transform.position, player.transform.position) > enemy.stats.retreatDistance)
        {
            enemy.currentState = enemy.chaseState;
            enemy.currentState.EnterState(enemy);
        }
    }
}
