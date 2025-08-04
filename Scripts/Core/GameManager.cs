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
        public List<TimeReverse.TimedAction> boxHistory; // ����/���ػط�����
    }
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public LevelCheckpoint checkpoint = new();
        public GameState currentState=GameState.PreReverseTime;
        public GameObject reverseWorld; // ��ʱ��
        public GameObject reversePlayer; // ��ʱ��
        public Cinemachine.CinemachineVirtualCamera reverseVirtualCamera; // ��ʱ�������
        public GameObject pastWorld;      // ��ʱ��
        public GameObject pastPlayer;      // ��ʱ��
        public Text countdownText;       // ������ʾ����ʱ��UI�ı�
        public List<WaterTransformer> waterTransformers; // ���ڿ���
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


        // ��ʼ��ʱ�ս׶�
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
            // ������ʱ����ң�������ʱ��AI

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
            Debug.Log("��ʱ�ս׶ο�ʼ������ " + TimeCountdown + " ��ʱ�䡣");
            countdownText.color = Color.red;
            countdownText.text = common.GetTimeCountDownStr(TimeCountdown);
        }

        // Ԥ����ʱ�ս׶�
        public void StartPreForwardTimePhase()
        {
            currentState = GameState.PreForwardTime;
            RemainTimeCount = Mathf.Max(TimeCountdown,0);
            TimeCountdown = getTimeLimit();
            // ������ʱ����ң�������ʱ��AI
            reversePlayer.GetComponent<Player>().controlEnabled = false;
            // Ԥ��ʱ����ʾ��ʱ�յ����
            //reversePlayer.SetActive(false);
            pastWorld.SetActive(true);
            Debug.Log("׼����ʼ��ʱ�ս׶Σ����� " + TimeCountdown + " ��ʱ�䡣");
            countdownText.color = Color.blue;
            countdownText.text = common.GetTimeCountDownStr(TimeCountdown) +"\n"+"���� R �� ����׼��";
        }    
        // ��ʼ��ʱ�ս׶�
        public void StartForwardTimePhase()
        {
            currentState = GameState.ForwardTime;
            pastPlayer.SetActive(true);
            EnableReverseSprite();
            //������������
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
            // ˮ�������
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
            // Ԥ��ʱ�䲻����ʱ
            if (TimeCountdown > 0 && (currentState != GameState.PreForwardTime || currentState != GameState.PreForwardTime))
            {
                TimeCountdown = TimeCountdown - Time.deltaTime;
                //Debug.Log("���� " + TimeCountdown + " ��ʱ�䡣");
                if(currentState == GameState.PreForwardTime || currentState == GameState.PreForwardTime)
                {
                    countdownText.text = "press Z to start";
                }
                    else countdownText.text = common.GetTimeCountDownStr(TimeCountdown);
            }// �������絹��ʱ������ʼ��������
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
        /* 1. ���浱ǰ�ؿ� */
        public void SaveCheckpoint()
        {
            checkpoint.levelName = SceneManager.GetActiveScene().name;
            checkpoint.playerPos = FindObjectOfType<Player>().transform.position;

            // ʾ����ÿ���ؿ��ĻطŹ��ܣ�������Ҫ�Լ�д�����ͽ�������
            //checkpoint.boxHistory = TimeManager.Instance.ExportHistory(); // �Լ�д����
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
                c.a = 0.4f;          // 40 % ͸���ȣ��ɵ�
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