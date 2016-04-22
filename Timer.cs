using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
namespace USS.Timers
{
    /// <summary>
    /// Timer is both static pool manager and dynamic DI container for concrete behaviors
    /// </summary>
    public partial class Timer
    {
        #region static part - this part manages timer and its behaviors pooling
        

        /// <summary>
        /// Behavior objects pool 
        /// </summary>
        static Dictionary<Type, List<ITimerBehavior>> behaviors = new Dictionary<Type, List<ITimerBehavior>>();
        static List<Timer> workingTimers = new List<Timer>();
        static List<Timer> freeTimers = new List<Timer>();
        static List<Timer> toRemove = new List<Timer>();

        public static void POLL_TIMER_DATA(TimerHelper.TimerHelperData data)
        {
            data.FreeTimers = freeTimers.Count;
            data.WorkingTimers = workingTimers.Count;
            data.AllTimers = data.FreeTimers + data.WorkingTimers;
        }

        public static void UpdateAllTimers()
        {
            int c = workingTimers.Count;
            //update all timers
            for (int i = 0; i < c; i++)
            {
                workingTimers[i].Update(Time.unscaledDeltaTime, Time.deltaTime);
            }
            c = toRemove.Count;
            for (int i = 0; i < c; i++)
            {
                workingTimers.Remove(toRemove[i]);
            }
            toRemove.Clear();
        }

        //again shortening common actions
        Timer registerBehavior(ITimerBehavior behav)
        {
            behavior = behav;
            behaviorBase = (TimerBehaviorBase)behavior;
            behaviorBase.timer = this;
            return this;
        }

        Timer SetBehavior<T>() where T: TimerBehaviorBase , new()
        {
            Type t = typeof(T);
            ITimerBehavior behav = null;
            if (behaviors.ContainsKey(t))
            {
                List<ITimerBehavior> list = behaviors[t];
                if (list.Count > 0)
                {
                    behav = list[0];
                    list.Remove(behav);
                }
            }
            if (behav == null)
                behav = (ITimerBehavior)new T();

            registerBehavior(behav);
            return this;
        }
        //attempt to get used timer from pool or make new
        static Timer getTimer()
        {
            Timer t = null;
            if (freeTimers.Count == 0)
            {
                t = newTimer();
            }
            else
            {
                t = freeTimers[0];
                freeTimers.Remove(t);
            }
            //Just to make sure our Helper appears when we need it
            if (TimerHelper.instance == null)
            { }
            registerTimer(t);
            return t;
        }
        // avoiding boilerplate
        static Timer newTimer()
        {
            Timer t = new Timer();
            //easy way to get notified of which timer was stopped
            t.OnStopped += ReleaseTimer;
            return t;
        }

        static void registerTimer(Timer timer)
        {
            timer.wasDestroyed = false;
            workingTimers.Add(timer);
        }
        //We do this at the end of timer lifecycle or when we destroy it
        static void ReleaseTimer(Timer timer)
        {
            if (timer.wasDestroyed)
                return;
            freeTimers.Add(timer);
            
            //we use type as key to cache our behaviors
            Type btype = timer.behavior.GetType();
            if (!behaviors.ContainsKey(btype))
                behaviors.Add(btype, new List<ITimerBehavior>());
            behaviors[btype].Add(timer.behavior);

            timer.behavior = null;
            timer.behaviorBase.ResetEntity();
            timer.wasDestroyed = true;
            toRemove.Add(timer);
        }

        public void Destroy()
        {
            ReleaseTimer(this);
        }

