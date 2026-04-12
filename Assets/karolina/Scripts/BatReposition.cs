using UnityEngine;

public class BatReposition : BatBaseState
{
    GameObject player;
    EnemyStats stats;

    Vector2 targetOffset;
    float repositionTime;
    float timer;

    public override void EnterState(BatStateManager enemy)
    {
        player = enemy.player;
        stats = enemy.stats;

        timer = 0f;
        repositionTime = Random.Range(0.8f, 1.5f);

        // losowy punkt wokół gracza
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Random.Range(stats.attackRange + 1f, stats.attackRange + 3f);

        targetOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }

    public override void ExitState(BatStateManager enemy)
    {
        // nic nie trzeba zatrzymywać
    }

    public override void UpdateState(BatStateManager enemy)
    {
        Vector2 targetPosition = (Vector2)player.transform.position + targetOffset;

        enemy.transform.position = Vector2.Lerp(
            enemy.transform.position,
            targetPosition,
            Time.deltaTime * stats.moveSpeed
        );

        timer += Time.deltaTime;

        if (timer >= repositionTime)
        {
            //Debug.Log("enter attack state");
            enemy.currentState = enemy.attackState;
            enemy.currentState.EnterState(enemy);
        }
    }
}