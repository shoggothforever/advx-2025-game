using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SaveYourself.Utils;
using SaveYourself.Mechanics;
using static SaveYourself.Core.TimeReverse;
using SaveYourself.Interact;

namespace SaveYourself.Core
{
    public enum GameState { PreReverseTime, ReverseTime,PreForwardTime, ForwardTime, LevelComplete, LevelFailed }
    [System.Serializable]
    public class LevelCheckpoint
    {
        public string levelName;                 // Level_01
        public Vector3 playerPos;
        public List<TimeReverse.TimedAction> boxHistory; // 箱子/机关回放数据
    }
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public LevelCheckpoint checkpoint = new();
        public GameState currentState=GameState.PreReverseTime;
        public GameObject reverseWorld; // 逆时空
        public GameObject reversePlayer; // 逆时空
        public Cinemachine.CinemachineVirtualCamera reverseVirtualCamera; // 逆时空摄像机
        public GameObject pastWorld;      // 正时空
        public GameObject pastPlayer;      // 正时空
        public Text countdownText;       // 用于显示倒计时的UI文本
        public List<WaterTransformer> waterTransformers; // 用于控制
        private float TimeCountdown=10f;
        public float RemainTimeCount=0;
        readonly List<ITimeTrackable> trackedCache = new();
        public GameObject[] boxes;
        public string levelName;
        public string nextLevelName;
        public float timeLimit;
        Dictionary<GameState, bool> tracked = new();

