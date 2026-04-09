using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingRock : MonoBehaviour
{

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BeginFall(Vector3 targetPosition, Vector3 rockPosition, float time)
    {
        transform.position = rockPosition;
        Vector3 velocity = targetPosition - rockPosition / time;
        rb.velocity = velocity;
        StartCoroutine(DestroyRock(time));

    }
    IEnumerator DestroyRock(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}
