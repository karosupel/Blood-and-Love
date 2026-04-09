using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    [SerializeField] float damage = 10f;
    [SerializeField] float radius = 1f;
    [SerializeField] float meteorDelay = 1f;
    [SerializeField] float meteorHeight;
    [SerializeField] float delay = 2f;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] GameObject target;
    [SerializeField] GameObject rock;

    float beginTime;
    bool meteorFalling = false;
    bool explosionTriggered = false;


    void Awake()
    {
        if (meteorDelay > delay)
        {
            meteorDelay = delay;
        }
        beginTime = Time.time;
    }

    // Start is called before the first frame update
    void Start()
    {
        beginTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (beginTime + meteorDelay <= Time.time && !meteorFalling)
        {
            Debug.Log("meteor activation requested");
            meteorFalling = true;
            rock.SetActive(true);
            rock.GetComponent<FallingRock>()?.BeginFall(target.transform.position, target.transform.position + new Vector3(0, meteorHeight, 0), delay - meteorDelay);
        }
        if (beginTime + delay <= Time.time && !explosionTriggered)
        {
            Debug.Log("Explosion!");
            explosionTriggered = true;
            Collider2D[] hits = Physics2D.OverlapCircleAll(target.transform.position, radius, playerLayer);
            
            foreach (var hit in hits)
            {
                hit.GetComponent<IDamageable>()?.TakeDamage(damage);
            }

            Destroy(gameObject);
            
        }
    }


    void OnDrawGizmos()
    {
        // basic attack
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.transform.position, radius);
    }
}
