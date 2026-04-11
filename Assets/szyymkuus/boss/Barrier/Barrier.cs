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
