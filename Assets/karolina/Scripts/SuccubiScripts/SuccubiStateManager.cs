using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateManager : MonoBehaviour
{
    public EnemyBaseState currentState;
    public SuccubiChase chaseState = new SuccubiChase();
    public SuccubiAttack attackState = new SuccubiAttack();

    public SuccubiRetreat retreatState = new SuccubiRetreat();

    private Rigidbody2D enemy_rb;

    public GameObject player;

    public EnemyStats stats;
    public Enemy enemy;
    public Animator animator;

    [SerializeField] public float offsetTime;
    private bool hasStarted = false;

    void Awake()
    {
        enemy_rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        hasStarted = true;
        ResetToChaseState();
    }

    void OnEnable()
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

        ResetToChaseState();
    }

    void Update()
    {
        if (enemy.IsDead() || enemy.IsFrozen())
        {
            return;
        }
        currentState.UpdateState(this);
    }

    void FixedUpdate()
    {
        if (enemy.IsDead() || enemy.IsFrozen())
        {
            return;
        }
        currentState.FixedUpdateState(this);
    }

    // Animation Event: call this on the exact hit frame of succubi attack animation.
    public void OnSuccubiAttackHitAnimationEvent()
    {
        if (enemy != null && enemy.IsDead())
        {
            return;
        }

        attackState.OnAttackAnimationHit(this);
    }

    private void ResetToChaseState()
    {
        currentState = chaseState;
        currentState.EnterState(this);
    }

    // Rysowanie range'a odbywa się teraz w UpdateState() przy użyciu LineRenderer
    // zamiast w OnDrawGizmos()
}
