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
    private LineRenderer lineRenderer;
    private bool hasLockedAttackData = false;
    private Vector2 lockedAttackDirection = Vector2.up;
    private Vector2 lastDirection = Vector2.up;
    private int currentAttackId = 0;
    private bool waitingForAnimationHit = false;
    private bool animationHitReceived = false;
    private const float animationEventTimeout = 2f;
    
    public override void EnterState(EnemyStateManager enemy)
    {
        //Debug.Log("Entered Attack State");
        isAttacking = true;
        waitingForAnimationHit = false;
        animationHitReceived = false;
        showAttackRange = false;
        hasLockedAttackData = false;
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
        if (showAttackRange)
        {
            DrawAttackRangeLineRenderer(enemy, attackAngle);
        }
        else if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0; // Upewni się, że LineRenderer jest pusty gdy nie rysujemy
        }
        
        if (isAttacking && player != null)
        {
            currentAttackId++;
            int attackId = currentAttackId;
            LockAttackData(enemy);
            animationHitReceived = false;
            waitingForAnimationHit = true;
            showAttackRange = true;
            isAttacking = false; // Prevent multiple coroutines from starting
            enemy.animator.SetTrigger("attack");
            enemy.StartCoroutine(AttackCoroutine(enemy, stats.attackCooldown, attackId));
        }
    }

    public IEnumerator AttackCoroutine(EnemyStateManager enemy, float cooldown, int attackId)
    {
        if (player == null)
        {
            FinishAttack(attackId);
            yield break; 
        }

        if (!CalculateAttackRange(enemy, attackAngle))
        {
            enemy.currentState = enemy.chaseState;
            enemy.currentState.EnterState(enemy);
            FinishAttack(attackId);
            yield break;
        }

        float elapsed = 0f;
        while (!animationHitReceived && attackId == currentAttackId && elapsed < animationEventTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (attackId != currentAttackId)
        {
            yield break;
        }

        bool canHitPlayer = animationHitReceived && CalculateAttackRange(enemy, attackAngle);
        if (canHitPlayer)
        {
            enemyReference.DealDamage(player, stats.damage);
            player.GetComponent<IConditionable>()?.Stun(1f);
            yield return new WaitForSeconds(cooldown);
        }
        else
        {
            enemy.currentState = enemy.chaseState;
            enemy.currentState.EnterState(enemy);
        }

        FinishAttack(attackId); // Allow attacking again after cooldown if player is still in range
    }

    private void FinishAttack(int attackId)
    {
        if (attackId != currentAttackId)
        {
            return;
        }

        waitingForAnimationHit = false;
        animationHitReceived = false;
        showAttackRange = false;
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
        ClearLockedAttackData();
        isAttacking = true;
    }

    public void OnAttackAnimationHit(EnemyStateManager enemy)
    {
        if (!waitingForAnimationHit)
        {
            return;
        }

        if (enemy == null || enemy.currentState != enemy.attackState)
        {
            return;
        }

        animationHitReceived = true;
    }

    bool CalculateAttackRange(EnemyStateManager enemy, float attackAngle)
    {
        if (player == null)
        {
            return false;
        }

        Vector2 attackOrigin = GetAttackOrigin(enemy);
        Vector2 toPlayer = (Vector2)player.transform.position - attackOrigin;
        if (toPlayer.sqrMagnitude <= 0.0001f)
        {
            return true;
        }

        Vector2 directionToPlayer = toPlayer.normalized;
        Vector2 forward = GetAttackForwardDirection(enemy);

        float angle = Vector2.Angle(forward, directionToPlayer);
        float distance = Vector2.Distance(attackOrigin, player.transform.position);

        bool inAngle = angle <= attackAngle / 2f;
        bool inRange = distance <= stats.attackRange;

        //Debug.Log($"Angle: {angle}");

        return inAngle && inRange;
    }

    private void LockAttackData(EnemyStateManager enemy)
    {
        if (player == null)
        {
            return;
        }

        Vector2 directionToPlayer = (player.transform.position - enemy.transform.position);
        if (directionToPlayer.sqrMagnitude > 0.0001f)
        {
            lastDirection = directionToPlayer.normalized;
        }

        lockedAttackDirection = lastDirection;
        hasLockedAttackData = true;
    }

    private void ClearLockedAttackData()
    {
        hasLockedAttackData = false;
    }

    private Vector2 GetAttackForwardDirection(EnemyStateManager enemy)
    {
        if (hasLockedAttackData)
        {
            return lockedAttackDirection;
        }

        if (player != null)
        {
            Vector2 directionToPlayer = (player.transform.position - enemy.transform.position);
            if (directionToPlayer.sqrMagnitude > 0.0001f)
            {
                lastDirection = directionToPlayer.normalized;
            }
        }

        return lastDirection;
    }

    private Vector2 GetAttackOrigin(EnemyStateManager enemy)
    {
        return enemy.transform.position;
    }

    public void DrawAttackRangeLineRenderer(EnemyStateManager enemy, float attackAngle)
    {
        if (enemyReference == null || lineRenderer == null) return;

        Vector2 forward = GetAttackForwardDirection(enemy);
        Vector2 attackOrigin = GetAttackOrigin(enemy);
        float halfAngle = attackAngle / 2f;

        Vector2 leftDir = Quaternion.Euler(0, 0, -halfAngle) * forward;
        Vector2 rightDir = Quaternion.Euler(0, 0, halfAngle) * forward;

        int segments = 20;
        List<Vector3> positions = new List<Vector3>();

        // Punkt środka
        positions.Add(attackOrigin);

        // Lewy kierunek na koniec rangu
        positions.Add((Vector3)attackOrigin + (Vector3)(leftDir * stats.attackRange));

        // Arc od lewej do prawej
        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngle + (attackAngle / segments) * i;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * forward;
            positions.Add((Vector3)attackOrigin + (Vector3)(dir * stats.attackRange));
        }

        // Prawy kierunek i powrót do środka
        positions.Add((Vector3)attackOrigin + (Vector3)(rightDir * stats.attackRange));
        positions.Add(attackOrigin);

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