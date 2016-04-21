using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
namespace USS.Timers
{
    /// <summary>
    /// Timer is both pool manager and DI container for concrete behaviors
    /// </summary>
    public class Timer
    {
        #region static part - this part manages timer and its behaviors pooling
        /// <summary>
        /// Create new Repeater type timer
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="callback">will happen each interval</param>
        /// <returns></returns>
        public static Timer Repeater(float interval, Action callback)
        {
            Timer timer = getTimer();
            timer.registerBehavior(getBehaviorBase<RepeaterBehavior>());
            timer.behaviorBase
                .ResetEntity()
                .Initialize(interval, 0, 0, 0)
                .SetCallbacks(callback, null, null, null);
            timer.behavior.Initialize();
            return timer;
        }

        /// <summary>
        /// Behavior objects pool 
        /// </summary>
        static Dictionary<Type, List<iTimerBehavior>> behaviors = new Dictionary<Type, List<iTimerBehavior>>();
        static List<Timer> workingTimers = new List<Timer>();
        static List<Timer> freeTimers = new List<Timer>();

        public static void UpdateAllTimers()
        {
            int count = workingTimers.Count;
            //update all timers
            for (int i = 0; i < count; i++)
            {
                workingTimers[i].Update(Time.unscaledDeltaTime);
            }
        }

        //attempt to retreive used behavior from pool, if none - create new
        static iTimerBehavior getBehaviorBase<T>() where T : TimerBehaviorBase , new()
        {
            Type t = typeof(T);
            iTimerBehavior behav = null;
            if (behaviors.ContainsKey(t))
            {
                List<iTimerBehavior> list = behaviors[t];
                if (list.Count > 0)
                {
                    behav = list[0];
                    list.Remove(behav);
                }
            }
            if(behav == null)
                behav =(iTimerBehavior) new T();
            return behav;
        }
        //attempt to get used timer from pool or make new
        static Timer getTimer()
        {
            Timer t = null;
            if (freeTimers.Count == 0)
            {
                t = new Timer();
                registerTimer(t);
            }
            else
            {
                t = freeTimers[0];
                freeTimers.Remove(t);
            }
            if (TimerHelper.instance == null)
            { }
            return t;
        }
        // avoiding boilerplate
        static void registerTimer(Timer timer)
        {
            //easy way to get notified of which timer was stopped
            timer.OnStopped += ReleaseTimer;
            workingTimers.Add(timer);
        }
        //We do this at the end of timer lifecycle or when we destroy it
        static void ReleaseTimer(Timer timer)
        {
            freeTimers.Add(timer);
            workingTimers.Remove(timer);
            //we use type as key to cache our behaviors
            Type btype = timer.behavior.GetType();
            if (!behaviors.ContainsKey(btype))
                behaviors.Add(btype, new List<iTimerBehavior>());
            behaviors[btype].Add(timer.behavior);

            timer.behavior = null;
        }

        public void Destroy()
        {
            ReleaseTimer(this);
        }

        #endregion
        //DI defines what kind of timer this is
        iTimerBehavior behavior;
        //This base is shared between all types of timers
        TimerBehaviorBase behaviorBase;
        
        event Action<Timer> OnStopped;

        void Update(float deltaTime)
        {
            if (behaviorBase.Completed)
            {
                OnStopped(this);
                return;
            }
            behavior.Update(deltaTime);
        }
        //again shortening common actions
        iTimerBehavior registerBehavior(iTimerBehavior behav)
        {
            behavior = behav;
            behaviorBase = (TimerBehaviorBase)behavior;
            return behavior;
        }
        /// <summary>
        /// base class for concrete implementations
        /// </summary>
        private class TimerBehaviorBase
        {
            
            public bool Completed { get; private set; }

            //Implementers can use these variables as they need
            float timePassed;
            float exitTime;
            float f1, f2, f3, f4;
            Action c1, c2, c3, c4;

            public TimerBehaviorBase SetCallbacks(Action one, Action two, Action three, Action four)
            {
                c1 = one;
                c2 = two;
                c3 = three;
                c4 = four;
                return this;
            }

            public TimerBehaviorBase Initialize(float one, float two, float three, float four)
            {
                f1 = one;
                f2 = two;
                f3 = three;
                f4 = four;
                return this;
            }

            public TimerBehaviorBase ResetEntity()
            {
                timePassed = 0f;
                exitTime = 0f;
                Completed = false;
                return this;
            }
        }
        /// <summary>
        /// Implementers create behavior
        /// </summary>
        private interface iTimerBehavior
        {
            void Initialize();
            void Update(float deltaTime);
        }

        private class RepeaterBehavior : TimerBehaviorBase, iTimerBehavior
        {
            public void Initialize()
            {
                Debug.Log("Initialized");
            }

            public void Update(float deltaTime)
            {
                Debug.Log("Doing work");
            }
        }
    }
}