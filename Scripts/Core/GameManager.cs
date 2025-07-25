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
        public GameObject reverseWorld; // ��ʱ��
        public GameObject reversePlayer; // ��ʱ��
        public Cinemachine.CinemachineVirtualCamera reverseVirtualCamera; // ��ʱ�������
        public GameObject pastWorld;      // ��ʱ��
        public GameObject pastPlayer;      // ��ʱ��
        public Text countdownText;       // ������ʾ����ʱ��UI�ı�
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

        // ��ʼ��ʱ�ս׶�
        public void StartReverseTimePhase()
        {
            reversePlayer.GetComponent<Player>().controlEnabled = true;
            currentState = GameState.ReverseTime;
            TimeCountdown = reverseLevelInfos[levelIndex].duration;
            TimeManager.Instance.phase=TimeManager.Phase.Reverse;
            // ������ʱ����ң�������ʱ��AI
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
            Debug.Log("��ʱ�ս׶ο�ʼ������ " + TimeCountdown + " ��ʱ�䡣");
            countdownText.color = Color.red;
            countdownText.text = common.GetTimeCountDownStr(TimeCountdown);
        }

        // Ԥ����ʱ�ս׶�
        public void StartPreForwardTimePhase()
        {
            currentState = GameState.PreForwardTime;
            //TimeManager.Instance.UnRegister(reversePlayer.GetComponent<Mechanics.Player>());
            TimeCountdown = postiveLevelInfos[levelIndex].duration;
            // ������ʱ����ң�������ʱ��AI
            reverseWorld.SetActive(false);
            pastWorld.SetActive(true);
            Debug.Log("׼����ʼ��ʱ�ս׶Σ����� " + TimeCountdown + " ��ʱ�䡣");
            countdownText.color = Color.blue;
            countdownText.text = common.GetTimeCountDownStr(TimeCountdown);
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
            // ����һ���¼��������п���ת�������֮ǰ�Ĳ�������״̬
            // ������SendMessage���򻯣������͵���Ŀ�������¼�ϵͳ(UnityEvent/Action)
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
            // Ԥ��ʱ�䲻����ʱ
            if (TimeCountdown > 0 && currentState != GameState.PreForwardTime)
            {
                TimeCountdown = TimeCountdown - Time.deltaTime;
                //Debug.Log("���� " + TimeCountdown + " ��ʱ�䡣");
                countdownText.text = common.GetTimeCountDownStr(TimeCountdown);
            }// �������絹��ʱ������ʼ��������
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
            //�����ȥ���Լ�������յ㣬���ڵ��Լ���������㣬��ͨ����
            if (flagCount == 2 && currentState == GameState.ForwardTime)
            {
                Debug.Log("���������������⣬����ͨ����һ��");
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