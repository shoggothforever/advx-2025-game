using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level/LevelConnection")]
public class LevelConnection : ScriptableObject
{
    public List<string> items = new();       // 所有可生成物
}
