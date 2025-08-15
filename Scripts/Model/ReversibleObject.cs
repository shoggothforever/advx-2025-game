using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveYourself.Model
{
    public abstract class ReversableItem : MonoBehaviour
    {

        // ���״̬�Ƿ�����ʱ�ս׶α��ı�
        protected bool stateChanged = false;

        // ��ʱ�����������彻��ʱ���õķ���
        public virtual void OnReverseInteraction()
        {
            stateChanged = true;
        }

        // ����ʱ�տ�ʼʱ������ stateChanged ��־����������״̬
        // ��������ᱻ GameManager �� BroadcastMessage ����
        public abstract void OnForwardTimeStart();
    }
}