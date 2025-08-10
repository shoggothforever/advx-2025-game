using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public string levelName;                    // 本关关卡名
    public string nextLevelName;                // 下关关卡名
    public float timeLimit;                     // 时间限制
    public Vector3 playerSpawn;                 // 出生点
    public List<SpawnItem> items = new();       // 所有可生成物
}

[System.Serializable]
public class SpawnItem
{
    public GameObject prefab;   // 拖预制体
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale = Vector3.one;
}
