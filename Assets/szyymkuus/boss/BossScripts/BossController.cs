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

    [Header("Second Phase Settings")]
    [SerializeField] float sp_MeteorSizeMultiplier = 2f;
    [SerializeField] int sp_AdditionalProjectileOrigins = 2;
    [SerializeField] float sp_ProjectileSpeedMultiplier = 1.3f;

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
        lastBarrierTime = float.MinValue;
        if (Random.value < 0.5f)
        {
            lastMeteorStormTime = float.MinValue;
            lastProjectileStormTime = Time.time;
        }
        else
        {
            lastMeteorStormTime = Time.time;
            lastProjectileStormTime = float.MinValue;
        }
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
        if (lastBarrierTime + barrierCooldownMaximum < Time.time)
        {
            
            StartCoroutine(DelayBarrierCoroutine(0f));
            return;
        }
        else
        {
            float randomCooldown = Random.Range(barrierCooldownMinimum, barrierCooldownMaximum);
            StartCoroutine(DelayBarrierCoroutine(randomCooldown));
        }
    }

    IEnumerator DelayBarrierCoroutine(float delay)
    {
        isBarrierActive = true;
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
        if (lastMeteorStormTime + meteorStormCooldownMaximum < Time.time)
        {
            StartCoroutine(DelayMeteorStormCoroutine(0f));
        }
        else
        {
            float randomCooldown = Random.Range(meteorStormCooldownMinimum, meteorStormCooldownMaximum);
            StartCoroutine(DelayMeteorStormCoroutine(randomCooldown));
        }
    }

    IEnumerator DelayMeteorStormCoroutine(float delay)
    {
        isMeteorStormActive = true;
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
        if (lastProjectileStormTime + projectileStormCooldownMaximum < Time.time)
        {
            StartCoroutine(DelayProjectileStormCoroutine(0f));
        }
        else
        {
            float randomCooldown = Random.Range(projectileStormCooldownMinimum, projectileStormCooldownMaximum);
            StartCoroutine(DelayProjectileStormCoroutine(randomCooldown));
        }
    }

    IEnumerator DelayProjectileStormCoroutine(float delay)
    {
        isProjectileStormActive = true;
        yield return new WaitForSeconds(delay);
        abilities.ProjectileStorm();
    }

    public void SetProjectileStormTimer(float time)
    {
        lastProjectileStormTime = time;
        isProjectileStormActive = false;

    }

    #endregion

    public void EnterSecondPhase()
    {
        abilities.HellishVariant(true);
        Debug.Log("Boss entered second phase! He is now stronger and more aggressive!");
        ForceNewBarrier();
        abilities.MultiplyMeteorSize(sp_MeteorSizeMultiplier);
        abilities.AddProjectileStormOrigin(sp_AdditionalProjectileOrigins);
        abilities.MultiplyProjectileStormSpeed(sp_ProjectileSpeedMultiplier);
    }
}
