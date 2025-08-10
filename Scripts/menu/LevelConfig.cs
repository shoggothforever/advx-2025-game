using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public string levelName;                    // ���عؿ���
    public string nextLevelName;                // �¹عؿ���
    public float timeLimit;                     // ʱ������
    public Vector3 playerSpawn;                 // ������
    public List<SpawnItem> items = new();       // ���п�������
}

[System.Serializable]
public class SpawnItem
{
    public GameObject prefab;   // ��Ԥ����
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale = Vector3.one;
}
