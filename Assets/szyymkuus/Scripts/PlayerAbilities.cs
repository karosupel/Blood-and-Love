using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{

    [SerializeField] LayerMask enemyLayers;
    [SerializeField] Camera cam;

    //TODO: dodać atak i promień podstawowego ataku
    [Header("Basic Attack")]
    [SerializeField] float basicAttackDamage = 10f;
    [SerializeField] float basicAttackRange = 1f;
    [SerializeField] float basicAttackRadius = 0.5f;
    [SerializeField] float basicAttackCooldown = 1f;
    [SerializeField] float basicAttackLifesteal = 0.1f;


    [Header("Special Attack")]
    [SerializeField] float specialAttackDamage = 10f;
    [SerializeField] float specialAttackRange = 5f;
    [SerializeField] float specialAttackCooldown = 3f;
    [SerializeField] float specialAttackLifesteal = 0.1f;

    [Header("Ultimate")]
    [SerializeField] float ultimateDamage = 100f;
    [SerializeField] float ultimateRadius = 10f;
    [SerializeField] float ultimateMaxHealthCost = 0.7f;
    [SerializeField] float ultimateLifesteal = 0.1f;

    [Header("Lesbian Panic")]
    [SerializeField] public float panicDuration = 3f;
    [SerializeField] public float panicCooldown = 60f;
    [SerializeField] public float panicSpeedMultiplier = 1.5f;
    float lastBasicAttackTime = float.MinValue;
    float lastSpecialAttackTime = float.MinValue;
    float lastPanicTime = float.MinValue;
    PlayerController playerController;
    BoxCollider2D col;
    PlayerHealth health;
    SpriteRenderer sprite;
    [Header("Layers")]
    [SerializeField] int defaultLayer = 6;
    [SerializeField] int immunityLayer = 7;
    bool lesbianPanicActive = false;



    CinemachineImpulseSource impulseSource;
    void Awake()
    {
        health = GetComponent<PlayerHealth>();
        playerController = GetComponent<PlayerController>();
        sprite = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }
    void Start()
    {
        cam = Camera.main;
    }

    Vector3 GetMousePosition()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        return mousePos;
    }


    public void UseUltimate(bool addHeart = true, bool freeUse = false)
    {
        if (health.CurrentHealth >= health.MaxHealth || freeUse)
        {
            Debug.Log("Ultimate used!");
            if (!freeUse)
            {
                health.TakeDamage(health.MaxHealth * ultimateMaxHealthCost);
            }
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, ultimateRadius, enemyLayers);
            impulseSource.GenerateImpulse(force: 1f);

            foreach (var enemy in hits)
            {
                //Debug.Log("enemy detected: " + enemy);
                enemy.GetComponent<IDamageable>()?.TakeDamage(ultimateDamage, 2f);
                health.Heal(ultimateDamage*ultimateLifesteal);
            }
            if (addHeart)
            {
                health.AddHeart();
            }

        }
        else
        {
            Debug.Log("Not enough blood!");
        }

    }

    public void UseBasicAttack()
    {
        if (lastBasicAttackTime + basicAttackCooldown > Time.time || lesbianPanicActive)
        {
            return;
        }
        Debug.Log("Basic attack used!");
        Vector2 attackPoint = GetAttackPoint();
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint, basicAttackRadius, enemyLayers);
        Debug.Log(hits);
        if (hits.Length > 0)
        {
            impulseSource.GenerateImpulse(force: 0.1f);
        }

        foreach (var enemy in hits)
        {
            enemy.GetComponent<IDamageable>()?.TakeDamage(basicAttackDamage);
            health.Heal(basicAttackDamage*basicAttackLifesteal);
        }
        lastBasicAttackTime = Time.time;
    }

    public void UseSpecialAttack()
    {
        if (lastSpecialAttackTime + specialAttackCooldown > Time.time || lesbianPanicActive)
        {
            return;
        }
        Debug.Log("Special attack used!");
        Vector3 mousePos = GetMousePosition();
        Vector2 direction = (mousePos - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, specialAttackRange, enemyLayers);
        Debug.DrawLine(transform.position, transform.position + (Vector3)direction*specialAttackRange, Color.red, 1f);
        IDamageable damageable = hit.collider?.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(specialAttackDamage);
            health.Heal(specialAttackDamage*specialAttackLifesteal);
            impulseSource.GenerateImpulse(0.3f);
        }
        lastSpecialAttackTime = Time.time;
    }

    public void LesbianPanic()
    {
        if (lastPanicTime + panicCooldown <= Time.time)
        {
            lastPanicTime = Time.time;
            StartCoroutine(LesbianPanicCoroutine());    
        }
        else
        {
            Debug.Log("Lesbian Panic still on cooldown! " + (lastPanicTime + panicCooldown - Time.time) + " seconds remaining");
        }
    }


    IEnumerator LesbianPanicCoroutine()
    {
        lesbianPanicActive = true;
        playerController.IncreaseSpeed(panicSpeedMultiplier);
        Color originalSpriteColor = sprite.color;
        sprite.color = Color.yellow;
        //col.enabled = false; // Zastąpione przez Layer
        gameObject.layer = immunityLayer;
        health.SetPanicked(true);
        playerController.Cleanse();
        playerController.ApplyStunImmunity(panicDuration);
        yield return new WaitForSeconds(panicDuration);
        lesbianPanicActive = false;
        playerController.NormalSpeed();
        sprite.color = originalSpriteColor;
        //col.enabled = true;
        gameObject.layer = defaultLayer;
        health.SetPanicked(false);
    }
    /*void OnDrawGizmos()
    {
        Gizmos.color = new Color (0.75f, 0f, 0f, 0.75f);
        Gizmos.DrawSphere(transform.position, ultimateRadius);
        Gizmos.color = Color.blue;
        Vector3 mousePos = GetMousePosition();
        Vector2 direction = (mousePos - transform.position).normalized; //doesn't display well, but fuck it - attacks are working
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction*specialAttackRange);
    }*/

    void OnDrawGizmos()
    {
        // basic attack
        Gizmos.color = Color.red;
        Vector2 attackPoint = GetAttackPoint();
        Gizmos.DrawWireSphere(attackPoint, basicAttackRadius);
        //special attack
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, ultimateRadius);
        Gizmos.color = Color.blue;
        Vector3 mousePos = GetMousePosition();
        Vector2 direction = (mousePos - transform.position).normalized; //doesn't display well, but fuck it - attacks are working
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction*specialAttackRange);
    }
    Vector2 GetAttackPoint()
    {
        Vector3 mousePos = GetMousePosition();
        Vector2 direction = (mousePos - transform.position).normalized;
        return (Vector2)transform.position + direction * basicAttackRange;
    }


}
