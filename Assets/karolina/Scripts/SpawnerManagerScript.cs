using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

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

    [SerializeField] public List<GameObject> EnemiesInAfterlife = new List<GameObject>();

    [SerializeField] public List<string> VisitedRooms = new List<string>();

    private HashSet<string> visitedRooms = new HashSet<string>();

    private Dictionary<string, RoomSpawnData> spawnDataDict;

    [SerializeField] private List<RoomSpawnData> spawnDataList;

    private Dictionary<string, System.Action> spawnActions;

    #nullable enable
    [SerializeField] private PopUpManager? popUpManagerScript;
    #nullable disable
    

    public List<string> roomVisitStack = new List<string>();
    public string instanceId;
    public string roomTypeId;

    public bool areEnemiesDead = false;
    private bool wasInAfterlife = false;

    [SerializeField] private string tutorialSceneName = "Tutorial";

    private void Start()
    {
        roomManagerScript = roomManager.GetComponent<RoomManager>();
        roomVisitStack = new List<string>(roomManagerScript.roomVisitStack);
        playerHealthScript = player.GetComponent<PlayerHealth>();

        spawnActions = new Dictionary<string, System.Action>()
        {
            { "Blue_", () => SpawnEnemies() },
            { "Red_", () => SpawnEnemies() },
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
            string roomKey = roomTypeId + instanceId;

            if (!visitedRooms.Contains(roomKey))
            {
                if (spawnActions.TryGetValue(roomTypeId, out var spawnAction))
                {
                    spawnAction.Invoke();
                    visitedRooms.Add(roomKey);
                }
            }
            else
            {
                Debug.Log("Room already visited: " + roomKey);
            }
        }

        foreach (GameObject enemy in ActiveEnemiesInScene)
        {
            if (enemy == null)
            {
                ActiveEnemiesInScene.Remove(enemy);
                break;
            }
        }

        foreach (GameObject enemy in EnemiesInAfterlife)
        {
            if (enemy == null)
            {
                EnemiesInAfterlife.Remove(enemy);
                break;
            }
        }

        // Sprawdzenie czy IsInAfterlife się zmieniła
        if(playerHealthScript.IsInAfterlife != wasInAfterlife)
        {
            wasInAfterlife = playerHealthScript.IsInAfterlife;
            
            if(playerHealthScript.IsInAfterlife)
            {
                HideEnemies();
                SpawnEnemiesInHell();
            }
            else
            {
                ShowEnemies();
            }
        }

        if(playerHealthScript.IsInAfterlife && !EnemiesInAfterlife.Any() && !IsInTutorialScenePhase1())
        {
            playerHealthScript.GoToMaterialPlane();
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

    public void SpawnEnemies()
    {
        if (!spawnDataDict.TryGetValue(roomTypeId, out var data))
            return;

        foreach (var spawn in data.spawnPoints)
        {
            var enemy = Instantiate(spawn.enemyPrefab, spawn.position, Quaternion.identity);
            enemy.GetComponent<SpriteRenderer>().color = enemy.GetComponent<Enemy>().materialPlaneColor;
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

    public void SpawnEnemiesInHell()
    {
        Vector3 offset = new Vector3(-30f, 0, 0);
        if (!spawnDataDict.TryGetValue(roomTypeId, out var data))
            return;

        foreach (var spawn in data.spawnPoints)
        {
            var enemy = Instantiate(spawn.enemyPrefab, spawn.position + offset, Quaternion.identity);
            enemy.GetComponent<SpriteRenderer>().color = enemy.GetComponent<Enemy>().afterlifeColor;
            EnemiesInAfterlife.Add(enemy);
        }
    }

    public void SpawnEnemyRed() { }
    public void SpawnEnemyGreen() { }
    public void SpawnEnemyBoss() { }
    public void SpawnEnemyYellow() { }

    public List<GameObject> GetActiveEnemiesInScene()
    {
        return ActiveEnemiesInScene;
    }

    private bool IsInTutorialScenePhase1()
    {
        if (SceneManager.GetActiveScene().name == tutorialSceneName)
        {
            if (popUpManagerScript.phase == 1 || popUpManagerScript.phase == 2)
            {
                return true;
            }
        }
        return false;
    }

}
