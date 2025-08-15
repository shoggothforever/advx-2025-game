using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SaveYourself.Utils;
using SaveYourself.Core;
using SaveYourself.Model;
public class ChooseLevelPanel : MonoBehaviour
{
    [SerializeField] Transform content;
    [SerializeField] Button levelButtonPrefab;
    LevelConnection levels;
    bool opened = false;
    public void Awake()
    {
        levels=Resources.Load<LevelConnection>(Const.LevelConnectionPath);
    }
    private void OnEnable()
    {
        if (opened) return;
        // 清理旧按钮
        //foreach (Transform t in content) Destroy(t.gameObject);

        for (int i = 1; i < levels.items.Count; i++)
        {
            int idx = i; // 闭包捕获
            var btn = Instantiate(levelButtonPrefab, content);
            var scenename = levels.items[i];
            btn.GetComponentInChildren<Text>().text = scenename;
            btn.interactable = true;
            //Debug.Log("loading scene " + scenename);
            btn.onClick.AddListener(() => LoaderManager.Instance.LoadScene(scenename));
        }
        opened = true;
    }
}
