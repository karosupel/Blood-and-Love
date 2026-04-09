using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuccubiAttack : EnemyBaseState
{
    private bool isAttacking = false;
    public EnemyStats stats;
    public GameObject player;
    public Enemy enemyReference;

    public float attackAngle = 45f;
    
    public override void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("Entered Attack State");
        isAttacking = true;
        player = enemy.player;
        stats = enemy.stats;
        enemyReference = enemy.enemy;
    }

    public override void UpdateState(EnemyStateManager enemy)
    {
        if (isAttacking && player != null)
        {
            isAttacking = false; // Prevent multiple coroutines from starting
            enemy.StartCoroutine(AttackCoroutine(enemy, stats.attackCooldown));
        }
    }

    public IEnumerator AttackCoroutine(EnemyStateManager enemy, float cooldown)
    {
        if (player == null)
        {
            yield break; // Exit if player reference is lost
        }
        
        enemyReference.DealDamage(player, stats.damage);
        player.GetComponent<IConditionable>()?.Stun(1f); //hard fixed stun

        yield return new WaitForSeconds(cooldown); // Attack every cooldown seconds

        //checks if player is still in range after cooldown, if not, switch back to chase state
        if (!CalculateAttackRange(enemy, attackAngle))
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

    bool CalculateAttackRange(EnemyStateManager enemy, float attackAngle)
    {
        if (player == null)
        {
            return false;
        }

        Vector2 directionToPlayer = (player.transform.position - enemy.transform.position).normalized;

        Vector2 forward = enemy.transform.up;

        float angle = Vector2.Angle(forward, directionToPlayer);
        float distance = Vector2.Distance(enemy.transform.position, player.transform.position);

        bool inAngle = angle <= attackAngle / 2f;
        bool inRange = distance <= stats.attackRange;

        Debug.Log($"Angle: {angle}");

        return inAngle && inRange;
    }

    private Vector2 lastDirection = Vector2.right;

    public void DrawAttackRange(EnemyStateManager enemy, float attackAngle)
    {
        if (enemyReference == null) return;

        Transform t = enemyReference.transform;

        Gizmos.color = Color.red;

        Vector2 forward = t.up;

        float halfAngle = attackAngle / 2f;

        Vector2 leftDir = Quaternion.Euler(0, 0, -halfAngle) * forward;
        Vector2 rightDir = Quaternion.Euler(0, 0, halfAngle) * forward;

        Gizmos.DrawLine(t.position, t.position + (Vector3)(leftDir * stats.attackRange));
        Gizmos.DrawLine(t.position, t.position + (Vector3)(rightDir * stats.attackRange));

        int segments = 20;
        Vector3 prevPoint = t.position + (Vector3)(leftDir * stats.attackRange);

        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngle + (attackAngle / segments) * i;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * forward;
            Vector3 newPoint = t.position + (Vector3)(dir * stats.attackRange);

            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

}
