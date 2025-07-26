using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace SaveYourself.Utils
{

    public class common : MonoBehaviour
    {
        static common instance_;
        private void Awake()
        {
            instance_ = this;
        }
        public static common Instance
        {
            get { return instance_; }
        }
        public static string GetTimeCountDownStr(float t)
        {
            string str="";
            if (t > 1e-5)
            {
                str = string.Format("time remain : {0:F} ",t);
            }
            else
            {
                str = "TIME OUT";
            }
            return str;
        }
        public const string initCountdownText = "press Z to start";
        public const int reversableRoleInitialID = 1;
        public const int reversableItemInitialID = 100;
        public const int pushForce = 50;
        public const int RecordGap = 3;
    }
}