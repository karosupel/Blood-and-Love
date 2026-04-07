using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingSpikes : MonoBehaviour
{

    [SerializeField] float damage = 10f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        collision.GetComponent<IDamageable>().TakeDamage(damage);
    }
}
