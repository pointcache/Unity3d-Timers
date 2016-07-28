# unityTimer
Timer class with various behaviors

This is timer framework, a way to have generic poolable(reusable) timer with customizeable behavior.

Example usage- 
Simple examples:
``` csharp
public class Test : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            //Start simple repeater 
            Timer.Repeater(5f, () => Debug.Log("Repeater test"));
            //Start countdown and call Draw in 2.5 seconds
            Timer.Countdown(2.5f, Draw);
            //Start countdown and tell it to ignore Time Scale
            Timer.Countdown(10f, () => Debug.Log("Terminating Countdown #2")).SetIgnoreTimeScale(true);
            //Start a repeater, if we try to destroy it manually it wont take effect as countdown self destructs on reaching end.
            Timer timer = Timer.Countdown(1f, null);
            timer.SetCallbacks(() => { Debug.Log("Destroyed timer:" + timer.GetHashCode()); timer.Destroy(); });
            //After we launched sphere repeater we will change its update speed in 8 seconds
            Timer.Countdown(8f, () => sphereRepeater.MainInterval = 0.01f);
            //Create a timer and modify callback afterwards
            Timer t = Timer.Countdown(10f, null);
            t.SetCallbacks(() => Debug.Log("Delayed Callback"));
            //Test performance this will create ~500 timers and will rotate them afterwards in the pool eternally with no GC
            Timer.Repeater(.01f, SpawnTimers);
        }

        void SpawnTimers()
        {
            Timer t = Timer.Countdown(5f, null);
            t.SetCallbacks(() => t.GetHashCode());
        }

        Timer sphereRepeater;
        void Draw()
        {
            //premake some data
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //Start new repeater with parameters that will update our data this is completely stupid in this scenario but it shows the usage.
            Debug.Log("Started RepositioningSphere");
            sphereRepeater = Timer.RepeaterParam(
                .1f
                , x =>  Reposition(x) 
                , new object[1] { go });
        }

        //Casting is costly, this is just example
        void Reposition(object[] args)
        {
            //Receive data and process it
            var go = args[0] as GameObject;

            var pos = go.transform.position;
            pos.y = Mathf.Sin(Time.time) * 2.5f;
            go.transform.position = pos;
        }
    }

```
