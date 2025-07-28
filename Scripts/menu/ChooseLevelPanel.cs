using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseLevelPanel : MonoBehaviour
{
    [SerializeField] Transform content;
    [SerializeField] Button levelButtonPrefab;

    private void OnEnable()
    {
        // 清理旧按钮
        foreach (Transform t in content) Destroy(t.gameObject);
        string[] levelNames = new string[]
        {
            "Tutorial1",
            "Tutorial2",
            "Tutorial3",
            "Level1",
            "Level2",
            "Level3",
            "finalLevel"
        };
        for (int i = 0; i < levelNames.Length; i++)
        {
            int idx = i; // 闭包捕获
            var btn = Instantiate(levelButtonPrefab, content);
            var scenename = levelNames[i];
            btn.GetComponentInChildren<Text>().text = scenename;
            btn.interactable = true;
            Debug.Log("loading scene " + scenename);
            btn.onClick.AddListener(() => LoaderManager.Instance.LoadScene(scenename));
        }
    }
}
