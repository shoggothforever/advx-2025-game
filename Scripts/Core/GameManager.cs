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

public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
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
        private static int levelIndex = 0;
        private int flagCount = 0;
        private float TimeCountdown;
        readonly List<ITimeTrackable> trackedCache = new();
        GameObject[] boxes;
        Dictionary<GameState, bool> tracked = new();

        void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(gameObject);
            GetComponentsInChildren<ITimeTrackable>(true, trackedCache);
        }

        void Start()
        {
            reversePlayer.GetComponent<Player>().controlEnabled = false;
            boxes = GameObject.FindGameObjectsWithTag("Box");
            Debug.LogFormat("find boxes count:{0}",boxes.Length);
        }

        // 开始逆时空阶段
        public void StartReverseTimePhase()
        {
            reversePlayer.GetComponent<Player>().controlEnabled = true;
            currentState = GameState.ReverseTime;
            TimeCountdown = reverseLevelInfos[levelIndex].duration;
            TimeManager.Instance.phase=TimeManager.Phase.Reverse;
            // 激活逆时空玩家，禁用正时空AI
            reverseWorld.SetActive(true);
            if (!tracked.ContainsKey(GameState.ReverseTime))
            {
                tracked[GameState.ReverseTime] = true;
                var per = reversePlayer.GetComponent<Mechanics.Player>();
                Debug.LogFormat("get player id {0}", per.Id);
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
            pastWorld.SetActive(false);
            Debug.Log("逆时空阶段开始！你有 " + TimeCountdown + " 秒时间。");
            countdownText.color = Color.red;
            countdownText.text = common.GetTimeCountDownStr(TimeCountdown);
        }

        // 预备正时空阶段
        public void StartPreForwardTimePhase()
        {
            currentState = GameState.PreForwardTime;
            //TimeManager.Instance.UnRegister(reversePlayer.GetComponent<Mechanics.Player>());
            TimeCountdown = postiveLevelInfos[levelIndex].duration;
            // 禁用逆时空玩家，激活正时空AI
            reverseWorld.SetActive(false);
            pastWorld.SetActive(true);
            Debug.Log("准备开始正时空阶段，你有 " + TimeCountdown + " 秒时间。");
            countdownText.color = Color.blue;
            countdownText.text = common.GetTimeCountDownStr(TimeCountdown);
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
            // 触发一个事件，让所有可逆转物体根据之前的操作更新状态
            // 我们用SendMessage来简化，更大型的项目建议用事件系统(UnityEvent/Action)
            BroadcastMessage("OnForwardTimeStart", SendMessageOptions.DontRequireReceiver);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
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
            if (gamePassCheck())
            {
                levelIndex++;
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
        void LoadNextScene()
        {
            int nextSceneIndex = levelIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
        }
        bool gamePassCheck()
        {
            //如果过去的自己到达的终点，现在的自己到达了起点，就通关了
            if (flagCount == 2 && currentState == GameState.ForwardTime)
            {
                Debug.Log("正反世界能量均衡，可以通往下一关");
                currentState = GameState.LevelComplete;
                return true;
            }
            return false;
        }
        private Dictionary<GameState, bool> flags = new Dictionary<GameState, bool>();
        public int setFlagCount()
        {
            if (!flags[GameState.ReverseTime]){
                flagCount++; 
                flags[GameState.ReverseTime] = true;
            }
            if (!flags[GameState.ForwardTime])
            {
                flagCount++;
                flags[GameState.ForwardTime] = true;
            }
            return flagCount;
        }
        public float getTimeLimit()
        {
            return reverseLevelInfos[levelIndex].duration;
        }
        private void EnableReverseSprite()
        {
            reverseWorld.SetActive(true);
            reversePlayer.GetComponent<Player>().controlEnabled = false;
            reverseVirtualCamera.enabled = false;
        } 
    }
}