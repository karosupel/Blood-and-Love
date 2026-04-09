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
    [SerializeField] int hearts = 0;
    [SerializeField] float currentHealth;
    [SerializeField] Vector3 hellOffset = new Vector3(-30f, 0f, 0f);
    PlayerAbilities playerAbilities;

    [SerializeField] float afterlifeInvincibilityDuration = 1f;
    [SerializeField] float materialInvincibilityDuration = 0.2f;
    [SerializeField] bool startInHellInTutorialScene = true;
    [SerializeField] string tutorialSceneName = "Tutorial";
    bool isInAfterlife = false;

    public float MaxHealth => maxHealth;

    public float CurrentHealth => currentHealth;
    public bool IsInAfterlife => isInAfterlife;
    public int Hearts => hearts = 1;
    private Vector3 deathPlace;
    bool isInvincible = false;


    // CAMERA

    CinemachineImpulseSource impulseSource;


    void Awake()
    {
        playerAbilities = GetComponent<PlayerAbilities>();
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();
        OnHeartsChanged?.Invoke(hearts);
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    void Start()
    {
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
        Debug.Log("Player has died! Fight for your life!");
        GoToHell();
    }

    public void GoToHell()
    {
        if (isInAfterlife)
        {
            Debug.Log("Player is already in afterlife!");
            return;
        }
        Debug.Log("Player's soul goes to afterlife!");
        deathPlace = transform.position;
        isInAfterlife = true;
        OnAfterlifeStateChanged?.Invoke(isInAfterlife);
        transform.position = transform.position + hellOffset;
        NotifyCinemachineTeleport(hellOffset);
        RoomManager.Instance?.SetConfinerForCurrentRoomVariant(true);
        StartCoroutine(InvincibilityCoroutine(afterlifeInvincibilityDuration));

    }
    public void GoToMaterialPlane()
    {
        if (!isInAfterlife)
        {
            return;
        }
        currentHealth = 0.3f * maxHealth;
        isInAfterlife = false;
        OnAfterlifeStateChanged?.Invoke(isInAfterlife);
        Vector3 returnDelta = deathPlace - transform.position;
        StartCoroutine(InvincibilityCoroutine(materialInvincibilityDuration));
        transform.position = deathPlace;
        NotifyCinemachineTeleport(returnDelta);
        RoomManager.Instance?.SetConfinerForCurrentRoomVariant(false);
        playerAbilities.UseUltimate(addHeart: false, freeUse: true);
        OnHealthChanged?.Invoke();
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
        if (isInvincible)
        {
            return;
        }
        impulseSource.GenerateImpulse(force: 1f);
        if (!isInAfterlife)
        {
            currentHealth -= damage;
            if (currentHealth <= MaxHealth * panicMaxHealth)
            {
                playerAbilities.LesbianPanic();
            }

            Debug.Log("Player took damage, current health: " + currentHealth);
            if (currentHealth <= 0)
            {
                currentHealth = 0f;
                OnHealthChanged?.Invoke();
                Die();
                return;
            }

            OnHealthChanged?.Invoke();
        }
        else
        {
            TakeHeart();
            StartCoroutine(InvincibilityCoroutine(afterlifeInvincibilityDuration));
        }

    }

    public void TakeHeart()
    {
        hearts--;
        OnHeartsChanged?.Invoke(hearts);
        Debug.Log("Gracz trafiony w zaświatach! pozostało " + hearts + " serc!");
        if (hearts < 0) //do zmiany, zależnie czy gracz kiedy ma 0 serc nadal może żyć
        {
            Annihilate();
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
        Debug.Log("Player's soul got annihilated!");
        Destroy(gameObject);
    }

    IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }








    void Update()
    {
        //Debug.Log("Current Health: " + currentHealth);
    }
}
