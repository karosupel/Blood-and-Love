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
    Coroutine stun;

    bool isStunned;

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
        stun = StartCoroutine(StunCoroutine(duration));
    }
    public void Cleanse() //Cleanse need fixing
    {
        StopCoroutine(stun);
        isStunned = false;
    }
    IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }
}
