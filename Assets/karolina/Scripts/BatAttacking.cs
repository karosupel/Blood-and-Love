using System.Collections;
using UnityEngine;

public class BatAttacking : BatBaseState
{
    GameObject player;
    EnemyStats stats;

    Coroutine attackCoroutine;

    private bool showAttackRange = false;
    private float attackRangeTimer;

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
        // Zmniejszaj timer dla range preview
        if (showAttackRange)
        {
            attackRangeTimer -= Time.deltaTime;
            if (attackRangeTimer <= 0)
            {
                showAttackRange = false;
            }
        }
    }

    IEnumerator AttackPlayer(BatStateManager enemy)
    {
        // Pokaż range ataku przez 0.5 sekund
        showAttackRange = true;
        attackRangeTimer = 0.5f;
        yield return new WaitForSeconds(0.5f);

        // Sprawdź czy gracz nadal jest w zasięgu
        if (Vector2.Distance(enemy.transform.position, player.transform.position) > stats.attackRange)
        {
            enemy.currentState = enemy.repositionState;
            enemy.currentState.EnterState(enemy);
            yield break;
        }

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
        if (player == null || stats == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemy.transform.position, stats.attackRange);
    }

    public bool ShouldShowAttackRange()
    {
        return showAttackRange;
    }

    public void OnCollisionEnter2DState(BatStateManager enemy, Collision2D collision)
    {
        if (collision.gameObject == player)
        {
            player.GetComponent<PlayerHealth>().TakeDamage(stats.damage);
        }
    }
}