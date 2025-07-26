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
    struct LevelInfo
    {
        public int level;
        public float duration;
        public float startTime;
        public float endTime;
    }
    [System.Serializable]
    public class LevelCheckpoint
    {
        public string levelName;                 // Level_01
        public Vector3 playerPos;
        public List<TimeReverse.TimedAction> boxHistory; // 箱子/机关回放数据
    }
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public LevelCheckpoint checkpoint = new();
        static LevelInfo[] reverseLevelInfos = new LevelInfo[] {
        new LevelInfo { level = 1, duration = 10, startTime = 50, endTime = 40 },
        new LevelInfo { level = 2, duration = 10, startTime = 40, endTime = 30 },
        new LevelInfo { level = 3, duration = 10, startTime = 30, endTime = 20 },
        new LevelInfo { level = 4, duration = 10, startTime = 20, endTime = 10 },
        new LevelInfo { level = 5, duration = 10, startTime = 10, endTime = 0 },
        };
        static LevelInfo[] postiveLevelInfos = new LevelInfo[] {
        new LevelInfo { level = 1, duration = 10, startTime = 80, endTime = 100 },
        new LevelInfo { level = 2, duration = 10, startTime = 60, endTime = 80 },
        new LevelInfo { level = 3, duration = 10, startTime = 40, endTime = 60 },
        new LevelInfo { level = 4, duration = 10, startTime = 20, endTime = 40 },
        new LevelInfo { level = 5, duration = 10, startTime = 0, endTime = 20 },
        };
        public GameState currentState=GameState.ReverseTime;
        public GameObject reverseWorld; // 逆时空
        public GameObject reversePlayer; // 逆时空
        public Cinemachine.CinemachineVirtualCamera reverseVirtualCamera; // 逆时空摄像机
        public GameObject pastWorld;      // 正时空
        public GameObject pastPlayer;      // 正时空
        public Text countdownText;       // 用于显示倒计时的UI文本
        public WaterTransformer[] waterTransformers; // 用于控制
        private int flagCount = 0;
        private float TimeCountdown;
        readonly List<ITimeTrackable> trackedCache = new();
        public GameObject[] boxes;
        public string levelName;
        public string nextLevelName;
        public float timeLimit;
        Dictionary<GameState, bool> tracked = new();

        void Start()
        {
            if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
            else Destroy(gameObject);
            GetComponentsInChildren<ITimeTrackable>(true, trackedCache);
            boxes = GameObject.FindGameObjectsWithTag("Box");
            Debug.LogFormat("find boxes count:{0}", boxes.Length);
        }


        // 开始逆时空阶段
        public void StartReverseTimePhase()
        {
            reversePlayer.SetActive(true);
            reversePlayer.GetComponent<Player>().enabled = true;
            currentState = GameState.ReverseTime;
            TimeCountdown = getTimeLimit();
            TimeManager.Instance.phase=TimeManager.Phase.Reverse;
            // 激活逆时空玩家，禁用正时空AI
            reverseWorld.SetActive(true);
            if (!tracked.ContainsKey(GameState.ReverseTime))
            {
                tracked[GameState.ReverseTime] = true;
                var per = reversePlayer.GetComponent<Mechanics.Player>();
                //Debug.LogFormat("get player id {0}", per.Id);
                TimeManager.Instance.Register(per);
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
            if (waterTransformers != null)
            {
                foreach (var wt in waterTransformers)
                {
                    wt.changeWater();
                }
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
            TimeCountdown = getTimeLimit();
            // 禁用逆时空玩家，激活正时空AI
            reverseWorld.SetActive(false);
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
            if (!LoaderManager.Instance.isReady) return;
            if (Input.GetKeyDown(KeyCode.Z))
            {
                StartReverseTimePhase();
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
                LoadLevel(levelName);
            }
            // 预备时间不倒计时
            if (TimeCountdown > 0 && currentState != GameState.PreForwardTime)
            {
                TimeCountdown = TimeCountdown - Time.deltaTime;
                //Debug.Log("你有 " + TimeCountdown + " 秒时间。");
                countdownText.text = common.GetTimeCountDownStr(TimeCountdown);
            }// 逆熵世界倒计时结束开始正熵世界
            else if (currentState == GameState.ReverseTime)
            {
                StartPreForwardTimePhase();
            }
        }
        public void LoadNextScene()
        { 
            LoadLevel(nextLevelName);
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

        /* 2. 异步加载关卡 */
        public void LoadLevel(string levelName)
        {
            StartCoroutine(LoadAsync(levelName));
        }

        IEnumerator LoadAsync(string levelName)
        {
            // 淡出 UI
            //FadeCanvas.FadeOut(0.3f);

            AsyncOperation op = SceneManager.LoadSceneAsync(levelName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f) yield return null;
            op.allowSceneActivation = true;

            // 场景加载完成后
            yield return new WaitForEndOfFrame();
            RestoreCheckpoint();
        }

        /* 3. 恢复存档 */
        void RestoreCheckpoint()
        {
            if (checkpoint.levelName != SceneManager.GetActiveScene().name)
                return;  // 第一次进本关

            Player p = FindObjectOfType<Player>();
            if (p) p.transform.position = checkpoint.playerPos;
            // 恢复存档机制
            //TimeManager.Instance.ImportHistory(checkpoint.boxHistory);
        }
        private void EnableReverseSprite()
        {
            reverseWorld.SetActive(true);
            reversePlayer.GetComponent<Player>().controlEnabled = false;
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("GhostPlayer"), LayerMask.NameToLayer("Box"), true);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("GhostPlayer"), LayerMask.NameToLayer("Water"), true);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("GhostPlayer"), LayerMask.NameToLayer("Steam"), true);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("GhostPlayer"), LayerMask.NameToLayer("Player"), true);

            foreach (var sr in reversePlayer.GetComponentsInChildren<SpriteRenderer>())
            {
                Color c = sr.color;
                c.a = 0.4f;          // 40 % 透明度，可调
                sr.color = c;
            }
            reverseVirtualCamera.enabled = false;
        } 
        public void Clear()
        {
            tracked.Clear();
        }
    }
}