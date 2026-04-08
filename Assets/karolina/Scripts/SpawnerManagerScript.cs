using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManagerScript : MonoBehaviour
{
    //references:
    [SerializeField] public GameObject succubusPrefab;
    [SerializeField] public GameObject maidPrefab;

    public RoomManager roomManagerScript;

    [SerializeField] public List<Enemy> ActiveEnemiesInScene = new List<Enemy>();

    //variables:
    public List<string> roomVisitStack = new List<string>();
    public RoomInstance instanceId;
    public RoomInstance roomTypeId;

    private void Awake()
    {
        roomManagerScript = GameObject.FindGameObjectWithTag("RoomManager").GetComponent<RoomManager>();
        roomTypeId = roomManagerScript.RoomInstance.roomTypeId;
        instanceId = roomManagerScript.RoomInstance.instanceId;
    }

    private void Update()
    {
        if (roomVisitStackChanges())
        {
            roomTypeId = roomManagerScript.RoomInstance.roomTypeId;
            instanceId = roomManagerScript.RoomInstance.instanceId;
        }

        switch(roomTypeId)
        {
            case ("Blue"):
                //twoja matka
                break;
            case ("Red"):
                //twoja matka
                break;
            case ("Green"):
                //twoja matka
                break;
            case ("Boss"):
                //twoja matka
                break;
            case ("Yellow"):
                //twoja matka
                break;
        }
    }

    public bool roomVisitStackChanges()
    {
        int currentStackSize = roomVisitStack.Count;
        int newStackSize = roomManagerScript.RoomInstance.roomVisitStack.Count;

        if (currentStackSize != newStackSize)
        {
            roomVisitStack = new List<string>(roomManagerScript.RoomInstance.roomVisitStack);
            return true;
        }
        return false;
    }

    public void SpawnEnemyBlue(GameObject succubusPrefab, GameObject maidPrefab)
    {
        //spawn logic for blue room
    }

}
