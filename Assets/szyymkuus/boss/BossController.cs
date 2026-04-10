using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{

    [SerializeField] float meteorStormCooldownMinimum;
    [SerializeField] float meteorStormCooldownMaximum;
    [SerializeField] float projectileStormCooldownMinimum;
    [SerializeField] float projectileStormCooldownMaximum;
    [SerializeField] float barrierCooldownMinimum;
    [SerializeField] float barrierCooldownMaximum;
    float lastMeteorStormTime;
    float lastProjectileStormTime;
    float lastBarrierTime;

    bool isMeteorStormActive;
    bool isProjectileStormActive;
    bool isBarrierActive;


    BossAbilities abilities;

    void Awake()
    {
        abilities = GetComponent<BossAbilities>();

    }


    void Update()
    {
        RandomizeBarrier();
        RandomizeMeteorStorm();
        RandomizeProjectileStorm();
        
    }

    #region  Barrier
    void RandomizeBarrier()
    {
        if (isBarrierActive)
        {
            return;
        }
        if (lastBarrierTime + barrierCooldownMinimum > Time.time)
        {
            return;
        }
        float randomCooldown = Random.Range(barrierCooldownMinimum, barrierCooldownMaximum);
        isBarrierActive = true;
        StartCoroutine(DelayBarrierCoroutine(randomCooldown));
    }

    IEnumerator DelayBarrierCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        abilities.Barrier();
    }

    public void SetBarrierTimer(float time)
    {
        lastBarrierTime = time;
        isBarrierActive = false;
    }

    public void ForceNewBarrier()
    {
        if(isBarrierActive)
        {
            return;
        }
        lastBarrierTime = float.MinValue;
        isBarrierActive = false;
    }

    #endregion

    #region Meteor Storm


    void RandomizeMeteorStorm()
    {
        if (isMeteorStormActive)
        {
            return;
        }
        if (lastMeteorStormTime + meteorStormCooldownMinimum > Time.time)
        {
            return;
        }
        float randomCooldown = Random.Range(meteorStormCooldownMinimum, meteorStormCooldownMaximum);
        isMeteorStormActive = true;
        StartCoroutine(DelayMeteorStormCoroutine(randomCooldown));
    }

    IEnumerator DelayMeteorStormCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        abilities.MeteorStorm();
    }



    public void SetMeteorStormTimer(float time)
    {
        lastMeteorStormTime = time;
        isMeteorStormActive = false;

    }

    #endregion


    #region Projectile Storm

    void RandomizeProjectileStorm()
    {
        if (isProjectileStormActive)
        {
            return;
        }
        if (lastProjectileStormTime + projectileStormCooldownMinimum > Time.time)
        {
            return;
        }
        float randomCooldown = Random.Range(projectileStormCooldownMinimum, projectileStormCooldownMaximum);
        isProjectileStormActive = true;
        StartCoroutine(DelayProjectileStormCoroutine(randomCooldown));
    }

    IEnumerator DelayProjectileStormCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        abilities.ProjectileStorm();
    }

    public void SetProjectileStormTimer(float time)
    {
        lastProjectileStormTime = time;
        isProjectileStormActive = false;

    }



    #endregion
}
