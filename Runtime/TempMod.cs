using System;
using UnityEngine;

[Serializable]
public class TempMod
{
    public FloatMod Mod;
    [SerializeField] int timeLeft;

    public TempMod(FloatMod mod, int timeLeft)
    {
        Mod = mod;
        this.timeLeft = timeLeft;
    }

    public bool TickDown(int ticks)
    {
        timeLeft -= ticks;
        return timeLeft <= 0;
    }

    public void IncreaseTime(int byTicks) => timeLeft += byTicks;
}