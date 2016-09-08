![License MIT](https://img.shields.io/badge/license-MIT-green.svg)

# Unity3d-Timers
Timer class with various behaviors

About: 
  Have a system allowing to easily create and extend complex timer behaviors.
  
Usage:

``` csharp
  //Start simple repeater 
  Timer.Repeater(5f, () => Debug.Log("Repeater test"));
  //Start countdown and call Draw in 2.5 seconds
  Timer.Countdown(2.5f, Draw);
  ```
  
  Concept:
  A manager class pools and constructs new timers, timer store up to 4 handlers, and arbitrarely raise events in 
  concrete implementation.
  
  ``` csharp
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
    ```
Countdown inherits TimerBehaviorBase and implements ITimerBehavior.
On top of that only concrete behavior in implemented that uses data and callbacks provided in constructor:

``` csharp
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
```

as you can see you can set 4 floats, and 4 callbacks and use them in your implementation of behavior as you like.
Examples 
![](http://i.imgur.com/3KabwIi.png)

For concrete real example look at Ability class,
it implements typical game ability, with cooldown.
