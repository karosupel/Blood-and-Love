using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HellishFlames : MonoBehaviour
{
    private Meteor parent;

    void Awake()
    {
        parent = GetComponentInParent<Meteor>();
    }
    
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            parent.OnChildTriggerStay(collision);
        }
        
    }
}
