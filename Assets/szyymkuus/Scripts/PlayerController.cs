using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{

    [SerializeField] float movementSpeed = 5f;
    [SerializeField] float regenRate = 1;
    Rigidbody2D rb;
    PlayerAbilities abilities;
    Vector2 movement;
    PlayerHealth health;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        abilities = GetComponent<PlayerAbilities>();
        health = GetComponent<PlayerHealth>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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


        health.Heal(regenRate * Time.deltaTime);

    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 movementDirection = new Vector2 (horizontal, vertical).normalized;
        movement = movementDirection * movementSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

    }
}
