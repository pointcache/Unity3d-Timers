using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Object that is used to control if action can be fired or not based on internal timer.
/// ON cooldown - means you are cooling down the ability and cant use it yet
/// OFF cooldown - means you finised cooling down and are ready to use it
/// 
/// 
/// USAGE
/// 1. Create Cooldown object with a constructor and provide starting parameters
/// 2. Use available events to specify cooldown functionality
/// 3. Call "Use" method for "casting" something
/// 4. When not needed anymore, use Destroy
/// 5. parameters are read only, if you need to modify cooldown on runtime, create new one
/// </summary>
[Serializable]
public class Ability  {

    Timer cooldown_timer;
    Timer cast_timer;

    /// <summary>
    /// What happens right the moment we start action, ignoring the cast time/delay
    /// </summary>
    public event Action onStart;
    /// <summary>
    /// What will happen if cooldown allows action
    /// </summary>
    public event Action onCast;
    /// <summary>
    /// What happens after cooldown ends
    /// </summary>
    public event Action onOffCooldown;

    /// <summary>
    /// When cancel was explicitly called
    /// </summary>
    public event Action onCancel;

    /// <summary>
    /// For errors and other purposes
    /// </summary> 
    public string name { get; private set; }

    /// <summary>
    /// Actual cooldown time 
    /// </summary>
    public float cooldown_time { get; private set; }
    public float cooldown_left { get; private set; }

    public CooldownType cooldowntype;

    /// <summary>
    /// When does the cooldown start?
    /// </summary>
    public enum CooldownType
    {
        /// <summary>
        /// Cooldown begins from the moment we Use this cooldown, not waiting for delay
        /// </summary>
        fromStart,
        /// <summary>
        /// Cooldown begins after the cast timer finished, right at cast point
        /// </summary>
        fromCastPoint
    }


    /// <summary>
    /// The time from start till actual cast, think of it as delay in which you play cast animation and you can cancel 
    /// </summary>
    public float castTime{ get; private set; }

    /// <summary>
    /// Initialization of timer should happen AFTER first use
    /// </summary>
    bool firstUse;

    /// <summary>
    /// When we are on cooldown, we cant use action, when we are off the cooldown, means action is ready
    /// </summary>
    public bool OnCooldown { get; private set; }

    /// <summary>
    /// Are we currently casting this ability?
    /// </summary>
    public bool isCasting { get; private set; }
    /// <summary>
    /// Is the ability completely suspended? (muted/stunned/frozen cooldown/disabled)
    /// </summary>
    public bool LockCasting { get; set; }

    bool has_cast_time;

    public Ability(string _name, float _cooldown_time, float _cast_time, Action _onActivate, CooldownType _type)
    {
        if(_onActivate == null)
        {
            Debug.LogError("Tried to create ability without callback" + name);
            return;
        }
        if (_cooldown_time <= 0f)
        {
            Debug.LogError("Resulting cooldown for " + _name + " is less or equal to zero, which is incorrect.");
            Debug.Break();
        }
        //we treat ability as immutable object, so we only accept the callback in constructor
        onCast = _onActivate;
        //use for error reports and other things
        name = _name;
        //do we have a cast time?
        has_cast_time = _cast_time > 0f ? true : false;
        //if yes, create cast timer
        if (has_cast_time &&  cast_timer == null)
        {
            cast_timer = Timer.Countdown(_cast_time, actual_cast).Pause();
        }

        cooldown_time = _cooldown_time;
        //switch (_type)
        //{
        //    case CooldownType.fromStart:
        //        cooldown_time = _cooldown_time + _cast_time;
        //        break;
        //    case CooldownType.fromCastPoint:
        //        cooldown_time = _cooldown_time;
        //        break;
        //    default:
        //        break;
        //}

        castTime = _cast_time;
        cooldowntype = _type;
        firstUse = true;
    }

    /// <summary>
    /// This will activate attached action
    /// </summary>
	public bool Use()
    {
        if (!OnCooldown && !LockCasting)
        {
            activate();
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// this activates attached action
    /// </summary>
    void activate()
    {
        if (onStart != null)
            onStart();
        //on first use we Activate, and create a cooldown timer
        if (firstUse)
        {
            if (has_cast_time && cooldowntype == CooldownType.fromStart)
                cooldown_timer = Timer.Countdown(cooldown_time + castTime, put_offcooldown);
            else
                cooldown_timer = Timer.Countdown(cooldown_time, put_offcooldown);

            cooldown_timer.OnUpdate += () => cooldown_left = cooldown_timer.TimeLeft;
            //Dont let the pool reclaim this timer
            cooldown_timer.DontDisposeOnComplete = true;
            firstUse = false;
        }

        if (has_cast_time)
        {
            isCasting = true;
            if (cooldowntype == CooldownType.fromStart)
            {
                //we already are on cooldown, from the start of animation
                PutOnCooldown();
            }
            //when cast timer countdown finished it will cast
            cast_timer.Unpause();
            return;
        }

        else
        {
            actual_cast();
            PutOnCooldown();
        }
    }

    /// <summary>
    /// If you are currently casting will go back to start of the ability
    /// </summary>
    public void CancelCast()
    {
        if (isCasting)
        {
            if (onCancel != null)
                onCancel();
            //reset the cast timer 
            if (has_cast_time)
                cast_timer.Reset().Pause();
        }
    }

    void actual_cast()
    {
        onCast();
        isCasting = false;
        //reset the cast timer 
        if (has_cast_time)
            cast_timer.Reset().Pause();
    }
    /// <summary>
    /// Skips cast and goes straight to cooldown, use when you need to "break" the spell with another thing (like stun)
    /// </summary>
    public void PutOnCooldown()
    {
        if(firstUse || OnCooldown)
        {
            Debug.Log(name + "Already on cooldown");
            return;
        }
        OnCooldown = true;
        cooldown_timer.Unpause();
        
    }

    void reset()
    {
        if (has_cast_time)
            cast_timer.Reset().Pause();
        OnCooldown = isCasting = false;
        cooldown_timer.Reset().Pause();
    }

    /// <summary>
    /// "deactivates" or rather, resets the cooldown
    /// </summary>
    void put_offcooldown()
    {
        //Event raised when cooldown stops
        if (onOffCooldown != null)
            onOffCooldown();
        OnCooldown = false;
        cooldown_timer.Reset().Pause();
    }


    void Destroy()
    {
        cast_timer.Destroy();
        cooldown_timer.Destroy();
    }

}
