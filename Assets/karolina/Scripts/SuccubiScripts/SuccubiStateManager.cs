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

    void Awake()
    {
        enemy_rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentState = new SuccubiChase();
        currentState.EnterState(this);
    }

    void Update()
    {
        if (enemy.IsDead())
        {
            return;
        }
        currentState.UpdateState(this);
    }

    void FixedUpdate()
    {
        if (enemy.IsDead())
        {
            return;
        }
        currentState.FixedUpdateState(this);
    }

    // Rysowanie range'a odbywa się teraz w UpdateState() przy użyciu LineRenderer
    // zamiast w OnDrawGizmos()
}
