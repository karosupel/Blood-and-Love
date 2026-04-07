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

    void Awake()
    {
        enemy_rb = GetComponent<Rigidbody2D>();
        Enemy enemy = GetComponent<Enemy>();
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
        Gizmos.color = Color.red;
        Gizmos.DrawLine(this.transform.position, this.transform.position + (player.transform.position - this.transform.position).normalized * 3f);
    }
}
