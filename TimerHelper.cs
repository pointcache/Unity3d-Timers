using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace USS.Timers
{
    /// <summary>
    /// Acts a link between Timer class and Unity Update method.
    /// </summary>
    public class TimerHelper : MonoBehaviour
    {
        private static TimerHelper _instance;
        public static TimerHelper instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TimerHelper>();
                    if (_instance == null)
                    {
                        _instance = CreateNew();
                    }

                }
                return _instance;
            }
        }

        static TimerHelper CreateNew()
        {
            GameObject go = new GameObject("TimerHelper");
            return go.AddComponent<TimerHelper>();
        }

        void Update()
        {
            Timer.UpdateAllTimers();
        }
    }
}