        #endregion
        //DI defines what kind of timer this is
        ITimerBehavior behavior;
        //This base is shared between all types of timers
        TimerBehaviorBase behaviorBase;
        //Which time delta we are using for this timer
        TimeDeltaType type = TimeDeltaType.TimeScaleDependent;
        public TimeDeltaType timeDeltaType { get { return type; } }
        public enum TimeDeltaType { TimeScaleDependent, TimeScaleIndependent }
        //Event signaling when some timer finished work and is ready to be released
        event Action<Timer> OnStopped;
        bool wasDestroyed;
        /// <summary>
        /// Not all timer have one fixed interval
        /// </summary>
        public float MainInterval
        {
            get { return behaviorBase.MainInterval; }
            set { behaviorBase.MainInterval = value; }
        }
        /// <summary>
        /// Not all timers return this
        /// </summary>
        public float ElapsedCycles { get { return behaviorBase.ElapsedCycles; } }
        /// <summary>
        /// Do we use Time.deltaTime OR Time.unscaledTimeDelta
        /// unscaled means not affected by Time scale, by default all timers ARE affected by timescale
        /// </summary>
        /// <param name="scaled"></param>
        /// <returns></returns>
        public Timer SetIgnoreTimeScale(bool ignoreTimeScale)
        {
            type = ignoreTimeScale ? TimeDeltaType.TimeScaleIndependent : TimeDeltaType.TimeScaleDependent;
            return this;
        }

        void Update(float REAL_DELTA, float TIMESCALE_DELTA)
        {
            //first check for potential problems
            behaviorBase.CheckErrors();

            if (type == TimeDeltaType.TimeScaleDependent)
                behavior.Update(TIMESCALE_DELTA);
            else
                behavior.Update(REAL_DELTA);

            //Notify pool if we completed work
            if (behaviorBase.Completed)
            {
                OnStopped(this);
                return;
            }
        }

        
        /// <summary>
        /// base class for concrete implementations
        /// </summary>
        private class TimerBehaviorBase
        {
            
            public bool Completed { get; protected set; }
            public Timer timer;
            public float TotalTimeActive { get; protected set; }
            //Implementers can use these variables as they need
            protected float timePassed;
            //Internally used floats
            protected float f1, f2, f3, f4;
            //callbacks
            protected Action<Timer> c1, c2, c3, c4;
            //callbacks with parameters
            protected Action<Timer, object[]> paramC1, paramC2;
            //parameters array
            protected object[] parameters;
            protected bool hasParameters;
            /// <summary>
            /// optional depends on implementation
            /// </summary>
            public int ElapsedCycles { get; protected set; }
            //use this to expose your "main" driving value of the timer
            public float MainInterval { get { return f1; } set { f1 = value; } }
            public TimerBehaviorBase ResetEntity()
            {
                ElapsedCycles = 0;
                timePassed = 0f;
                TotalTimeActive = 0f;
                MainInterval = 0f;
                Completed = false;
                
                parameters = null;
                c1 = c2 = c3 = c4 = null;
                hasParameters = false;
                paramC1 = paramC2 = null;
                f1 = f2 = f3 = f4 = 0f;
                timer = null;

                return this;
            }
            public void CheckErrors()
            {
                if (!hasParameters && c1 == null && c2 == null && c3 == null && c4 == null)
                {
                    Debug.LogError("Timer cant have zero callbacks, such timer is useless. Make sure to add a callback before the timer" + 
                         "starts execution");
                    Debug.Break();
                }
                if (hasParameters && paramC1 == null && paramC2 == null)
                {
                    Debug.LogError("Timer cant have zero parameter callbacks, such timer is useless. Make sure to add a callback before the timer" + 
                         "starts execution");
                    Debug.Break();
                }
            }

            public TimerBehaviorBase SetCallbacks(Action<Timer> one, Action<Timer> two, Action<Timer> three, Action<Timer> four)
            {
                c1 = one;
                c2 = two;
                c3 = three;
                c4 = four;
                return this;
            }

            public TimerBehaviorBase SetCallbacksWithParameters(Action<Timer, object[]> pc1, Action<Timer, object[]> pc2, object[] p)
            {
                paramC1 = pc1;
                paramC2 = pc2;
                parameters = p;
                hasParameters = true;
                return this;
            }

            public TimerBehaviorBase SetFloats(float one, float two, float three, float four)
            {
                //Typically in most scenarios first float will be our main "driving" value for the timer.
                MainInterval = one;

                f1 = one;
                f2 = two;
                f3 = three;
                f4 = four;
                return this;
            }
        }
        /// <summary>
        /// Implementers create behavior
        /// </summary>
        private interface ITimerBehavior
        {
            void Initialize();
            void Update(float deltaTime);
        }
    }
}