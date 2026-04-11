using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour
{

    Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SelfDestruct()
    {
        Destroy(gameObject);
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
