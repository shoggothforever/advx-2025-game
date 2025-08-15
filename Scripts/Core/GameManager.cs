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
        public GameObject reverseWorld; // ��ʱ��
        public GameObject reversePlayer; // ��ʱ�ս�ɫ
        public Cinemachine.CinemachineVirtualCamera reverseVirtualCamera; // ��ʱ�������
        public GameObject pastWorld;      // ��ʱ��
        public GameObject pastPlayer;      // ��ʱ�ս�ɫ
        public Vector3 originPastPlayPosition;
        public Text countdownText;       // ������ʾ����ʱ��UI�ı�
        public List<WaterTransformer> waterTransformers; // ���ڿ���
        private float TimeCountdown=10f; // ����ʱ
        public float RemainTimeCount=0; // ��Լ��ʱ��
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
            timeStopped = false;
            TimeManager.Instance.phase=TimeManager.Phase.Reverse;
            // ������ʱ����ң�������ʱ��AI

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
            Debug.Log("��ʱ�ս׶ο�ʼ������ " + TimeCountdown + " ��ʱ�䡣");
            countdownText.color = Color.red;
            countdownText.text = common.GetTimeCountDownStr(TimeCountdown);
        }

        // Ԥ����ʱ�ս׶�
        public void StartPreForwardTimePhase(bool first)
        {
            currentState = GameState.PreForwardTime;
            // ��ȡ����ʱ�ս�Լ������ʱ��
            if(first)RemainTimeCount = Mathf.Max(TimeCountdown,0);
            // �ָ�TimeCounddown
            TimeCountdown = getTimeLimit();
            // ������ʱ����ң�������ʱ��AI
            reversePlayer.GetComponent<Player>().controlEnabled = false;
            // Ԥ��ʱ����ʾ��ʱ�յ����
            reversePlayer.SetActive(false);
            pastWorld.SetActive(true);
            pastPlayer.SetActive(true);
            pastPlayer.GetComponent<Player>().controlEnabled = false;
            countdownText.color = Color.blue;
            countdownText.text = common.GetTimeCountDownStr(TimeCountdown) +"\n"+"���� Z �� ����׼��";
        }    
        // ��ʼ��ʱ�ս׶�
        public void StartForwardTimePhase()
        {
            currentState = GameState.ForwardTime;
            pastPlayer.GetComponent<Player>().controlEnabled = true;
            EnableReverseSprite();
            //������������
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
            // ˮ�������
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
                countdownText.text = "����Q���ָ�ʱ������";
                return;
            }
            if (Input.GetKeyDown(KeyCode.P)) {
                LoaderManager.Instance.LoadScene(levelName);
            }
            // Ԥ��ʱ�䲻����ʱ
            if (!timeStopped && TimeCountdown > 0 && (currentState != GameState.PreForwardTime || currentState != GameState.PreForwardTime))
            {
                TimeCountdown -= Time.deltaTime;
                //Debug.Log("���� " + TimeCountdown + " ��ʱ�䡣");
                if (currentState == GameState.PreForwardTime || currentState == GameState.PreForwardTime)
                {
                    countdownText.text = "���� Z �� ����׼��";
                }
                else countdownText.text = common.GetTimeCountDownStr(TimeCountdown);
            }// �������絹��ʱ������ʼ��������
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
                countdownText.text = "����Z�� ��ʼ��Ϸ";
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