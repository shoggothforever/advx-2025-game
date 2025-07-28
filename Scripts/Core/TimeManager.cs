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
        // 只在逆向阶段调用，将要记录的物体添加到记录队列中去
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
        // 只在逆向阶段调用,统一记录所有可以逆向运行物体的状态
        public void Record()
        {
            foreach (var t in registry)
            {
                    Record(t.Value.RecordSnapshot());
            }
        }
        // 内部使用，往history中添加记录
        private void Record(TimeReverse.TimedAction a)
        {
            history.Add(a);
        }
        // 在 Replay 阶段逐帧调用
        public void RewindStep(float dt)
        {

            currentTime = Mathf.Max(currentTime - dt, 0f);

            // 二分查找第一个 time >= currentTime 的索引
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
            // 把游标同步到 hit，后续帧直接顺序扫描
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

        // 正向阶段结束后清理
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
                    // 这里可以让玩家控制 forwardPlayer
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
            Instance.currentTime = GameManager.instance.getTimeLimit() - GameManager.instance.RemainTimeCount; // 从末尾开始倒放
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