using UnityEngine;

public abstract class EnemyBaseState
{
    public abstract void EnterState(EnemyStateManager enemy);

    public abstract void UpdateState(EnemyStateManager enemy);

    public virtual void FixedUpdateState(EnemyStateManager enemy)
    {
        // Default implementation - can be overridden by derived classes
    }

    public virtual void OnTriggerEnter2D(EnemyStateManager enemy, Collider2D collider)
    {
        // Default implementation - can be overridden by derived classes
    }
}
