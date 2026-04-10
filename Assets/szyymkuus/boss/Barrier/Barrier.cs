using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{

    BossAbilities bossAbilities;
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            Debug.Log("Barrier Animator found. Is playing: " + !animator.enabled);
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning("WARNING: Animator Controller is NULL on Barrier!");
            }
        }
        else
        {
            Debug.LogError("ERROR: No Animator found on Barrier!");
        }
    }

    // Start is called before the first frame update

    public void Init(BossAbilities boss)
    {
        bossAbilities = boss;
        Debug.Log("Barrier initialized with BossAbilities: " + (boss != null));
    }


    public void Test(){
        Debug.Log("AnimationEventPlayed!");
    }

    public Animator GetAnimator()
    {
        return animator;
    }

    public void BarrierFinished()
    {
        Debug.Log("Barrier.BarrierFinished() called, bossAbilities is: " + (bossAbilities != null ? "valid" : "NULL"));
        if (bossAbilities != null)
        {
            bossAbilities.BarrierFinished();
        }
        else
        {
            Debug.LogError("ERROR: bossAbilities is null in Barrier.BarrierFinished()!");
        }
    }
    
    void BarrierDestroyed()
    {
        bossAbilities?.BarrierDestroyed();
    }

}
