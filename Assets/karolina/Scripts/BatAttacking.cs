using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class BatAttacking : BatBaseState
{
    GameObject player;
    EnemyStats stats;

    Coroutine attackCoroutine;

    private bool showAttackRange = false;
    private float attackRangeTimer;
    private LineRenderer lineRenderer;

    public override void EnterState(BatStateManager enemy)
    {
        player = enemy.player;
        stats = enemy.stats;
        
        // Zainicjalizuj LineRenderer
        BatStateManager batManager = enemy as BatStateManager;
        if (batManager != null)
        {
            lineRenderer = batManager.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = batManager.gameObject.AddComponent<LineRenderer>();
            }
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingOrder = 10;
        }

        // Sprawdź czy gracz jest w zasięgu i w linii prostej
        if (player != null && stats != null)
        {
            float distance = Vector2.Distance(enemy.transform.position, player.transform.position);
            if (distance < stats.attackRange && IsPlayerInLineOfSight(enemy))
            {
                // Gracz w zasięgu i w linii prostej - start ataku
                attackCoroutine = enemy.StartCoroutine(AttackPlayer(enemy));
            }
            else
            {
                // Gracz poza zasięgiem lub nie w linii prostej - powrót do repositionState
                enemy.currentState = enemy.repositionState;
                enemy.currentState.EnterState(enemy);
            }
        }
    }

    public override void ExitState(BatStateManager enemy)
    {
        if (attackCoroutine != null)
        {
            enemy.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        showAttackRange = false;
        if (lineRenderer != null)
            lineRenderer.positionCount = 0;
    }

    public override void UpdateState(BatStateManager enemy)
    {
        
    }

    IEnumerator AttackPlayer(BatStateManager enemy)
    {
        // Pokaż linie dashowania
        showAttackRange = true;
        attackRangeTimer = 0.5f;

        while (attackRangeTimer > 0)
        {
            attackRangeTimer -= Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // Sprawdź czy gracz nadal jest w zasięgu i w linii prostej
        if (!IsPlayerInLineOfSight(enemy) || Vector2.Distance(enemy.transform.position, player.transform.position) > stats.attackRange)
        {
            showAttackRange = false;
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

            // Sprawdzaj czy gracz jest blisko (promieniowo)
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
        showAttackRange = false;
        enemy.currentState = enemy.repositionState;
        enemy.currentState.EnterState(enemy);
    }

    private bool IsPlayerInLineOfSight(BatStateManager enemy)
    {
        if (player == null) return false;

        Vector2 directionToPlayer = (player.transform.position - enemy.transform.position).normalized;
        Vector2 forward = enemy.transform.up;

        // Sprawdź czy gracz jest w linii prostej - dopuszczamy kąt do 15 stopni
        float angle = Vector2.Angle(forward, directionToPlayer);
        return angle <= 45f;
    }

    public bool ShouldShowAttackRange()
    {
        return showAttackRange;
    }

#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
    public void OnCollisionEnter2DState(BatStateManager enemy, Collision2D collision)
#pragma warning restore CS0114 // Member hides inherited member; missing override keyword
    {
        if (collision.gameObject == player)
        {
            // Sprawdzaj warunki ataku: odległość i linia prosta
            float distance = Vector2.Distance(enemy.transform.position, player.transform.position);
            if (distance < stats.attackRange && IsPlayerInLineOfSight(enemy))
            {
                if (attackCoroutine != null)
                {
                    enemy.StopCoroutine(attackCoroutine);
                }
                attackCoroutine = enemy.StartCoroutine(AttackPlayer(enemy));
            }
        }
    }
}