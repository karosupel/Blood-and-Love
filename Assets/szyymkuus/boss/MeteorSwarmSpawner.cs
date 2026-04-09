using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorSwarmSpawner : MonoBehaviour
{
    BossAbilities bossAbilities;


    void Awake()
    {
        bossAbilities = GetComponent<BossAbilities>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Meteor Storm Incoming!");
            bossAbilities.MeteorStorm();
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Projectile Storm Incoming!");
            bossAbilities.ProjectileStorm();
        }
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Barrier Incoming!");
            bossAbilities.Barrier();
        }
    }
}
