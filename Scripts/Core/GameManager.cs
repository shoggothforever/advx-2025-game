using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SaveYourself.Utils;
using SaveYourself.Model;
using SaveYourself.Mechanics;
using static SaveYourself.Core.TimeReverse;
using SaveYourself.Interact;

namespace SaveYourself.Core
{
    public enum GameState { PreReverseTime, ReverseTime,PreForwardTime, ForwardTime, LevelComplete, LevelFailed }
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public GameState currentState=GameState.PreReverseTime;
        public GameObject reverseWorld; // 逆时空
        public GameObject reversePlayer; // 逆时空角色
        public Cinemachine.CinemachineVirtualCamera reverseVirtualCamera; // 逆时空摄像机
        public GameObject pastWorld;      // 正时空
        public GameObject pastPlayer;      // 正时空角色
        public Vector3 originPastPlayPosition;
        public Text countdownText;       // 用于显示倒计时的UI文本
        public List<WaterTransformer> waterTransformers; // 用于控制
        private float TimeCountdown=10f; // 倒计时
        public float RemainTimeCount=0; // 节约的时间
        public GameObject[] boxes;
        public string levelName;
        public string nextLevelName;
        public float timeLimit;
        public List<ITimeTrackable> trackList = new();
        public bool timeStopped = false;
        private bool tracked = false;
        public LevelManager lm;
        void Start()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else Destroy(gameObject);
            boxes = GameObject.FindGameObjectsWithTag("Box");
            Debug.LogFormat("find boxes count:{0}", boxes.Length);
        }
        public void addTrack(ITimeTrackable t)
        {
            trackList.Add(t); 
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
            timeStopped = false;
            TimeManager.Instance.phase=TimeManager.Phase.Reverse;
            // 激活逆时空玩家，禁用正时空AI

            if (!tracked)
            {
                tracked=true;
                var per = reversePlayer.GetComponent<Mechanics.Player>();
                //Debug.LogFormat("get player id {0}", per.Id);
                TimeManager.Instance.Register(per);
                Debug.Log("register reverse player into TimeManager, ID: "+per.Id);
                Debug.LogFormat("will register {0} Items in TimeManager",trackList.Count);
                foreach (var t in trackList)
                {
                    TimeManager.Instance.Register(t);
                    Debug.LogFormat("put box into TimeManager, ID:{0}", t.Id);
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
        public void StartPreForwardTimePhase(bool first)
        {
            currentState = GameState.PreForwardTime;
            // 获取逆向时空节约下来的时间
            if(first)RemainTimeCount = Mathf.Max(TimeCountdown,0);
            // 恢复TimeCounddown
            TimeCountdown = getTimeLimit();
            // 禁用逆时空玩家，激活正时空AI
            reversePlayer.GetComponent<Player>().controlEnabled = false;
            // 预备时不显示逆时空的玩家
            reversePlayer.SetActive(false);
            pastWorld.SetActive(true);
            pastPlayer.SetActive(true);
            pastPlayer.GetComponent<Player>().controlEnabled = false;
            countdownText.color = Color.blue;
            countdownText.text = common.GetTimeCountDownStr(TimeCountdown) +"\n"+"按下 Z 键 结束准备";
        }    
        // 开始正时空阶段
        public void StartForwardTimePhase()
        {
            currentState = GameState.ForwardTime;
            pastPlayer.GetComponent<Player>().controlEnabled = true;
            EnableReverseSprite();
            //箱子热胀冷缩
            if (boxes != null)
            {
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

        public void OneRoll()
        {
            timeStopped = true;
        }

        void Update()
        {
            if (levelName!="SampleScene"&&!LoaderManager.Instance.isReady) return;
            if (Input.GetKeyDown(KeyCode.Q))
            {
                timeStopped = !timeStopped;
                controlTime(timeStopped);
            }
            if (timeStopped)
            {
                countdownText.text = "按下Q键恢复时间运行";
                return;
            }
            if (Input.GetKeyDown(KeyCode.P)) {
                LoaderManager.Instance.LoadScene(levelName);
            }
            // 预备时间不倒计时
            if (!timeStopped && TimeCountdown > 0 && (currentState != GameState.PreForwardTime || currentState != GameState.PreForwardTime))
            {
                TimeCountdown -= Time.deltaTime;
                //Debug.Log("你有 " + TimeCountdown + " 秒时间。");
                if (currentState == GameState.PreForwardTime || currentState == GameState.PreForwardTime)
                {
                    countdownText.text = "按下 Z 键 结束准备";
                }
                else countdownText.text = common.GetTimeCountDownStr(TimeCountdown);
            }// 逆熵世界倒计时结束开始正熵世界
            else if (TimeCountdown <= 0)
            {
                if (currentState == GameState.ReverseTime)
                    StartPreForwardTimePhase(true);
                else if (currentState == GameState.ForwardTime)
                {
                    lm.SetPasueMenu(true);
                }
            }
            if (currentState == GameState.PreReverseTime)
            {
                countdownText.text = "按下Z键 开始游戏";
                reversePlayer.GetComponent<Player>().controlEnabled = false;
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    StartReverseTimePhase();
                }
                return;
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (currentState == GameState.ReverseTime)
                {
                    StartPreForwardTimePhase(true);
                }
                else if (currentState == GameState.PreForwardTime)
                {
                    StartForwardTimePhase();
                }
            }
            if(Input.GetKeyDown(KeyCode.O))
            {
                RestartFromPreForward();
            }

        }
        public void LoadNextScene()
        {
            LoaderManager.Instance.LoadScene(nextLevelName);
        }
        public void RestartFromPreForward()
        {
            StartPreForwardTimePhase(false);
            pastPlayer.transform.position = originPastPlayPosition;
            TimeManager.Instance.RestartFromPreForward();
        }
        public float getTimeLimit()
        {
            return timeLimit;
        }
        public void controlTime(bool val)
        {
            timeStopped = val;
            if (val) Time.timeScale = 0;
            else Time.timeScale = 1;
            if(currentState==GameState.ForwardTime)pastPlayer.GetComponent<Player>().controlEnabled = !val;
            if (currentState == GameState.ReverseTime)reversePlayer.GetComponent<Player>().controlEnabled = !val;
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
            trackList.Clear();
            currentState = GameState.PreReverseTime;
            tracked = false;
            timeStopped = false;
            waterTransformers = null;
            boxes = null;
            lm = null;
        }
    }
}