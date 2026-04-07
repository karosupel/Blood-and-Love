using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTestScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //basic movement:
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        transform.position += new Vector3(moveX, moveY, 0).normalized * Time.deltaTime * 5f;
    }
}
