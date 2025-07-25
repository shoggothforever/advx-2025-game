using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveYourself.Core
{
    public class TimeManager : MonoBehaviour
    {
        public GameObject pastPlayer;
        public GameObject reversePlayer;
        public enum Phase { PreReverse,Reverse,PreReplay, Replay, Forward }
        public Phase phase = Phase.PreReverse;

        public float reverseDuration = 10f;   // ����׶���ʱ��
        public float currentTime=0;             // 0..reverseDuration

        readonly List<TimeReverse.TimedAction> history = new();
        readonly Dictionary<int, TimeReverse.ITimeTrackable> registry = new();
        static public float global_time = 0f;
        static private TimeManager instance_;
        public static TimeManager Instance
        {
            get { return instance_; }
        }
        private void Awake()
        {
            instance_ = this;
        }
        // ֻ������׶ε��ã���Ҫ��¼��������ӵ���¼������ȥ
        public void Register(TimeReverse.ITimeTrackable t)
        {
            if (!registry.ContainsKey(t.Id))
            {
                Debug.LogFormat("set reversible object ID {0}", t.Id);
                registry.Add(t.Id, t);
            }
        }
        public void UnRegister(TimeReverse.ITimeTrackable t)
        {
            if (registry.ContainsKey(t.Id))
            {
                registry.Remove(t.Id);
            }
        }
        // ֻ������׶ε���,ͳһ��¼���п����������������״̬
        public void Record()
        {
            foreach(var t in registry)
            {
                Record(t.Value.RecordSnapshot());
            }
        }
        // �ڲ�ʹ�ã���history����Ӽ�¼
        private void Record(TimeReverse.TimedAction a)
        {
            history.Add(a);
        }
        // �� Replay �׶���֡����
        public void RewindStep(float dt)
        {
            currentTime = Mathf.Max(currentTime - dt, 0);
            foreach (var a in history.FindAll(x => Mathf.Abs(x.time - currentTime) < 0.02f))
            {
                registry[a.objId].ApplySnapshot(a);
                Debug.LogFormat("replay obj ID:{0}",a.objId);
            }
        }

        // ����׶ν���������
        public void Clear()
        {
            history.Clear();
            registry.Clear();
        }
        // Update is called once per frame
        void Update()
        {
            switch (Instance.phase)
            {
                case Phase.Reverse:
                    Instance.currentTime += Time.deltaTime;
                    if (Instance.currentTime >= GameManager.instance.getTimeLimit())
                    {
                        StartPreReplay();
                    }
                    else
                    {
                        if (Time.frameCount % 3 == 0)
                        {
                            Instance.Record();
                        }
                    }
                    break;
                case Phase.PreReplay:
                    if (GameManager.instance.currentState == GameState.ForwardTime)
                    {
                        StartReplay();
                        Debug.Log("start replay");
                        Debug.LogFormat("replayList length is {0}",history.Count);
                    }
                    break;
                case Phase.Replay:
                    Instance.RewindStep(Time.deltaTime);
                    if (Instance.currentTime <= 0)
                        StartForward();
                    break;
                case Phase.Forward:
                    // �����������ҿ��� forwardPlayer
                    break;
            }
        }
        void StartPreReplay()
        {
           Instance.phase =Phase.PreReplay;

        }
        void StartReplay()
        {
            Instance.phase = Phase.Replay;
            //reversePlayer.SetActive(false);
            Instance.currentTime = Instance.reverseDuration; // ��ĩβ��ʼ����
            //Instance.RewindStep(Time.deltaTime);
            //if (Instance.currentTime <= 0)
            //    StartForward();
        }

        void StartForward()
        {
            Instance.phase = Phase.Forward;
            //pastPlayer.SetActive(true);
        }
    }
}