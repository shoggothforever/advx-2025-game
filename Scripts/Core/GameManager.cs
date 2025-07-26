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
        public List<TimeReverse.TimedAction> boxHistory; // ����/���ػط�����
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
        public GameObject reverseWorld; // ��ʱ��
        public GameObject reversePlayer; // ��ʱ��
        public Cinemachine.CinemachineVirtualCamera reverseVirtualCamera; // ��ʱ�������
        public GameObject pastWorld;      // ��ʱ��
        public GameObject pastPlayer;      // ��ʱ��
        public Text countdownText;       // ������ʾ����ʱ��UI�ı�
        public WaterTransformer[] waterTransformers; // ���ڿ���
        private static int levelIndex = 0;
        private int flagCount = 0;
        private float TimeCountdown;
        readonly List<ITimeTrackable> trackedCache = new();
        public GameObject[] boxes;
        public string levelName;
        public string nextLevelName;
        Dictionary<GameState, bool> tracked = new();

        void Awake()
        {
            if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
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
            foreach (var wt in waterTransformers)
            {
                wt.changeWater();
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
            TimeCountdown = postiveLevelInfos[levelIndex].duration;
            // ������ʱ����ң�������ʱ��AI
            reverseWorld.SetActive(false);
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
            foreach (var wt in waterTransformers)
            {
                wt.changeWater();
            }
            // ����һ���¼��������п���ת�������֮ǰ�Ĳ�������״̬
            // ������SendMessage���򻯣������͵���Ŀ�������¼�ϵͳ(UnityEvent/Action)
            BroadcastMessage("OnForwardTimeStart", SendMessageOptions.DontRequireReceiver);
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
            if (Input.GetKeyDown(KeyCode.R)) {
                LoadLevel(levelName);
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
            if (gamePassCheck())
            {
                LoadNextScene();
            }
        }
        public void LoadNextScene()
        {
            int nextSceneIndex = levelIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                levelIndex += 1;
                Debug.LogFormat("load {0} level",levelIndex);
                SceneManager.LoadScene(levelIndex);
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
        /* 1. ���浱ǰ�ؿ� */
        public void SaveCheckpoint()
        {
            checkpoint.levelName = SceneManager.GetActiveScene().name;
            checkpoint.playerPos = FindObjectOfType<Player>().transform.position;

            // ʾ����ÿ���ؿ��ĻطŹ��ܣ�������Ҫ�Լ�д�����ͽ�������
            //checkpoint.boxHistory = TimeManager.Instance.ExportHistory(); // �Լ�д����
        }

        /* 2. �첽���عؿ� */
        public void LoadLevel(string levelName)
        {
            StartCoroutine(LoadAsync(levelName));
        }

        IEnumerator LoadAsync(string levelName)
        {
            // ���� UI
            //FadeCanvas.FadeOut(0.3f);

            AsyncOperation op = SceneManager.LoadSceneAsync(levelName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f) yield return null;
            op.allowSceneActivation = true;

            // ����������ɺ�
            yield return new WaitForEndOfFrame();
            RestoreCheckpoint();
        }

        /* 3. �ָ��浵 */
        void RestoreCheckpoint()
        {
            if (checkpoint.levelName != SceneManager.GetActiveScene().name)
                return;  // ��һ�ν�����

            Player p = FindObjectOfType<Player>();
            if (p) p.transform.position = checkpoint.playerPos;
            // �ָ��浵����
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
                c.a = 0.4f;          // 40 % ͸���ȣ��ɵ�
                sr.color = c;
            }
            reverseVirtualCamera.enabled = false;
        } 
    }
}