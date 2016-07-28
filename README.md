# unityTimer
Timer class with various behaviors

This is timer framework, a way to have generic poolable(reusable) timer with customizeable behavior.

Example usage- 
``` csharp
var light_switch_timer = Timer.Cooldown(1f, () => light.SetActive(true););
```
