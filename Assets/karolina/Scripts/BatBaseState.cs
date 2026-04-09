using UnityEngine;

public abstract class BatBaseState
{
    public abstract void EnterState(BatStateManager enemy);

    public abstract void UpdateState(BatStateManager enemy);

    public virtual void FixedUpdateState(BatStateManager enemy)
    {
        // Default implementation - can be overridden by derived classes
    }

    public virtual void OnCollisionEnter2DState(BatStateManager enemy, Collision2D collision)
    {
        
    }
    public virtual void ExitState(BatStateManager enemy) { }
}
