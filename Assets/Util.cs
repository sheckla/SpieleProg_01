using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtil 
{
    // float i~[fromMin,fromMax] -> i~[toMin, toMax]
    public static float remapBounds(float i, float fromMin, float fromMax, float toMin, float toMax)
    {
        return (i - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin;
    }

}

public class CooldownTimer
    {
        private float cooldownDuration;
        private float timeRemaining;

        public CooldownTimer(float duration)
        {
            this.cooldownDuration = duration;
            this.timeRemaining = 0;
        }

        public void start()
        {
            this.timeRemaining = this.cooldownDuration;
        }

        public bool onCooldown()
        {
            return this.timeRemaining > 0;
        }

        public void update(float deltaTime)
        {
            this.timeRemaining -= deltaTime;
            if (this.timeRemaining < 0)
            {
                this.timeRemaining = 0;
            }
        }
}

public class FloatRange
{
    private float Value;
    public readonly float Min;
    public readonly float Max;


    public FloatRange(float min, float max)
    {
        this.Min = min;
        this.Max = max;
    }

    public float value() {
        return Value;
    }

    public void setMin() {
        Value = Min;
    }

    public void setMax() {
        Value = Max;
    }

    public void set(float newVal) {
        Value = newVal;
        if (Value < Min) Value = Min;
        if (Value > Max) Value = Max;
    }

    // Standard implicit conversion to float when referencing class
    public static implicit operator float(FloatRange floatRange)
    {
        return floatRange.Value;
    }
}


public class InputToggle
{
    private bool Active;
    private KeyCode Key;
    private bool ButtonHeldDown;

    public InputToggle(KeyCode code) 
    {
        this.Key = code;
        Active = false;
        ButtonHeldDown = false;
    }

    public void update() {
        if (Input.GetKeyDown(Key)) {
            Active = !Active;
        }
    }

    public bool active() {
        return Active;
    }
}