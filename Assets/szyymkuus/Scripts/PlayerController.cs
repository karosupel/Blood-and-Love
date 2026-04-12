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
    Animator animator;

    bool isStunned = false;
    int stunImmune = 0;

    [Header("Skill icons")]
    [SerializeField] private AbilityCooldownUI dashIcon;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        abilities = GetComponent<PlayerAbilities>();
        health = GetComponent<PlayerHealth>();
        speed = basicSpeed;
        col = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseMenuManager.IsPaused)
        {
            return;
        }

        if (isStunned)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        RotatePlayer();

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
        if (Input.GetKeyDown(KeyCode.Space) && lastDashTime + dashCooldown <= Time.time)
        {
            if (movementDirection != Vector2.zero)
            {
                dashDirection = movementDirection;
            }
            else
            {
                Vector3 mousePos = abilities.GetMousePosition();
                dashDirection = (mousePos - transform.position).normalized * -1f;
            }
            dashTimer = dashDuration;
            isDashing = true;
            animator.SetBool("isDashing", true);
            health.SetDashing(true);
            lastDashTime = Time.time;
            dashIcon.StartCooldown(dashCooldown);
        }
    }

    void RotatePlayer()
    {
        float direction;
        Vector2 mousePos = abilities.GetMousePosition();
        if (isDashing)
        {
            direction = dashDirection[0];
        }
        else
        {
            direction = mousePos.x - transform.position.x;
        }
            if (direction > 0)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
    }

    void FixedUpdate()
    {
        if (PauseMenuManager.IsPaused)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (isStunned)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            rb.velocity = dashDirection*dashVelocity;
            if (dashTimer <= 0)
            {
                isDashing = false;
                animator.SetBool("isDashing", false);
                health.SetDashing(false);
            }
            return;
        }
        movement = movementDirection * speed * Time.fixedDeltaTime;
        if (movement == Vector2.zero)
        {
            animator.SetBool("isWalking", false);
        }
        else
        {
            animator.SetBool("isWalking", true);
        }
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

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }

        stunCoroutine = StartCoroutine(StunCoroutine(duration));
        //Debug.Log("started coroutine " + stunCoroutine);
    }
    public void ApplyStunImmunity(float duration)
    {
        StartCoroutine(StunImmunityCoroutine(duration));
    }
public void Cleanse() //swierk tu był naprawiać sry za grzebanie u ciebie uwu
{
    //Debug.Log("Cleanse!");

    if (stunCoroutine != null)
    {
        StopCoroutine(stunCoroutine);
        stunCoroutine = null;
    }

    isStunned = false;
    //Debug.Log("Is stunned: " + isStunned);
}
    IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
            StopMovementAndDash();
        yield return new WaitForSeconds(duration);
        isStunned = false;
            stunCoroutine = null;
        StartCoroutine(StunImmunityCoroutine());
    }

        private void StopMovementAndDash()
        {
            isDashing = false;
            dashTimer = 0f;
            movementDirection = Vector2.zero;
            movement = Vector2.zero;
            rb.velocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            animator.SetBool("isDashing", false);
            health.SetDashing(false);
        }

    IEnumerator StunImmunityCoroutine(float duration = 1f)
    {
        stunImmune++;
        yield return new WaitForSeconds(duration);
        stunImmune--;
    }


}
