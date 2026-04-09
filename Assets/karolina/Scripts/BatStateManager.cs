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

     void Awake()
    {
        Enemy enemy = GetComponent<Enemy>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void Start()
    {
        currentState = repositionState;
        currentState.EnterState(this);
    }

    public void Update()
    {
        currentState.UpdateState(this);
    }

    private void OnDrawGizmos()
    {
        if (currentState != null)
        {
            if (currentState is BatAttacking attack)
            {
                attack.DrawAttackRangeGizmo(this);
            }
        }
    }
}
