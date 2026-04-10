using System.Collections;
using UnityEngine;

public class BatAttacking : BatBaseState
{
    GameObject player;
    EnemyStats stats;

    Coroutine attackCoroutine;

    public override void EnterState(BatStateManager enemy)
    {
        player = enemy.player;
        stats = enemy.stats;

        attackCoroutine = enemy.StartCoroutine(AttackPlayer(enemy));
    }

    public override void ExitState(BatStateManager enemy)
    {
        if (attackCoroutine != null)
        {
            enemy.StopCoroutine(attackCoroutine);
        }
    }

    public override void UpdateState(BatStateManager enemy)
    {
        // brak logiki w Update — wszystko w coroutine
    }

    IEnumerator AttackPlayer(BatStateManager enemy)
    {
        // małe opóźnienie przed dash
        yield return new WaitForSeconds(0.3f);

        // zapamiętaj pozycję gracza (dash w konkretny punkt)
        Vector2 dashTarget = player.transform.position;

        float dashTime = 1f;
        float timer = 0f;
        bool damageDealt = false;

        while (timer < dashTime)
        {
            enemy.transform.position = Vector2.MoveTowards(
                enemy.transform.position,
                dashTarget,
                stats.moveSpeed * Time.deltaTime
            );

            // Sprawdzaj czy znowu w zasięgu (może gracz się poruszył)
            if (!damageDealt && Vector2.Distance(enemy.transform.position, player.transform.position) < stats.attackRange)
            {
                player.GetComponent<PlayerHealth>().TakeDamage(stats.damage);
                damageDealt = true;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // cooldown po ataku
        yield return new WaitForSeconds(stats.attackCooldown);

        // zmiana stanu (BEZ ExitState tutaj!)
        enemy.currentState = enemy.repositionState;
        enemy.currentState.EnterState(enemy);
    }

    public void DrawAttackRangeGizmo(BatStateManager enemy)
    {
        if (player == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(enemy.transform.position, player.transform.position);
    }

#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
    public void OnCollisionEnter2DState(BatStateManager enemy, Collision2D collision)
#pragma warning restore CS0114 // Member hides inherited member; missing override keyword
    {
        if (collision.gameObject == player)
        {
            player.GetComponent<PlayerHealth>().TakeDamage(stats.damage);
        }
    }
}