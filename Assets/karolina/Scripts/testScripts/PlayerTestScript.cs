using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTestScript : MonoBehaviour
{

    public bool isStunned = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //basic movement:
        if (!isStunned)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveY = Input.GetAxis("Vertical");

            transform.position += new Vector3(moveX, moveY, 0).normalized * Time.deltaTime * 5f;
        }
    }

    public IEnumerator Stun(float duration)
    {
        Debug.Log("Player stunned for " + duration + " seconds");
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
        Debug.Log("Player unstunned");
    }
}