        void Start()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else Destroy(gameObject);
            GetComponentsInChildren<ITimeTrackable>(true, trackedCache);
            boxes = GameObject.FindGameObjectsWithTag("Box");
            Debug.LogFormat("find boxes count:{0}", boxes.Length);
        }


        // 开始逆时空阶段
        public void StartReverseTimePhase()
        {
            RemainTimeCount = 0f;
            reverseWorld.SetActive(true);
            reversePlayer.SetActive(true);
            reversePlayer.GetComponent<Player>().controlEnabled = true;
            SetGhostPhysicsIgnoreCollision(false);
            currentState = GameState.ReverseTime;
            TimeCountdown = getTimeLimit();
            TimeManager.Instance.phase=TimeManager.Phase.Reverse;
            // 激活逆时空玩家，禁用正时空AI

            if (!tracked.ContainsKey(GameState.ReverseTime))
            {
                tracked.Add(GameState.ReverseTime,true);
                var per = reversePlayer.GetComponent<Mechanics.Player>();
                //Debug.LogFormat("get player id {0}", per.Id);
                TimeManager.Instance.Register(per);
                Debug.Log("register reverse player into TimeManager, ID: "+per.Id);
                if (boxes != null)
                {
                    foreach (var box in boxes)
                    {
                        if (box.name.StartsWith("reversible_box"))
                        {
                            var bx = box.GetComponent<BaseBox>();
                            TimeManager.Instance.Register(bx);
                            Debug.LogFormat("put box into TimeManager, ID:{0}", bx.Id);
                        }
                    }
                }
            }

            if (waterTransformers == null) { 
                Scene scene = SceneManager.GetSceneByName(levelName);
                if (!scene.isLoaded) return;
                waterTransformers = new();
                foreach (var t in LoaderManager.FindComponentsInScene<WaterTransformer>(levelName))
                {
                    if (t != null)
                    {
                        waterTransformers.Add(t);
                        Debug.Log("add waterTransform into game manager");
                    }

                }
            }
            foreach (var wt in waterTransformers)
            {
                wt.changeWater();
            }
            pastPlayer.SetActive(false);
            Debug.Log("逆时空阶段开始！你有 " + TimeCountdown + " 秒时间。");
            countdownText.color = Color.red;
            countdownText.text = common.GetTimeCountDownStr(TimeCountdown);
        }

        // 预备正时空阶段
        public void StartPreForwardTimePhase()
        {
            currentState = GameState.PreForwardTime;
            RemainTimeCount = Mathf.Max(TimeCountdown,0);
            TimeCountdown = getTimeLimit();
            // 禁用逆时空玩家，激活正时空AI
            reversePlayer.GetComponent<Player>().controlEnabled = false;
            // 预备时不显示逆时空的玩家
            //reversePlayer.SetActive(false);
            pastWorld.SetActive(true);
            Debug.Log("准备开始正时空阶段，你有 " + TimeCountdown + " 秒时间。");
            countdownText.color = Color.blue;
            countdownText.text = common.GetTimeCountDownStr(TimeCountdown) +"\n"+"按下 R 键 结束准备";
        }    
        // 开始正时空阶段
        public void StartForwardTimePhase()
        {
            currentState = GameState.ForwardTime;
            pastPlayer.SetActive(true);
            EnableReverseSprite();
            //箱子热胀冷缩
            foreach (var box in boxes)
            {
                if (box.name.StartsWith("shrink_box"))
                {
                    var bx = box.GetComponent<ShrinkBox>();
                    if (bx)
                    {
                        bx.EnlargeStable();
                    }
                }
            }
            // 水变成蒸汽
            if (waterTransformers != null)
            {
                foreach (var wt in waterTransformers)
                {
                    wt.changeWater();
                }
            }
        }

        void Update()
        {
            if (levelName!="SampleScene"&&!LoaderManager.Instance.isReady) return;
            if (currentState == GameState.PreReverseTime)
            {
                countdownText.text = "press Z to start";
                reversePlayer.GetComponent<Player>().controlEnabled = false;
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    StartReverseTimePhase();
                }
                return;
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (currentState == GameState.ReverseTime)
                {
                    StartPreForwardTimePhase();
                }
                else if (currentState == GameState.PreForwardTime)
                {
                    StartForwardTimePhase();
                }
                else
                {
                    StartReverseTimePhase();
                }
            }
            if (Input.GetKeyDown(KeyCode.P)) {
                Clear();
                TimeManager.Instance.Clear();
                LoaderManager.Instance.LoadScene(levelName);
            }
            // 预备时间不倒计时
            if (TimeCountdown > 0 && (currentState != GameState.PreForwardTime || currentState != GameState.PreForwardTime))
            {
                TimeCountdown = TimeCountdown - Time.deltaTime;
                //Debug.Log("你有 " + TimeCountdown + " 秒时间。");
                if(currentState == GameState.PreForwardTime || currentState == GameState.PreForwardTime)
                {
                    countdownText.text = "press Z to start";
                }
                    else countdownText.text = common.GetTimeCountDownStr(TimeCountdown);
            }// 逆熵世界倒计时结束开始正熵世界
            else if (TimeCountdown <=0 && currentState == GameState.ReverseTime)
            {
                StartPreForwardTimePhase();
            }
        }
        public void LoadNextScene()
        {
            Clear();
            TimeManager.Instance.Clear();
            LoaderManager.Instance.LoadScene(nextLevelName);
        }
        public float getTimeLimit()
        {
            return timeLimit;
        }
        /* 1. 保存当前关卡 */
        public void SaveCheckpoint()
        {
            checkpoint.levelName = SceneManager.GetActiveScene().name;
            checkpoint.playerPos = FindObjectOfType<Player>().transform.position;

            // 示例：每个关卡的回放功能，后续需要自己写导出和解析功能
            //checkpoint.boxHistory = TimeManager.Instance.ExportHistory(); // 自己写导出
        }
        private void EnableReverseSprite()
        {
            reverseWorld.SetActive(true);
            reversePlayer.SetActive(true);
            reversePlayer.GetComponent<Player>().controlEnabled = false;
            reversePlayer.GetComponent<Rigidbody2D>().simulated = false;
            SetGhostPhysicsIgnoreCollision(true);

            foreach (var sr in reversePlayer.GetComponentsInChildren<SpriteRenderer>())
            {
                Color c = sr.color;
                c.a = 0.4f;          // 40 % 透明度，可调
                sr.color = c;
            }
            reverseVirtualCamera.enabled = false;
        } 
        void SetGhostPhysicsIgnoreCollision(bool isIgnore)
        {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("GhostPlayer"), LayerMask.NameToLayer("Box"), isIgnore);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("GhostPlayer"), LayerMask.NameToLayer("Water"), isIgnore);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("GhostPlayer"), LayerMask.NameToLayer("Steam"), isIgnore);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("GhostPlayer"), LayerMask.NameToLayer("Player"), true);
        }
        public void Clear()
        {
            trackedCache.Clear();
            tracked.Clear();
            currentState = GameState.PreReverseTime;
            waterTransformers = null;
            boxes = null;
        }
    }
}