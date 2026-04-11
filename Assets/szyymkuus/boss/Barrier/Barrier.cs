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


    public void Test()
    {
  Debug.Log("AnimationEventPlayed!");      
    }
    public void BarrierFinished()
    {
        if (bossAbilities == null)
        {
            Destroy(gameObject);
            return;
        }
        bossAbilities.BarrierFinished();
    }
    
    void BarrierDestroyed()
    {
        if (bossAbilities == null)
        {
            Destroy(gameObject);
            return;
        }
        bossAbilities?.BarrierDestroyed();
    }

}
