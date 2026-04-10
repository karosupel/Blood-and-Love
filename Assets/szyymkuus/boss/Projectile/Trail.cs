using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trail : MonoBehaviour
{

    TrailRenderer trail;
    void Awake()
    {
        trail = GetComponent<TrailRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        Color color = trail.GetComponentInParent<SpriteRenderer>().color;
        trail.startColor = new Color(color.r, color.g, color.b, 0.5f);
        trail.endColor = new Color(color.r, color.g, color.b, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
