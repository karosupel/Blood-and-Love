using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public event Action OnHealthChanged;
    public event Action<bool> OnAfterlifeStateChanged;
    public event Action<int> OnHeartsChanged;

    [SerializeField] float maxHealth = 100f;
    [SerializeField] float panicMaxHealth = 0.25f;
    [SerializeField] public int hearts = 1;
    [SerializeField] float currentHealth;
    [SerializeField] Vector3 hellOffset = new Vector3(-30f, 0f, 0f);
    PlayerAbilities playerAbilities;

    [SerializeField] float afterlifeInvincibilityDuration = 1f;
    [SerializeField] float materialInvincibilityDuration = 0.2f;
    [SerializeField] float dashDamageMultiplier = 0.5f;
    [Header("Tutorial settings")]
    [SerializeField] bool startInHellInTutorialScene = true;
    [SerializeField] string tutorialSceneName = "Tutorial";
    bool isInAfterlife = false;

    #nullable enable
    [SerializeField] private PopUpManager? popUpManagerScript;
    #nullable disable
    

    public float MaxHealth => maxHealth;

    public float CurrentHealth => currentHealth;
    public bool IsInAfterlife => isInAfterlife;
    public int Hearts => hearts;
    private Vector3 deathPlace;
    bool isInvincible = false;
    bool isDashing = false;
    [SerializeField] float cameraShakeCooldown = 0.5f;
    float lastImpulse = 0;
    bool isPanicked = false;
    SpriteRenderer spriteRenderer;
    Animator animator;

    // CAMERA

    CinemachineImpulseSource impulseSource;


    void Awake()
    {
        playerAbilities = GetComponent<PlayerAbilities>();
        currentHealth = maxHealth;

        if (BossFightRestartState.TryConsumeHeartRestore(out int restoredHearts))
        {
            hearts = restoredHearts;
            isInAfterlife = false;
        }

        OnHealthChanged?.Invoke();
        OnHeartsChanged?.Invoke(hearts);
        impulseSource = GetComponent<CinemachineImpulseSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (popUpManagerScript == null && SceneManager.GetActiveScene().name == tutorialSceneName)
        {
            popUpManagerScript = FindObjectOfType<PopUpManager>();
        }

        if (!startInHellInTutorialScene)
        {
            return;
        }

        if (SceneManager.GetActiveScene().name == tutorialSceneName)
        {
            GoToHell();
        }
    }

    public void Die()
    {
        if (ShouldShowImmediateGameOverInBossPhaseOne())
        {
            Debug.Log("Player died in tutorial or boss phase 1. Showing Game Over without sending player to afterlife.");
            TriggerImmediateGameOver();
            return;
        }

        Debug.Log("Player has died! Fight for your life!");
        animator.SetTrigger("die");
    }

    bool ShouldShowImmediateGameOverInBossPhaseOne()
    {
        if (SceneManager.GetActiveScene().name == tutorialSceneName)
        {
            return true;
        }

        BossHealth bossHealth = FindObjectOfType<BossHealth>();
        if (bossHealth == null)
        {
            return false;
        }

        return !bossHealth.IsInAfterlife;
    }

    void TriggerImmediateGameOver()
    {
        hearts = -1;
        OnHeartsChanged?.Invoke(hearts);
        Annihilate();
    }

    public void GoToHell()
    {
        if (isInAfterlife)
        {
            Debug.Log("Player is already in afterlife!");
            return;
        }
        animator.SetBool("isHellish", true);
        animator.ResetTrigger("die");
        Debug.Log("Player's soul goes to afterlife!");
        deathPlace = transform.position;
        isInAfterlife = true;
        OnAfterlifeStateChanged?.Invoke(isInAfterlife);
        transform.position = transform.position + hellOffset;
        SetConfinerForCurrentRoomVariant(true);
        NotifyCinemachineTeleport(hellOffset);
        StartCoroutine(InvincibilityCoroutine(afterlifeInvincibilityDuration));

    }
    public void GoToMaterialPlane()
    {
        if (!isInAfterlife)
        {
            return;
        }
        animator.SetBool("isHellish", false);
        animator.SetTrigger("rebirth");
        currentHealth = 0.3f * maxHealth;
        isInAfterlife = false;
        OnAfterlifeStateChanged?.Invoke(isInAfterlife);
        Vector3 returnDelta = deathPlace - transform.position;
        StartCoroutine(InvincibilityCoroutine(materialInvincibilityDuration));
        transform.position = deathPlace;
        SetConfinerForCurrentRoomVariant(false);
        NotifyCinemachineTeleport(returnDelta);
        playerAbilities.UseUltimate(addHeart: false, freeUse: true);
        OnHealthChanged?.Invoke();
    }

    void SetConfinerForCurrentRoomVariant(bool useHellVariant)
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.SetConfinerForCurrentRoomVariant(useHellVariant);
            return;
        }

        CinemachineConfiner confiner = FindObjectOfType<CinemachineConfiner>();
        if (confiner == null)
        {
            Debug.LogWarning("Could not set confiner bounds because no CinemachineConfiner was found.");
            return;
        }

        PolygonCollider2D targetBoundary = null;
        PolygonCollider2D currentBoundary = confiner.m_BoundingShape2D as PolygonCollider2D;
        PolygonCollider2D[] allBoundaries = FindObjectsOfType<PolygonCollider2D>(true);

        if (currentBoundary != null && !string.IsNullOrEmpty(currentBoundary.gameObject.name))
        {
            string targetName = GetRoomVariantId(currentBoundary.gameObject.name, useHellVariant);
            for (int i = 0; i < allBoundaries.Length; i++)
            {
                PolygonCollider2D candidate = allBoundaries[i];
                if (candidate == null || !candidate.gameObject.scene.IsValid())
                {
                    continue;
                }

                if (string.Equals(candidate.gameObject.name, targetName, StringComparison.OrdinalIgnoreCase))
                {
                    targetBoundary = candidate;
                    break;
                }
            }
        }

        if (targetBoundary == null)
        {
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < allBoundaries.Length; i++)
            {
                PolygonCollider2D candidate = allBoundaries[i];
                if (candidate == null || !candidate.gameObject.scene.IsValid())
                {
                    continue;
                }

                string candidateName = candidate.gameObject.name;
                if (string.IsNullOrEmpty(candidateName))
                {
                    continue;
                }

                bool isHellBoundary = candidateName.EndsWith("H", StringComparison.OrdinalIgnoreCase);
                if (isHellBoundary != useHellVariant)
                {
                    continue;
                }

                float distance = Vector2.Distance(transform.position, candidate.bounds.center);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    targetBoundary = candidate;
                }
            }
        }

        if (targetBoundary == null)
        {
            Debug.LogWarning("Could not set confiner bounds because no matching room boundary was found.");
            return;
        }

        confiner.m_BoundingShape2D = targetBoundary;
        confiner.InvalidatePathCache();
    }

    string GetRoomVariantId(string roomTypeId, bool useHellVariant)
    {
        bool isHellId = roomTypeId.EndsWith("H", StringComparison.OrdinalIgnoreCase);
        if (useHellVariant)
        {
            return isHellId ? roomTypeId : roomTypeId + "H";
        }

        if (!isHellId)
        {
            return roomTypeId;
        }

        return roomTypeId.Substring(0, roomTypeId.Length - 1);
    }

    void NotifyCinemachineTeleport(Vector3 delta)
    {
        Cinemachine.CinemachineVirtualCameraBase[] virtualCameras = FindObjectsOfType<Cinemachine.CinemachineVirtualCameraBase>();
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            if (virtualCameras[i] == null)
            {
                continue;
            }

            virtualCameras[i].OnTargetObjectWarped(transform, delta);
        }
    }


    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        OnHealthChanged?.Invoke();
    }

    public void TakeDamage(float damage, float knockback = 1f)
    {
        if (isInvincible || isPanicked)
        {
            return;
        }
        if (isDashing)
        {
            damage *= dashDamageMultiplier;
        }
        if(Time.time >= lastImpulse + cameraShakeCooldown)
        {
            impulseSource.GenerateImpulse(force: 1f);
            lastImpulse = Time.time;
        }
        
        StartCoroutine(ColorCoroutine(0.1f, Color.red));
        if (!isInAfterlife)
        {
            currentHealth -= damage;
            if (currentHealth <= MaxHealth * panicMaxHealth && currentHealth > 0)
            {
                playerAbilities.LesbianPanic();
            }
            if (currentHealth <= 0)
            {
                currentHealth = 0f;
                OnHealthChanged?.Invoke();
                Die();
                return;
            }

            OnHealthChanged?.Invoke();
        }
        else if (popUpManagerScript == null || (popUpManagerScript.phase > 4 || popUpManagerScript.phase == 0))
        {
            TakeHeart();
            StartCoroutine(InvincibilityCoroutine(afterlifeInvincibilityDuration));
        }

    }

    IEnumerator ColorCoroutine(float duration, Color targetColor)
    {
        Color basicColor = Color.white;
        spriteRenderer.color = targetColor;
        yield return new WaitForSecondsRealtime(duration);
        spriteRenderer.color = basicColor;
    }

    public void TakeHeart()
    {
        if (SceneManager.GetActiveScene().name == tutorialSceneName)
        {
            return;
        }

        hearts--;
        OnHeartsChanged?.Invoke(hearts);
        if (hearts < 0) //do zmiany, zależnie czy gracz kiedy ma 0 serc nadal może żyć
        {
            animator.SetTrigger("die");
        } 
    }

    public void AddHeart()
    {
        hearts++;
        OnHeartsChanged?.Invoke(hearts);
        Debug.Log("Gracz zdobył serce! Aktualnie posiada " + hearts);
    }

    public void Annihilate()
    {
        animator.ResetTrigger("die");
        Debug.Log("Player's soul got annihilated!");
        Destroy(gameObject);
    }

    IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSecondsRealtime(duration);
        isInvincible = false;
    }

    private bool CanTakeHeartInAfterlife()
    {
        bool isTutorialScene = SceneManager.GetActiveScene().name == tutorialSceneName;

        if (isTutorialScene)
        {
            return false;
        }

        return true;
    }


    public void SetDashing(bool dashing)
    {
        isDashing = dashing;
    }

    public void SetPanicked(bool panicked)
    {
        isPanicked = panicked;
    }
}
