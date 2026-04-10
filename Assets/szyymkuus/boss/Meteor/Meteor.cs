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

    bool hellishVariant = false;

    float beginTime;
    bool meteorFalling = false;
    bool explosionTriggered = false;

    [SerializeField] float hellishFlamesDuration = 3f;
    [SerializeField] float hellishFlamesDamagePerSecond = 5f;
    bool hellishFlamesActive = false;


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
        if (hellishFlamesActive)
        {
            return;
        }
        if (beginTime + meteorDelay <= Time.time && !meteorFalling)
        {
            //Debug.Log("meteor activation requested");
            meteorFalling = true;
            rock.SetActive(true);
            rock.GetComponent<FallingRock>()?.BeginFall(target.transform.position, target.transform.position + new Vector3(0, meteorHeight, 0), delay - meteorDelay);
        }
        if (beginTime + delay <= Time.time && !explosionTriggered)
        {
            //Debug.Log("Explosion!");
            explosionTriggered = true;
            Collider2D[] hits = Physics2D.OverlapCircleAll(target.transform.position, radius * transform.localScale.x, playerLayer);
            
            foreach (var hit in hits)
            {
                hit.GetComponent<IDamageable>()?.TakeDamage(damage);
            }
            if (!hellishVariant)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                rock.SetActive(false);
                hellishFlamesActive = true;
                StartCoroutine(HellishFlamesCoroutine());
            }
            
        }
    }
    IEnumerator HellishFlamesCoroutine()
    {
        Collider2D col = GetComponentInChildren<Collider2D>();
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(hellishFlamesDuration*0.95f, hellishFlamesDuration);
        ps.Play();
        //Debug.Log(ps.isPlaying);
        col.enabled = true;
        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(0.1f, 0f, 0f, 0f);
        yield return new WaitForSeconds(hellishFlamesDuration);
        Destroy(gameObject);
    }

    public void SetVariant(bool isHellish)
    {
        hellishVariant = isHellish;
    }


    public void OnChildTriggerStay(Collider2D collision)
    {
        collision.GetComponent<IDamageable>().TakeDamage(hellishFlamesDamagePerSecond*Time.deltaTime);
    }


    //     void OnDrawGizmos()
    //     {
    //         // basic attack
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawWireSphere(target.transform.position, radius * transform.localScale.x);
    //     }
}
