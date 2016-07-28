using UnityEngine;
using System.Collections;
using System;

public partial class Timer
{

    /// <summary>
    /// Create new Repeater type timer
    /// </summary>
    /// <param name="interval"></param>
    /// <param name="callback">will happen each interval</param>
    /// <returns></returns>
    public static Timer Repeater(float interval, Action callback)
    {
        Timer timer = TimerManager.getTimer();
        timer.SetBehavior<RepeaterBehavior>();
        timer.behaviorBase
            .SetFloats(interval, 0, 0, 0)
            .SetCallbacks(callback, null, null, null);
        timer.behavior.Initialize();
        return timer;
    }


    /// <summary>
    /// Creates Repeater that passes parameters to a method on tick.
    /// </summary>
    /// <param name="interval"></param>
    /// <param name="callback"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static Timer RepeaterParam(float interval, Action<object[]> callback, object[] parameters)
    {
        Timer timer = TimerManager.getTimer();
        timer.SetBehavior<RepeaterBehavior>();
        timer.behaviorBase
            .SetFloats(interval, 0, 0, 0)
            .SetCallbacksWithParameters(callback, null, parameters);
        timer.behavior.Initialize();
        if (timer.behaviorBase.Completed)
        {
            Debug.Log(1);
        }
        return timer;
    }

    private class RepeaterBehavior : TimerBehaviorBase, ITimerBehavior
    {

        public void Initialize()
        {

        }

        public void Update(float deltaTime)
        {
            TimePassed += deltaTime;
            TotalTimeActive += deltaTime;

            if (TimePassed > MainInterval)
            {
                if (hasParameters)
                    paramC1(parameters);
                else
                    c1();
                TimePassed = 0f + (TimePassed % MainInterval);
            }
        }
    }

    /// <summary>
    /// Create new Repeater type timer
    /// </summary>
    /// <param name="interval"></param>
    /// <param name="callback">will happen each interval</param>
    /// <returns></returns>
    public static Timer Countdown(float exitTime, Action callback)
    {
        Timer timer = TimerManager.getTimer();
        timer.SetBehavior<CountdownBehavior>();
        timer.behaviorBase
            .SetFloats(exitTime, 0, 0, 0)
            .SetCallbacks(callback, null, null, null);
        timer.behavior.Initialize();
        return timer;
    }

    private class CountdownBehavior : TimerBehaviorBase, ITimerBehavior
    {
        float exitTime;
        public void Initialize()
        {
            exitTime = f1;
        }

        public void Update(float deltaTime)
        {
            TimePassed += deltaTime;
            TotalTimeActive += deltaTime;

            if (TimePassed > exitTime)
            {
                Completed = true;
                c1();
            }
        }
    }
}
