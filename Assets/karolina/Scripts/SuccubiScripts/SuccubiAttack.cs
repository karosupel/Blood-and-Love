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

    public Collider2D[] overlaping_whip_results = new Collider2D[100];

    private ContactFilter2D interactFilter;
    
    private bool showAttackRange = false;
    private float attackRangeTimer;
    private LineRenderer lineRenderer;
    
    public override void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("Entered Attack State");
        isAttacking = true;
        player = enemy.player;
        stats = enemy.stats;
        enemyReference = enemy.enemy;
        LayerMask mask = LayerMask.GetMask("WhipLayer");
		interactFilter.SetLayerMask(mask);
        interactFilter.useTriggers = true;
        
        // Zainicjalizuj LineRenderer
        if (enemyReference != null)
        {
            lineRenderer = enemyReference.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = enemyReference.gameObject.AddComponent<LineRenderer>();
            }
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
        }
    }

    public override void UpdateState(EnemyStateManager enemy)
    {
        // Zmniejszaj timer dla range preview
        if (showAttackRange)
        {
            attackRangeTimer -= Time.deltaTime;
            if (attackRangeTimer <= 0)
            {
                showAttackRange = false;
                if (lineRenderer != null)
                    lineRenderer.positionCount = 0; // Wyczyść LineRenderer
            }
            else
            {
                // Rysuj range przy użyciu LineRenderer
                DrawAttackRangeLineRenderer(enemy, attackAngle);
            }
        }
        else if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0; // Upewni się, że LineRenderer jest pusty gdy nie rysujemy
        }
        
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
            yield break; 
        }

        if(CalculateAttackRange(enemy, attackAngle))
        {
            // Pokaż range przez 0.5 sekund
            showAttackRange = true;
            attackRangeTimer = enemy.offsetTime;

            yield return new WaitForSeconds(enemy.offsetTime);

            if(CalculateAttackRange(enemy, attackAngle))
            {
                enemyReference.DealDamage(player, stats.damage);
                player.GetComponent<IConditionable>()?.Stun(1f);
                yield return new WaitForSeconds(cooldown);
            }
            else if (!CalculateAttackRange(enemy, attackAngle))
            {
                enemy.currentState = enemy.chaseState;
                enemy.currentState.EnterState(enemy);
            }
        }
        else if (!CalculateAttackRange(enemy, attackAngle))
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

    public void DrawAttackRangeLineRenderer(EnemyStateManager enemy, float attackAngle)
    {
        if (enemyReference == null || lineRenderer == null) return;

        Transform t = enemyReference.transform;
        Vector2 forward = t.up;
        float halfAngle = attackAngle / 2f;

        Vector2 leftDir = Quaternion.Euler(0, 0, -halfAngle) * forward;
        Vector2 rightDir = Quaternion.Euler(0, 0, halfAngle) * forward;

        int segments = 20;
        List<Vector3> positions = new List<Vector3>();

        // Punkt środka
        positions.Add(t.position);

        // Lewy kierunek na koniec rangu
        positions.Add(t.position + (Vector3)(leftDir * stats.attackRange));

        // Arc od lewej do prawej
        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngle + (attackAngle / segments) * i;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * forward;
            positions.Add(t.position + (Vector3)(dir * stats.attackRange));
        }

        // Prawy kierunek i powrót do środka
        positions.Add(t.position + (Vector3)(rightDir * stats.attackRange));
        positions.Add(t.position);

        // Ustaw pozycje w LineRenderer
        lineRenderer.positionCount = positions.Count;
        for (int i = 0; i < positions.Count; i++)
        {
            lineRenderer.SetPosition(i, positions[i]);
        }
    }

    public bool ShouldShowAttackRange()
    {
        return showAttackRange;
    }}