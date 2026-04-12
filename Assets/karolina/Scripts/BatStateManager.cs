using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatStateManager : MonoBehaviour
{
    public BatBaseState currentState;
    public BatReposition repositionState = new BatReposition();
    public BatAttacking attackState = new BatAttacking();

    public GameObject player;
    public EnemyStats stats;

    public bool isPlayerAttacked = false;
    Enemy enemy;
    private bool hasStarted = false;
    private SpriteRenderer spriteRenderer;
    private Vector3 lastPosition;
    private float leftFacingScaleX = 1f;
    private const float horizontalDirectionThreshold = 0.001f;

     void Awake()
    {
        enemy = GetComponent<Enemy>();
        player = GameObject.FindGameObjectWithTag("Player");
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        leftFacingScaleX = transform.localScale.x;
        if (Mathf.Approximately(leftFacingScaleX, 0f))
        {
            leftFacingScaleX = 1f;
        }
        lastPosition = transform.position;
    }

    public void Start()
    {
        hasStarted = true;
        ResetToRepositionState();
        FaceLeft();
        lastPosition = transform.position;
    }

    private void OnEnable()
    {
        if (!hasStarted)
        {
            return;
        }

        if (enemy == null)
        {
            enemy = GetComponent<Enemy>();
        }

        if (enemy != null && enemy.IsDead())
        {
            return;
        }

        ResetToRepositionState();
        FaceLeft();
        lastPosition = transform.position;
    }

    public void Update()
    {
        if (enemy.IsDead() || enemy.IsFrozen())
        {
            return;
        }

        currentState.UpdateState(this);
        UpdateFacingDirection();
    }

    // Rysowanie range'a odbywa się teraz w UpdateState() przy użyciu LineRenderer
    // zamiast w OnDrawGizmos()

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == 6 && isPlayerAttacked == false)
        {
            StartCoroutine(CollisionAttack());
        }
    }

    public IEnumerator CollisionAttack()
    {
        if(isPlayerAttacked == false)
        {
            isPlayerAttacked = true;
            player.GetComponent<PlayerHealth>().TakeDamage(stats.damage);
            yield return new WaitForSeconds(1f);
            isPlayerAttacked = false;
        }
    }

    private void ResetToRepositionState()
    {
        isPlayerAttacked = false;
        currentState = repositionState;
        currentState.EnterState(this);
    }

    private void UpdateFacingDirection()
    {
        Vector3 currentPosition = transform.position;
        float horizontalDelta = currentPosition.x - lastPosition.x;

        if (horizontalDelta > horizontalDirectionThreshold)
        {
            FaceRight();
        }
        else if (horizontalDelta < -horizontalDirectionThreshold)
        {
            FaceLeft();
        }

        lastPosition = currentPosition;
    }

    private void FaceLeft()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
            return;
        }

        Vector3 localScale = transform.localScale;
        localScale.x = leftFacingScaleX;
        transform.localScale = localScale;
    }

    private void FaceRight()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = true;
            return;
        }

        Vector3 localScale = transform.localScale;
        localScale.x = -leftFacingScaleX;
        transform.localScale = localScale;
    }

    // void OnCollisionEnter2D(Collision2D collision)
    // {
    //     currentState = attackState;
    //     currentState.OnCollisionEnter2DState(this, collision);
    // }
}
