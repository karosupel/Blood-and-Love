using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAbilities : MonoBehaviour
{

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
    Animator animator;

    int activeCrystalCount;
    GameObject activeBarrierInstance;
    BossController bossController;
    Collider2D bossCollider;
    bool hellishVariant = false;


    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        bossController = GetComponent<BossController>();
        bossCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    #region Meteor Storm
    public float MeteorStorm()
    {
        animator.SetBool("isCastingMeteorStorm", true);
        StartCoroutine(MeteorStormCoroutine(farMeteors, meteorStormDuration, meteorStormFarInnerRadius, meteorStormFarOuterRadius));
        StartCoroutine(MeteorStormCoroutine(closeMeteors, meteorStormDuration, meteorStormCloseInnerRadius, meteorStormCloseOuterRadius));
        return meteorStormDuration;
    }


    IEnumerator MeteorStormCoroutine(int meteors, float time, float innerRadius, float outerRadius)
    {
        yield return new WaitForSeconds(1f); // Short delay before meteors start falling, can be adjusted or removed as needed
        float interval = time / meteors;
        for (int i = 0; i < meteors; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle.normalized * Random.Range(innerRadius, outerRadius);
            Vector3 randomPos = new Vector3(player.transform.position.x + randomOffset.x, player.transform.position.y + randomOffset.y, 0);
            GameObject newMeteor = Instantiate(meteorPrefab, randomPos, Quaternion.identity);
            newMeteor.transform.localScale *= meteorSizeMultiplier;
            if (hellishVariant)
            {
                newMeteor.GetComponent<Meteor>()?.SetVariant(true);
            }
            yield return new WaitForSeconds(interval);
            bossController.SetMeteorStormTimer(Time.time);
        }
        animator.SetBool("isCastingMeteorStorm", false);
    }
    #endregion

    #region Projectile Storm
    public float ProjectileStorm()
    {
        animator.SetBool("isCastingProjectileStorm", true);
        StartCoroutine(ProjectileStormCoroutine());
        return projectileStormDuration;
    }
    IEnumerator ProjectileStormCoroutine()
    {
        yield return new WaitForSeconds(1f); // Short delay before starting to fire projectiles, can be adjusted or removed as needed
        float elapsed = 0f;
        float fireInterval = 1f / projectileStormRateOfFire; // Convert projectiles/second to interval
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
                    float angle = currentRotation + (i * 360f / projectileStormOrigins);
                    Vector3 originPos = transform.position + Quaternion.Euler(0, 0, angle) * Vector3.up * radius;

                    // Calculate direction outward from center
                    Vector3 direction = (originPos - transform.position).normalized;

                    GameObject projectile = Instantiate(projectilePrefab, originPos, Quaternion.identity);
                    if (hellishVariant)
                    {
                        projectile.GetComponent<SpriteRenderer>().color = Color.red;
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
        bossController.SetProjectileStormTimer(Time.time);
        animator.SetBool("isCastingProjectileStorm", false);
    }
    #endregion

    #region Barrier
    public void Barrier()
    {
        activeCrystalCount = 0;

        if (activeBarrierInstance != null)
        {
            Destroy(activeBarrierInstance);
            activeBarrierInstance = null;
        }

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
                        Debug.Log($"Invalid crystal position at {spawnPos}, hit {hit.collider.name}");
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
                        activeCrystalCount++;
                    }
                    Debug.DrawLine(transform.position, spawnPos, Color.green, 2f);
                    Debug.Log($"Spawned barrier crystal at {spawnPos}");
                }

                attempts++;
            }
        }

        if (activeCrystalCount > 0)
        {
            Debug.Log("Instantiated barrier at position: " + transform.position);
            // Disable boss collider when barrier is made
            if (bossCollider != null)
            {
                bossCollider.enabled = false;
            }
            Debug.Log("Barrier instantiated with " + activeCrystalCount + " crystals. Boss collider disabled.");
            activeBarrierInstance = Instantiate(barrierPrefab, transform.position, Quaternion.identity);
            if (hellishVariant)
            {
                activeBarrierInstance.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 0.4f); // Light red color for hellish variant
                Debug.Log("Hellish variant active: Barrier color set to red.");
            }
        }
    }

    public void OnCrystalDestroyed()
    {
        activeCrystalCount--;
        if (activeCrystalCount <= 0)
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


}
