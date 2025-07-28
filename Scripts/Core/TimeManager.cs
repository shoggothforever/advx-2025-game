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
        readonly List<TimeReverse.TimedAction> history = new();
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
        // ֻ������׶ε���,ͳһ��¼���п����������������״̬
        public void Record()
        {
            foreach (var t in registry)
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
                    if (registry.TryGetValue(a.objId, out var obj))
                    {
                        st.Push(a);
                        //obj.ApplySnapshot(a);k
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
            Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("GhostPlayer"),
            LayerMask.NameToLayer("Box"),
            false);
            Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("GhostPlayer"),
            LayerMask.NameToLayer("ShrinkBox"),
            false);
        }
        // Update is called once per frame
        void Update()
        {
            //if (!LoaderManager.Instance.isReady) return;
            switch (Instance.phase)
            {
                case Phase.Reverse:
                    Instance.currentTime += Time.deltaTime;
                    if (Instance.currentTime >= GameManager.instance.getTimeLimit() - GameManager.instance.RemainTimeCount)
                    {
                        StartPreReplay();
                    }
                    else
                    {
                        if (Time.frameCount % common.RecordGap == 0)
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
            Instance.currentTime = GameManager.instance.getTimeLimit() - GameManager.instance.RemainTimeCount; // ��ĩβ��ʼ����
            Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("GhostPlayer"),
            LayerMask.NameToLayer("Box"),
            true);
            Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("GhostPlayer"),
            LayerMask.NameToLayer("ShrinkBox"),
            true);            
            Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("GhostPlayer"),
            LayerMask.NameToLayer("Player"),
            true);
        }

        void StartForward()
        {
            Instance.phase = Phase.Forward;
        }
    }
}