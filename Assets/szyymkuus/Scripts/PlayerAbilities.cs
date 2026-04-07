using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{

    [SerializeField] LayerMask enemyLayers;
    Camera cam;

    //TODO: dodać atak i promień podstawowego ataku
    [Header("Basic Attack")]
    [SerializeField] float basicAttackDamage = 10f;
    [SerializeField] float basicAttackRange = 1f;
    [SerializeField] float basicAttackRadius = 0.5f;


    [Header("Special Attack")]
    [SerializeField] float specialAttackDamage = 10f;
    [SerializeField] float specialAttackRange = 5f;

    [Header("Ultimate")]
    [SerializeField] float ultimateDamage = 100f;
    [SerializeField] float ultimateRadius = 10f;
    [SerializeField] float ultimateMaxHealthCost = 0.7f;
    [Header("Lesbian Panic")]
    [SerializeField] public float panicDuration = 3f;
    [SerializeField] public float panicCooldown = 60f;
    [SerializeField] public float panicSpeedMultiplier = 1.5f;
    float lastPanicTime = float.MinValue;
    PlayerController playerController;
    BoxCollider2D col;
    PlayerHealth health;
    SpriteRenderer sprite;

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
        playerController = GetComponent<PlayerController>();
        sprite = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
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


    public void UseUltimate()
    {
        if (health.CurrentHealth >= health.MaxHealth)
        {
            Debug.Log("Ultimate used!");
            health.TakeDamage(health.MaxHealth * ultimateMaxHealthCost);
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, ultimateRadius, enemyLayers);

            foreach (var enemy in hits)
            {
                enemy.GetComponent<IDamageable>()?.TakeDamage(ultimateDamage);
            }
            health.AddHeart();
        }
        else
        {
            Debug.Log("Not enough blood!");
        }

    }

    public void UseBasicAttack()
    {
        Debug.Log("Basic attack used!");
        Vector3 mousePos = GetMousePosition();
        Vector2 direction = (mousePos - transform.position).normalized;
        Vector2 attackPoint = (Vector2)transform.position + direction * basicAttackRange;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint, basicAttackRadius, enemyLayers);

        foreach (var enemy in hits)
        {
            enemy.GetComponent<IDamageable>()?.TakeDamage(basicAttackDamage);
        }
    }

    public void UseSpecialAttack()
    {
        Debug.Log("Special attack used!");
        Vector3 mousePos = GetMousePosition();
        Vector2 direction = (mousePos - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, specialAttackRange, enemyLayers);
        Debug.DrawLine(transform.position, transform.position + (Vector3)direction*specialAttackRange, Color.red, 10f);

        hit.collider?.GetComponent<IDamageable>()?.TakeDamage(specialAttackDamage);

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
         playerController.IncreaseSpeed(panicSpeedMultiplier);
         Color originalSpriteColor = sprite.color;
         sprite.color = Color.yellow;
         col.enabled = false;
         yield return new WaitForSeconds(panicDuration);
         playerController.NormalSpeed();
         sprite.color = originalSpriteColor;
         col.enabled = true;
    }


}
