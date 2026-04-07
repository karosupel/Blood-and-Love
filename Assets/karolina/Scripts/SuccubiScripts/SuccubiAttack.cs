using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuccubiAttack : EnemyBaseState
{
    private bool isAttacking = false;
    public EnemyStats stats;
    public GameObject player;
    public override void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("Entered Attack State");
        isAttacking = true;
        player = enemy.player;
        stats = enemy.stats;
    }

    public override void UpdateState(EnemyStateManager enemy)
    {
        if (isAttacking)
        {
            isAttacking = false; // Prevent multiple coroutines from starting
            enemy.StartCoroutine(AttackCoroutine(enemy, stats.attackCooldown)); 
        }
    }

    public IEnumerator AttackCoroutine(EnemyStateManager enemy, float cooldown)
    {
        Debug.Log("Attacking player...");

        PlayerTestScript playerScript = player.GetComponent<PlayerTestScript>();
        playerScript.StartCoroutine(playerScript.Stun(0.8f)); // Stun player for 0.5 seconds

        yield return new WaitForSeconds(cooldown); // Attack every cooldown seconds

        //checks if player is still in range after cooldown, if not, switch back to chase state
        if (Vector2.Distance(enemy.transform.position, player.transform.position) >= stats.attackRange)
        {
            enemy.currentState = enemy.chaseState;
            enemy.currentState.EnterState(enemy);
        }
        else if (Vector2.Distance(enemy.transform.position, player.transform.position) < stats.retreatRange) //player is too close after attack, switch to retreat state
        {
            enemy.currentState = enemy.retreatState;
            enemy.currentState.EnterState(enemy);
        }

        isAttacking = true; // Allow attacking again after cooldown if player is still in range
    }

}
