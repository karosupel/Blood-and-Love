using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{

    [SerializeField] float attackRange = 1f;
    [SerializeField] float attackRadius = 0.5f;
    [SerializeField] LayerMask enemyLayers;
    Camera cam;

    //TODO: dodać atak i promień podstawowego ataku
    [Header("Basic Attack")]
    [SerializeField] float basicAttackDamage = 10f;

    [Header("Special Attack")]
    [SerializeField] float specialAttackDamage = 10f;
    [SerializeField] float specialAttackRange = 5f;

    [Header("Ultimate")]
    [SerializeField] float ultimateDamage = 100f;
    [SerializeField] float ultimateRadius = 10f;
    PlayerHealth health;

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
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
            health.TakeDamage(health.MaxHealth * 0.7f);
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, ultimateRadius, enemyLayers);

            foreach (var enemy in hits)
            {
                enemy.GetComponent<IDamageable>()?.TakeDamage(ultimateDamage);
            }
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
        Vector2 attackPoint = (Vector2)transform.position + direction * attackRange;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint, attackRadius, enemyLayers);

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

}
