using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAbilities : MonoBehaviour
{
    private enum OffensiveCastType
    {
        None,
        MeteorStorm,
        ProjectileStorm
    }

    [Header("Meteor Storm")]
    [SerializeField] int farMeteors = 10;
    [SerializeField] float meteorStormFarInnerRadius = 2;
    [SerializeField] float meteorStormFarOuterRadius = 5;
    [SerializeField] int closeMeteors = 5;
    [SerializeField] float meteorStormCloseInnerRadius = 0;
    [SerializeField] float meteorStormCloseOuterRadius = 2;
    [SerializeField] float meteorStormDuration = 5f;
    [SerializeField] float meteorSizeMultiplier = 1f;
    [SerializeField] GameObject meteorPrefab;
    [Header("Projectile Storm")]
    [SerializeField] float projectileStormDuration = 5f;
    [SerializeField] float projectileStormRateOfFire = 0.5f;
    [SerializeField] float projectileStormProjectileSpeed = 5f;
    [SerializeField] int projectileStormOrigins = 4;
    [SerializeField] float minimumProjectileStormRotationSpeed = 30f;
    [SerializeField] float maximumProjectileStormRotationSpeed = 60f;
    [SerializeField] GameObject projectilePrefab;
    [Header("Barrier")]
    [SerializeField] float minimumCrystalDistance = 2f;
    [SerializeField] float maximumCrystalDistance = 5f;
    [SerializeField] int minimumCrystals = 3;
    [SerializeField] int maximumCrystals = 6;
    [SerializeField] GameObject barrierCrysalPrefab;
    [SerializeField] GameObject barrierPrefab;
    [Header("Other")]
    [SerializeField] GameObject player;
    [SerializeField] float delayAfterStartAnimationBeforeCast = 1f;

    int activeCrystalCount;
    GameObject activeBarrierInstance;
    BossController bossController;
    Collider2D bossCollider;
    bool hellishVariant = false;
    Animator bossAnimator;
    Animator barrierAnimator;
    BossHealth bossHealth;
    private OffensiveCastType activeOffensiveCast = OffensiveCastType.None;
    private int activeMeteorCoroutines = 0;
    private float meteorAnimationEventFallbackTimer = 0f;
    private float meteorCastTimeoutTimer = 0f;
    private int meteorCastId = 0;
    private const float meteorAnimationEventFallbackDelay = 1f;
    private const float meteorCastFailSafeBuffer = 1.5f;
    private float projectileAnimationEventFallbackTimer = 0f;
    private float projectileCastTimeoutTimer = 0f;
    private int projectileCastId = 0;
    private const float projectileAnimationEventFallbackDelay = 1f;
    private const float projectileCastFailSafeBuffer = 1.5f;
    private const float startAnimationExitTimeout = 2f;
    private static readonly int meteorStartStateHash = Animator.StringToHash("WizardMeteorStormStart");
    private static readonly int meteorStartSpStateHash = Animator.StringToHash("WizardMeteorStormStartSP");
    private static readonly int projectileStartStateHash = Animator.StringToHash("WizardProjectileStormStart");
    private static readonly int projectileStartSpStateHash = Animator.StringToHash("WizardProjectileStormStartSP");
    private Coroutine pendingMeteorCastCoroutine;
    private Coroutine pendingProjectileCastCoroutine;
    private bool meteorCastQueued = false;
    private bool projectileCastQueued = false;


    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        bossController = GetComponent<BossController>();
        bossCollider = GetComponent<Collider2D>();
        bossAnimator = GetComponent<Animator>();
        bossHealth = GetComponent<BossHealth>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (activeBarrierInstance != null)
        {
            activeBarrierInstance.transform.position = transform.position;
        }

        HandleMeteorStormFailSafes();
        HandleProjectileStormFailSafes();
    }
    
    #region Meteor Storm
    // AnimationEvent wrapper to avoid relying on non-void return signatures.
    public void MeteorStormAnimationEvent()
    {
        QueueMeteorStormCast();
    }

    private void QueueMeteorStormCast()
    {
        if (meteorCastQueued || activeOffensiveCast == OffensiveCastType.MeteorStorm)
        {
            return;
        }

        meteorCastQueued = true;
        if (pendingMeteorCastCoroutine != null)
        {
            StopCoroutine(pendingMeteorCastCoroutine);
        }

        pendingMeteorCastCoroutine = StartCoroutine(DelayedMeteorStormCastCoroutine());
    }

    private IEnumerator DelayedMeteorStormCastCoroutine()
    {
        yield return WaitForStartAnimationToFinish(meteorStartStateHash, meteorStartSpStateHash);
        yield return new WaitForSeconds(Mathf.Max(0f, delayAfterStartAnimationBeforeCast));

        meteorCastQueued = false;
        pendingMeteorCastCoroutine = null;

        if (bossAnimator != null && !bossAnimator.GetBool("isCastingMeteorStorm"))
        {
            yield break;
        }

        MeteorStorm();
    }

    public float MeteorStorm()
    {
        if (!TryBeginOffensiveCast(OffensiveCastType.MeteorStorm))
        {
            bossAnimator.SetBool("isCastingMeteorStorm", false);
            return 0f;
        }

        meteorCastId++;
        meteorCastTimeoutTimer = 0f;
        activeMeteorCoroutines = 0;

        TryStartMeteorWave(farMeteors, meteorStormDuration, meteorStormFarInnerRadius, meteorStormFarOuterRadius, meteorCastId);
        TryStartMeteorWave(closeMeteors, meteorStormDuration, meteorStormCloseInnerRadius, meteorStormCloseOuterRadius, meteorCastId);

        if (activeMeteorCoroutines <= 0)
        {
            CompleteMeteorStormCast(meteorCastId);
            return 0f;
        }

        return meteorStormDuration;
    }

    private void TryStartMeteorWave(int meteors, float time, float innerRadius, float outerRadius, int castId)
    {
        if (meteors <= 0 || time <= 0f)
        {
            return;
        }

        activeMeteorCoroutines++;
        StartCoroutine(MeteorStormCoroutine(meteors, time, innerRadius, outerRadius, castId));
    }


    IEnumerator MeteorStormCoroutine(int meteors, float time, float innerRadius, float outerRadius, int castId)
    {
        try
        {
            float interval = time / meteors;
            for (int i = 0; i < meteors; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle.normalized * Random.Range(innerRadius, outerRadius);
                if (player != null)
                {
                    Vector3 randomPos = new Vector3(player.transform.position.x + randomOffset.x, player.transform.position.y + randomOffset.y, 0);
                    GameObject newMeteor = Instantiate(meteorPrefab, randomPos, Quaternion.identity);
                    newMeteor.transform.localScale *= meteorSizeMultiplier;
                    if (!hellishVariant)
                    {
                        newMeteor.GetComponent<Meteor>()?.SetVariant(true);
                    }
                }

                yield return new WaitForSeconds(interval);
            }
        }
        finally
        {
            CompleteMeteorStormWave(castId);
        }
    }

    private void CompleteMeteorStormWave(int castId)
    {
        if (castId != meteorCastId)
        {
            return;
        }

        activeMeteorCoroutines = Mathf.Max(0, activeMeteorCoroutines - 1);
        if (activeMeteorCoroutines <= 0)
        {
            CompleteMeteorStormCast(castId);
        }
    }

    private void CompleteMeteorStormCast(int castId)
    {
        if (castId != meteorCastId)
        {
            return;
        }

        activeMeteorCoroutines = 0;
        meteorCastTimeoutTimer = 0f;
        if (bossAnimator != null)
        {
            bossAnimator.SetBool("isCastingMeteorStorm", false);
        }

        bossController?.SetMeteorStormTimer(Time.time);
        EndOffensiveCast(OffensiveCastType.MeteorStorm);
    }

    private void HandleMeteorStormFailSafes()
    {
        if (bossAnimator == null)
        {
            return;
        }

        bool meteorAnimationFlag = bossAnimator.GetBool("isCastingMeteorStorm");

        if (meteorAnimationFlag && activeOffensiveCast == OffensiveCastType.None && !meteorCastQueued)
        {
            meteorAnimationEventFallbackTimer += Time.deltaTime;
            if (meteorAnimationEventFallbackTimer >= meteorAnimationEventFallbackDelay)
            {
                meteorAnimationEventFallbackTimer = 0f;
                MeteorStorm();
            }
        }
        else
        {
            meteorAnimationEventFallbackTimer = 0f;
        }

        if (activeOffensiveCast == OffensiveCastType.MeteorStorm)
        {
            meteorCastTimeoutTimer += Time.deltaTime;
            float meteorCastMaxDuration = Mathf.Max(1f, meteorStormDuration + meteorCastFailSafeBuffer);
            if (meteorCastTimeoutTimer >= meteorCastMaxDuration)
            {
                CompleteMeteorStormCast(meteorCastId);
            }
        }
        else
        {
            meteorCastTimeoutTimer = 0f;
        }
    }
    #endregion

    #region Projectile Storm
    public void ProjectileStormAnimationEvent()
    {
        QueueProjectileStormCast();
    }

    private void QueueProjectileStormCast()
    {
        if (projectileCastQueued || activeOffensiveCast == OffensiveCastType.ProjectileStorm)
        {
            return;
        }

        projectileCastQueued = true;
        if (pendingProjectileCastCoroutine != null)
        {
            StopCoroutine(pendingProjectileCastCoroutine);
        }

        pendingProjectileCastCoroutine = StartCoroutine(DelayedProjectileStormCastCoroutine());
    }

    private IEnumerator DelayedProjectileStormCastCoroutine()
    {
        yield return WaitForStartAnimationToFinish(projectileStartStateHash, projectileStartSpStateHash);
        yield return new WaitForSeconds(Mathf.Max(0f, delayAfterStartAnimationBeforeCast));

        projectileCastQueued = false;
        pendingProjectileCastCoroutine = null;

        if (bossAnimator != null && !bossAnimator.GetBool("isCastingProjectileStorm"))
        {
            yield break;
        }

        ProjectileStorm();
    }

    public float ProjectileStorm()
    {
        if (!TryBeginOffensiveCast(OffensiveCastType.ProjectileStorm))
        {
            bossAnimator.SetBool("isCastingProjectileStorm", false);
            return 0f;
        }

        projectileCastId++;
        projectileCastTimeoutTimer = 0f;

        if (projectileStormDuration <= 0f)
        {
            CompleteProjectileStormCast(projectileCastId);
            return 0f;
        }

        StartCoroutine(ProjectileStormCoroutine(projectileCastId));
        return projectileStormDuration;
    }

    IEnumerator ProjectileStormCoroutine(int castId)
    {
        try
        {
            float elapsed = 0f;
            float safeRateOfFire = Mathf.Max(0.01f, projectileStormRateOfFire);
            float fireInterval = 1f / safeRateOfFire; // Convert projectiles/second to interval
            float nextFireTime = 0f;
            float radius = 1f;
            float currentRotation = 0f;
            float rotationSpeed = Random.Range(minimumProjectileStormRotationSpeed, maximumProjectileStormRotationSpeed) * (Random.value < 0.5f ? -1 : 1); // Randomize rotation direction

            while (elapsed < projectileStormDuration)
            {
                elapsed += Time.deltaTime;

                // Rotate the origins around the boss
                currentRotation += rotationSpeed * Time.deltaTime;

                // Fire projectiles at intervals
                if (elapsed >= nextFireTime)
                {
                    for (int i = 0; i < projectileStormOrigins; i++)
                    {
                        if (projectilePrefab == null)
                        {
                            continue;
                        }

                        float angle = currentRotation + (i * 360f / projectileStormOrigins);
                        Vector3 originPos = transform.position + Quaternion.Euler(0, 0, angle) * Vector3.up * radius;

                        // Calculate direction outward from center
                        Vector3 direction = (originPos - transform.position).normalized;

                        GameObject projectile = Instantiate(projectilePrefab, originPos, Quaternion.identity);
                        if (hellishVariant)
                        {
                            SpriteRenderer projectileRenderer = projectile.GetComponent<SpriteRenderer>();
                            if (projectileRenderer != null)
                            {
                                projectileRenderer.color = Color.blue;
                            }
                        }
                        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            rb.velocity = direction * projectileStormProjectileSpeed;
                        }
                    }
                    nextFireTime += fireInterval;
                }

                yield return null;
            }

            Debug.Log($"Projectile Storm Elapsed Time: {elapsed}");
        }
        finally
        {
            CompleteProjectileStormCast(castId);
        }
    }

    private void CompleteProjectileStormCast(int castId)
    {
        if (castId != projectileCastId)
        {
            return;
        }

        projectileCastTimeoutTimer = 0f;
        if (bossAnimator != null)
        {
            bossAnimator.SetBool("isCastingProjectileStorm", false);
        }

        bossController?.SetProjectileStormTimer(Time.time);
        EndOffensiveCast(OffensiveCastType.ProjectileStorm);
    }

    private void HandleProjectileStormFailSafes()
    {
        if (bossAnimator == null)
        {
            return;
        }

        bool projectileAnimationFlag = bossAnimator.GetBool("isCastingProjectileStorm");

        if (projectileAnimationFlag && activeOffensiveCast == OffensiveCastType.None && !projectileCastQueued)
        {
            projectileAnimationEventFallbackTimer += Time.deltaTime;
            if (projectileAnimationEventFallbackTimer >= projectileAnimationEventFallbackDelay)
            {
                projectileAnimationEventFallbackTimer = 0f;
                ProjectileStorm();
            }
        }
        else
        {
            projectileAnimationEventFallbackTimer = 0f;
        }

        if (activeOffensiveCast == OffensiveCastType.ProjectileStorm)
        {
            projectileCastTimeoutTimer += Time.deltaTime;
            float projectileCastMaxDuration = Mathf.Max(1f, projectileStormDuration + projectileCastFailSafeBuffer);
            if (projectileCastTimeoutTimer >= projectileCastMaxDuration)
            {
                CompleteProjectileStormCast(projectileCastId);
            }
        }
        else
        {
            projectileCastTimeoutTimer = 0f;
        }
    }

    private IEnumerator WaitForStartAnimationToFinish(int normalStartStateHash, int secondPhaseStartStateHash)
    {
        if (bossAnimator == null)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < startAnimationExitTimeout)
        {
            AnimatorStateInfo currentState = bossAnimator.GetCurrentAnimatorStateInfo(0);
            int currentStateHash = currentState.shortNameHash;
            if (currentStateHash != normalStartStateHash && currentStateHash != secondPhaseStartStateHash)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    #endregion

    #region Barrier
    public void Barrier()
    {

        GameObject[] crystals = GameObject.FindGameObjectsWithTag("BarrierCrystal");
        foreach (var crystal in crystals)
        {
            Destroy(crystal);
        }
        activeCrystalCount = 0;

        if (activeBarrierInstance != null)
        {
            Destroy(activeBarrierInstance);
            activeBarrierInstance = null;
        }
        Debug.Log("Preparing new Barrier");

        int crystalsToSpawn = Random.Range(minimumCrystals, maximumCrystals + 1);
        for (int i = 0; i < crystalsToSpawn; i++)
        {
            Vector3 spawnPos;
            int maxAttempts = 10;
            int attempts = 0;
            bool validPosition = false;

            while (!validPosition && attempts < maxAttempts)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                float randomDistance = Random.Range(minimumCrystalDistance, maximumCrystalDistance);
                spawnPos = transform.position + new Vector3(randomDirection.x, randomDirection.y, 0) * randomDistance;

                // Raycast to check if position is inside room (only check for walls)
                // Only detect walls/boundaries, ignore all other colliders
                int layerMask = LayerMask.GetMask("Default"); // Change to "Wall" or "Boundary" layer name if available
                RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, spawnPos, layerMask);
                bool wallHit = false;
                foreach (var hit in hits){
                    if (hit.collider.name == "Walls" || hit.collider.name == "WallsH")
                    {
                        wallHit = true;
                        Debug.DrawLine(transform.position, spawnPos, Color.red, 2f);
                        //Debug.Log($"Invalid crystal position at {spawnPos}, hit {hit.collider.name}");
                        break;
                    }
                }
                if(!wallHit)
                {
                    validPosition = true;
                    GameObject crystalObject = Instantiate(barrierCrysalPrefab, spawnPos, Quaternion.identity);
                    BarrierCrystal crystal = crystalObject.GetComponent<BarrierCrystal>();
                    if (crystal != null)
                    {
                        crystal.Initialize(this);
                        if (hellishVariant)
                        {
                            crystal.GetComponent<SpriteRenderer>().color = Color.red;
                        }
                        else
                        {
                            crystal.GetComponent<SpriteRenderer>().color = Color.green;
                        }
                        activeCrystalCount++;
                    }
                    Debug.DrawLine(transform.position, spawnPos, Color.green, 2f);
                    //Debug.Log($"Spawned barrier crystal at {spawnPos}");
                }

                attempts++;
            }
        }

        if (activeCrystalCount > 0)
        {
            activeBarrierInstance = Instantiate(barrierPrefab, transform.position, Quaternion.identity);
            Barrier barrier = activeBarrierInstance.GetComponent<Barrier>();
            if (barrier != null)
            {
                barrier.Init(this);
            }
            barrierAnimator = activeBarrierInstance.GetComponent<Animator>();
            activeBarrierInstance.GetComponent<Collider2D>().enabled = false;
            if (hellishVariant)
            {
                activeBarrierInstance.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 1f); // Light red color for hellish variant
                //Debug.Log("Hellish variant active: Barrier color set to red.");
            }
        }
    }

    public void BarrierFinished()
    {
        // Disable boss collider when barrier is made
        Debug.Log("Barrier Finished (bossAbilities) running. Collider: " + bossCollider);
        activeBarrierInstance.GetComponent<Collider2D>().enabled = true;
        Debug.Log("Collider: " + activeBarrierInstance.GetComponent<Collider2D>() + " , state: " + activeBarrierInstance.GetComponent<Collider2D>().enabled);
        if (bossCollider != null)
        {
            bossCollider.enabled = false;
            Debug.Log("Boss collider: " + bossCollider.enabled);
        }
        if (activeBarrierInstance != null)
        {
            activeBarrierInstance.GetComponent<Collider2D>().enabled = true;
        }

        bossHealth?.NotifyBarrierCastFinished();
    }

    public void OnCrystalDestroyed()
    {
        activeCrystalCount--;
        if (activeCrystalCount <= 0)
        {
            if (activeBarrierInstance != null)
            {
                Animator animator = activeBarrierInstance.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger("Collapse");
                }
            }
        }
    }

    public void BarrierDestroyed()
    {
        if (activeBarrierInstance != null)
            {
                Destroy(activeBarrierInstance);
                activeBarrierInstance = null;
            }

            if (bossCollider != null)
            {
                bossCollider.enabled = true;
            }

            activeCrystalCount = 0;
            bossController.SetBarrierTimer(Time.time);
    }
    #endregion

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.transform.position, meteorStormFarInnerRadius);
        Gizmos.DrawWireSphere(player.transform.position, meteorStormFarOuterRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.transform.position, meteorStormCloseInnerRadius);
        Gizmos.DrawWireSphere(player.transform.position, meteorStormCloseOuterRadius);
    }



    public void MultiplyMeteorSize(float multiplier)
    {
        meteorSizeMultiplier *= multiplier;
    }
    public void AddProjectileStormOrigin(int amount = 1)
    {
        projectileStormOrigins += amount;
    }
    public void MultiplyProjectileStormSpeed(float multiplier)
    {
        projectileStormProjectileSpeed *= multiplier;
    }


    public void HellishVariant(bool isHellish)
    {
        hellishVariant = isHellish;
    }



    public bool IsBarrierActive()
    {
        return activeBarrierInstance != null;
    }

    public bool IsOffensiveAbilityActive()
    {
        return activeOffensiveCast != OffensiveCastType.None;
    }

    public void ResetOffensiveCastState()
    {
        activeOffensiveCast = OffensiveCastType.None;
        activeMeteorCoroutines = 0;
        meteorAnimationEventFallbackTimer = 0f;
        meteorCastTimeoutTimer = 0f;
        projectileAnimationEventFallbackTimer = 0f;
        projectileCastTimeoutTimer = 0f;
        meteorCastQueued = false;
        projectileCastQueued = false;

        if (pendingMeteorCastCoroutine != null)
        {
            StopCoroutine(pendingMeteorCastCoroutine);
            pendingMeteorCastCoroutine = null;
        }

        if (pendingProjectileCastCoroutine != null)
        {
            StopCoroutine(pendingProjectileCastCoroutine);
            pendingProjectileCastCoroutine = null;
        }

        if (bossAnimator != null)
        {
            bossAnimator.SetBool("isCastingMeteorStorm", false);
            bossAnimator.SetBool("isCastingProjectileStorm", false);
        }
    }

    private bool TryBeginOffensiveCast(OffensiveCastType castType)
    {
        if (activeOffensiveCast != OffensiveCastType.None)
        {
            return false;
        }

        activeOffensiveCast = castType;
        return true;
    }

    private void EndOffensiveCast(OffensiveCastType castType)
    {
        if (activeOffensiveCast == castType)
        {
            activeOffensiveCast = OffensiveCastType.None;
        }
    }

    public void ResumeCasting()
    {
        bossController.pauseCasting = false;
        bossHealth.isInvincible = false;
        
    }
}