using UnityEngine;

public abstract class BossBaseState
{
    public abstract void EnterState(BossStateManager enemy);

    public abstract void UpdateState(BossStateManager enemy);

    public virtual void FixedUpdateState(BossStateManager enemy)
    {
        // Default implementation - can be overridden by derived classes
    }
}
