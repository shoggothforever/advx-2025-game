using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveYourself.Model
{
    public abstract class ReversableItem : MonoBehaviour
    {

        // 标记状态是否在逆时空阶段被改变
        protected bool stateChanged = false;

        // 逆时空玩家与此物体交互时调用的方法
        public virtual void OnReverseInteraction()
        {
            stateChanged = true;
        }

        // 在正时空开始时，根据 stateChanged 标志来更新自身状态
        // 这个方法会被 GameManager 的 BroadcastMessage 调用
        public abstract void OnForwardTimeStart();
    }
}