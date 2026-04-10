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

    [SerializeField] public float offsetTime;

    void Awake()
    {
        enemy_rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Start()
    {
        currentState = new SuccubiChase();
        currentState.EnterState(this);
    }

    void Update()
    {
        currentState.UpdateState(this);
    }

    void FixedUpdate()
    {
        currentState.FixedUpdateState(this);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (currentState != null)
        {
            if (currentState is SuccubiAttack attack)
            {
                //attack.DrawAttack(this);
                if (attack.ShouldShowAttackRange())
                {
                    attack.DrawAttackRange(this, attack.attackAngle);
                }
            }
        }
    }
}
