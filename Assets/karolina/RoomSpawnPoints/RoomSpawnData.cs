using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Spawn/Room Spawn Data")]
public class RoomSpawnData : ScriptableObject
{
    public string roomTypeId;
    public List<EnemySpawnPoint> spawnPoints;
}