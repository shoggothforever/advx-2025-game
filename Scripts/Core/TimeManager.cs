using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveYourself.Utils;
namespace SaveYourself.Core
{
    public class TimeManager : MonoBehaviour
    {
        public GameObject pastPlayer;
        public GameObject reversePlayer;
        public enum Phase { PreReverse,Reverse,PreReplay, Replay, Forward }
        public Phase phase = Phase.PreReverse;
        public float currentTime=0;             // 0..reverseDuration
        private float currentTimeRecord = 0;
        public  List<TimeReverse.TimedAction> history = new();
        private List<TimeReverse.TimedAction> historyRecord = new();
        readonly Dictionary<int, TimeReverse.ITimeTrackable> registry = new();
        static private TimeManager instance_;
        public static TimeManager Instance
        {
            get { return instance_; }
        }
        private int rewindIndex = 0;
        private void Awake()
        {
            instance_ = this;
        }
        // ֻ������׶ε��ã���Ҫ��¼��������ӵ���¼������ȥ
        public void Register(TimeReverse.ITimeTrackable t)
        {
            if (!registry.ContainsKey(t.Id))
            {
                //Debug.LogFormat("set reversible object ID {0}", t.Id);
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
        public void UnRegisterByID(int id)
        {
            if (registry.ContainsKey(id))
            {
                registry.Remove(id);
            }
        }
        public void addHistory(TimeReverse.ITimeTrackable t,TimeReverse.TimedAction ta)
        {
            Register(t);
            history.Add(ta);
        }
        // ֻ������׶ε���,ͳһ��¼���п����������������״̬
        public void Record()
        {
            foreach (var t in registry)
            {
                if (t.Value.GetActionType()!=TimeReverse.ActionType.Manual)
                {
                    Record(t.Value.RecordSnapshot());
                }
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
            if (GameManager.Instance.timeStopped) return;
            currentTime = Mathf.Max(currentTime - dt, 0f);

            // ���ֲ��ҵ�һ�� time >= currentTime ������
            int left = 0;
            int right = history.Count - 1;
            int hit = -1;
            while (left <= right)
            {
                int mid = (left + right) >> 1;
                if (history[mid].time >= currentTime)
                {
                    hit = mid;
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }
            // ���α�ͬ���� hit������ֱ֡��˳��ɨ��
            if (hit >= 0)
            {
                int cnt = 0;
                rewindIndex = hit;
                Stack<TimeReverse.TimedAction> st=new();
                for (; rewindIndex < history.Count && history[rewindIndex].time <= currentTime + dt; ++rewindIndex)
                {
                    var a = history[rewindIndex];
                    if (registry.TryGetValue(a.objId, out _))
                    {
                        st.Push(a);
                        ++cnt;
                    }
                }
                while(st.Count > 0)
                {
                    registry[st.Peek().objId].ApplySnapshot(st.Pop());
                }
                //Debug.LogFormat("apply {0} snapshot",cnt);
            }
        }

        // ����׶ν���������
        public void Clear()
        {
            currentTime = 0;
            phase = Phase.PreReverse;
            history.Clear();
            registry.Clear();
        }
        // Update is called once per frame
        void Update()
        {
            if (GameManager.Instance.timeStopped) return;
            switch (phase)
            {
                case Phase.Reverse:
                    currentTime += Time.deltaTime;
                    if (currentTime >= GameManager.Instance.getTimeLimit() - GameManager.Instance.RemainTimeCount)
                    {
                        StartPreReplay();
                    }
                    else
                    {
                        if (Time.frameCount % Const.RecordGap == 0)
                        {
                            Record();
                        }
                    }
                    break;
                case Phase.PreReplay:
                    if (GameManager.Instance.currentState == GameState.ForwardTime)
                    {
                        SaveManager.Instance.SaveSnapshot(GameManager.Instance.levelName, history);
                        StartReplay();
                        Debug.Log("start replay");
                        Debug.LogFormat("replayList length is {0}",history.Count);
                    }
                    break;
                case Phase.Replay:
                    RewindStep(Time.deltaTime);
                    if (currentTime <= 0)
                        StartForward();
                    break;
                case Phase.Forward:
                    // �����������ҿ��� forwardPlayer
                    break;
            }
        }
        void StartPreReplay()
        {
            historyRecord = new List<TimeReverse.TimedAction>(history);
            Debug.LogFormat("save history, length: {0}", history.Count);
            currentTimeRecord = currentTime;
            Debug.LogFormat("record currentTime:{0}", currentTime);
            Instance.phase =Phase.PreReplay;
        }
        void StartReplay()
        {
            Instance.currentTime = GameManager.Instance.getTimeLimit() - GameManager.Instance.RemainTimeCount; // ��ĩβ��ʼ����
            Instance.phase = Phase.Replay;
        }

        void StartForward()
        {
            Instance.phase = Phase.Forward;
            currentTime = 0;
        }
        public void RestartFromPreForward()
        {
            currentTime = currentTimeRecord;
            history = new List<TimeReverse.TimedAction>(historyRecord);
            phase = Phase.PreReplay;
        }
    }
}