using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{

    [SerializeField]  public float basicSpeed = 5f;
    float speed;
    //[SerializeField] float regenRate = 1;
    Rigidbody2D rb;
    PlayerAbilities abilities;
    Vector2 movement;
    PlayerHealth health;
    Collider2D col;
    Coroutine stunCoroutine;
    Coroutine stunImmunityCoroutine;

    float horizontal;
    float vertical;
    Vector2 movementDirection;
    Vector2 dashDirection;

    [Header("Dash settings")]
    [SerializeField] float dashVelocity = 500f;
    [SerializeField] float dashDuration = 0.15f;
    [SerializeField] float dashCooldown = 4f;
    float lastDashTime = float.MinValue;
    float dashTimer;
    bool isDashing;

    bool isStunned = false;
    int stunImmune = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        abilities = GetComponent<PlayerAbilities>();
        health = GetComponent<PlayerHealth>();
        speed = basicSpeed;
        col = GetComponent<BoxCollider2D>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isStunned)
        {
            return;
        }
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        movementDirection = new Vector2 (horizontal, vertical).normalized;
        if (Input.GetKeyDown(KeyCode.R))
        {
            abilities.UseUltimate();
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            abilities.UseBasicAttack();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            abilities.UseSpecialAttack();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            health.GoToHell();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            health.GoToMaterialPlane();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            health.TakeDamage(10f);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            health.Heal(10f);
        }
        if (Input.GetKeyDown(KeyCode.Space) && lastDashTime + dashCooldown <= Time.time && movementDirection != Vector2.zero)
        {
            dashDirection = movementDirection;
            dashTimer = dashDuration;
            isDashing = true;
            lastDashTime = Time.time;
        }




    }

    void FixedUpdate()
    {
        if (isStunned)
        {
            return;
        }
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            rb.velocity = dashDirection*dashVelocity;
            if (dashTimer <= 0)
            {
                isDashing = false;
            }
            return;
        }
        movement = movementDirection * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

    }

    public void IncreaseSpeed(float multiplier)
    {
        speed *= multiplier;
    }

    public void NormalSpeed()
    {
        speed = basicSpeed;
    }

    public void ApplyStun(float duration)
    {
        if (stunImmune > 0)
        {
            return;
        }
        stunCoroutine = StartCoroutine(StunCoroutine(duration));
        Debug.Log("started coroutine " + stunCoroutine);
    }
    public void ApplyStunImmunity(float duration)
    {
        StartCoroutine(StunImmunityCoroutine(duration));
    }
public void Cleanse() //swierk tu był naprawiać sry za grzebanie u ciebie uwu
{
    Debug.Log("Cleanse!");

    if (stunCoroutine != null)
    {
        StopCoroutine(stunCoroutine);
        stunCoroutine = null;
    }

    isStunned = false;
    Debug.Log("Is stunned: " + isStunned);
}
    IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
        StartCoroutine(StunImmunityCoroutine());
    }

    IEnumerator StunImmunityCoroutine(float duration = 1f)
    {
        stunImmune++;
        yield return new WaitForSeconds(duration);
        stunImmune--;
    }
}
