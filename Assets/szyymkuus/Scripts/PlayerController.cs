using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{

    [SerializeField]  public float basicSpeed = 5f;
    float speed;
    [SerializeField] float regenRate = 1;
    Rigidbody2D rb;
    PlayerAbilities abilities;
    Vector2 movement;
    PlayerHealth health;
    Collider2D col;
    Coroutine stunCoroutine;
    Coroutine stunImmunityCoroutine;

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





    }

    void FixedUpdate()
    {
        if (isStunned)
        {
            return;
        }
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 movementDirection = new Vector2 (horizontal, vertical).normalized;
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
    public void Cleanse() //Cleanse need fixing
    {
        Debug.Log("Cleanse!");
        StopCoroutine(stunCoroutine);
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
