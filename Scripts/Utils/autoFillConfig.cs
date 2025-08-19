#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public static class autoFillConfig
{
    [MenuItem("Tools/Fill Scene Config")]
    static void Fill()
    {
        Scene scene = SceneManager.GetActiveScene();
        var sceneName = scene.name;
        Debug.Log("scene: "+sceneName);
        //Resources.Load<LevelConfig>
        LevelConfig config =AssetDatabase.LoadAssetAtPath<LevelConfig>($"Assets/Resources/Configs/{sceneName}.asset");
        if (config == null)
        {
            Debug.Log("can not find the config ");
        }
        config.levelName = sceneName;
        string[] keyItem = new string[]
        {
            "BeginPos",
            "EndPos",
            "Player_Variant",
            "Player_reverse",
            "Canvas",
        };
        string[] necessaryItem = new string[] {"Canvas" };
        Dictionary<string,GameObject> keys = new Dictionary<string, GameObject>();
        foreach (var go in scene.GetRootGameObjects())
        {
            for (int i=0;i<keyItem.Length; i++)
            {
                if (go.name.StartsWith(keyItem[i])){
                    keys[keyItem[i]] = go;
                    Debug.Log($"add {keyItem[i]}");
                }
            }
        }
        foreach(var key in keyItem)
        {
            if (keys.ContainsKey(key))
            {
                if(key=="Player_Variant")
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefabs/reversible/Player_Variant.prefab");
                    config.items.Add(
                    new SpawnItem
                    {
                        prefab = go,
                        position = keys[key].transform.position,
                        rotation = keys[key].transform.eulerAngles,
                        scale = keys[key].transform.localScale,
                    }
                    );
                }
                else if(key=="Player_reverse")
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefabs/reversible/Player_reverse_Variant.prefab");
                    config.items.Add(
                    new SpawnItem
                    {
                        prefab = go,
                        position = keys[key].transform.position,
                        rotation = keys[key].transform.eulerAngles,
                        scale = keys[key].transform.localScale,
                    }
                    );
                }
                else Add(keys[key], config);
                
            }
        }
        for (int i = 0; i < necessaryItem.Length; i++)
        {
            if (!keys.ContainsKey(necessaryItem[i]))
            {
                if (necessaryItem[i] == "Canvas")
                {
                    Debug.Log($"search {keyItem[i]}");
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scripts/level/Canvas.prefab");
                    if (go == null) Debug.Log($"can not find {necessaryItem[i]}");
                    config.items.Add(
                     new SpawnItem
                     {
                         prefab = go,
                         position = go.transform.position,
                         rotation = go.transform.eulerAngles,
                         scale = go.transform.localScale,
                     });
                }
            }
        }
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"fill {sceneName}.asset successfully");
    }
    static void Add(GameObject go,LevelConfig lc)
    {
        if (go == null) return;
        Debug.Log($"add {go.name} properties into config list");
        lc.items.Add(
            new SpawnItem
            {
                prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go),
                position=go.transform.position,
                rotation= go.transform.position,
                scale=go.transform.localScale,
            }
            );
    }
}

#endif