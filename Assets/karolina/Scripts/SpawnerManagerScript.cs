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

    //variables:
    [SerializeField] public List<GameObject> ActiveEnemiesInScene = new List<GameObject>();

    private Dictionary<string, RoomSpawnData> spawnDataDict;

    [SerializeField] private List<RoomSpawnData> spawnDataList;

    private Dictionary<string, System.Action> spawnActions;

    public List<string> roomVisitStack = new List<string>();
    public string instanceId;
    public string roomTypeId;

    private void Start()
    {
        roomManagerScript = roomManager.GetComponent<RoomManager>();
        roomVisitStack = new List<string>(roomManagerScript.roomVisitStack);
        playerHealthScript = player.GetComponent<PlayerHealth>();

        spawnActions = new Dictionary<string, System.Action>()
        {
            { "Blue_", () => SpawnEnemies() },
            { "Red_", SpawnEnemyRed },
            { "Green_", SpawnEnemyGreen },
            { "Boss_", SpawnEnemyBoss },
            { "Yellow_", SpawnEnemyYellow }
        };

        spawnDataDict = new Dictionary<string, RoomSpawnData>();

        foreach (var data in spawnDataList)
        {
            spawnDataDict[data.roomTypeId] = data;
        }
    }

    private void Update()
    {
        if (roomVisitStackChanges())
        {
            if (spawnActions.TryGetValue(roomTypeId, out var spawnAction))
            {
                spawnAction.Invoke();
            }
            else
            {
                Debug.LogWarning("Unknown room type: " + roomTypeId);
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

    /*public void SpawnEnemyBlue(GameObject succubusPrefab, GameObject maidPrefab)
    {
        if (!spawnPointsDict.TryGetValue("Blue_", out var points))
            return;

        foreach (var point in points)
        {
            GameObject enemy = Instantiate(succubusPrefab, point, Quaternion.identity);
            ActiveEnemiesInScene.Add(enemy);
        }
    }*/

    public void SpawnEnemies()
    {
        if (!spawnDataDict.TryGetValue(roomTypeId, out var data))
            return;

        foreach (var spawn in data.spawnPoints)
        {
            var enemy = Instantiate(spawn.enemyPrefab, spawn.position, Quaternion.identity);
            ActiveEnemiesInScene.Add(enemy);
        }
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

    public void SpawnEnemyRed() { }
    public void SpawnEnemyGreen() { }
    public void SpawnEnemyBoss() { }
    public void SpawnEnemyYellow() { }

}
