using UnityEngine;

public abstract class MaidBaseState
{
    public abstract void EnterState(MaidStateManager enemy);

    public abstract void UpdateState(MaidStateManager enemy);

    public virtual void FixedUpdateState(MaidStateManager enemy)
    {
        // Default implementation - can be overridden by derived classes
    }
}
