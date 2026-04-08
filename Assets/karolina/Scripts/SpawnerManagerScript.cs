using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManagerScript : MonoBehaviour
{
    //references:
    [SerializeField] public GameObject succubusPrefab;
    [SerializeField] public GameObject maidPrefab;

    [SerializeField] public GameObject roomManager;

    [SerializeField] public GameObject player;

    [SerializeField] public PlayerHealth playerHealthScript;
    private RoomManager roomManagerScript;

    [SerializeField] public List<GameObject> ActiveEnemiesInScene = new List<GameObject>();

    //variables:
    public List<string> roomVisitStack = new List<string>();
    public string instanceId;
    public string roomTypeId;

    private void Start()
    {
        roomManagerScript = roomManager.GetComponent<RoomManager>();
        roomVisitStack = new List<string>(roomManagerScript.roomVisitStack);
        playerHealthScript = player.GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (roomVisitStackChanges())
        {
            switch(roomTypeId)
            {
                case ("Blue_"):
                    Debug.Log("Blue Room");
                    SpawnEnemyBlue(succubusPrefab, maidPrefab);
                    break;
                case ("Red_"):
                    //twoja matka
                    break;
                case ("Green_"):
                    //twoja matka
                    break;
                case ("Boss_"):
                    //twoja matka
                    break;
                case ("Yellow_"):
                    //twoja matka
                    break;
                default:
                    //twoja matka
                    break;
            }
        }

        if(playerHealthScript.IsInAfterlife)
        {
            HideEnemies();
        }
        else if (!playerHealthScript.IsInAfterlife)
        {
            ShowEnemies();
        }

    }

    public bool roomVisitStackChanges()
    {
        int currentStackSize = roomVisitStack.Count;
        int newStackSize = roomManagerScript.roomVisitStack.Count;

        if (currentStackSize != newStackSize)
        {
            roomVisitStack = new List<string>(roomManagerScript.roomVisitStack);
            roomTypeId = roomVisitStack[roomVisitStack.Count - 1].Substring(0, roomVisitStack[roomVisitStack.Count - 1].Length - 1);
            instanceId = roomVisitStack[roomVisitStack.Count - 1].Substring(roomVisitStack[roomVisitStack.Count - 1].Length - 1);
            return true;
        }
        return false;
    }

    public void SpawnEnemyBlue(GameObject succubusPrefab, GameObject maidPrefab)
    {
        GameObject succubus1 = Instantiate(succubusPrefab, new Vector3(-5, -11, 0), Quaternion.identity);
        ActiveEnemiesInScene.Add(succubus1);
    }

    public void HideEnemies()
    {
        foreach (GameObject enemy in ActiveEnemiesInScene)
        {
            if (enemy == null)
            {
                continue;
            }
            enemy.SetActive(false);
        }  
    }

    public void ShowEnemies()
    {
        foreach (GameObject enemy in ActiveEnemiesInScene)
        {
            if (enemy == null)
            {
                continue;
            }
            enemy.SetActive(true);
        }
    }

    //public void SpawnEnemiesInHell(string roomTypeId, )

}
