using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaidAttack : MaidBaseState
{
    public GameObject player;
    public EnemyStats stats;

    public bool isAttacking = false;
    bool isDead = false;

    public override void EnterState(MaidStateManager enemy)
    {
        //Debug.Log("Entered Attack State");
        player = enemy.player;
        stats = enemy.stats;
    }

    public override void UpdateState(MaidStateManager enemy)
    {
        if (isDead)
        {
            return;
        }
        //Vector2 direction = (enemy.transform.position - player.transform.position).normalized;
        if(enemy.transform.position.x < player.transform.position.x)
        {
            enemy.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            enemy.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        //enemy.transform.rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.identity.eulerAngles);

        if (Vector2.Distance(enemy.transform.position, player.transform.position) > enemy.stats.attackRange)
        {
            enemy.currentState = enemy.chaseState;
            enemy.currentState.EnterState(enemy);
        }

        if (Vector2.Distance(enemy.transform.position, player.transform.position) < enemy.stats.attackRange && !isAttacking)
        {
            isAttacking = true;
            enemy.StartCoroutine(AttackPlayer(enemy));

        }
    }

    public IEnumerator AttackPlayer(MaidStateManager enemy)
    {
        enemy.animator.SetBool("isAttacking", true);
        yield return new WaitForSeconds(stats.attackCooldown);
        if(IsInAttackRange(enemy))
        {
            player.GetComponent<PlayerHealth>().TakeDamage(stats.damage);
        }
        yield return new WaitForSeconds(stats.attackCooldown);
        isAttacking = false;
        enemy.animator.SetBool("isAttacking", false);
    }

    public bool IsInAttackRange(MaidStateManager enemy)
    {
        if(Vector2.Distance(enemy.transform.position, player.transform.position) < enemy.stats.attackRange)
        {
            return true;
        }
        return false;
    }

    public void StateMachineDead(bool isDead)
    {
        if (isDead)
        {
            isDead = true;
        }
    }
}
