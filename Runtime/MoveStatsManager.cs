using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveStatsManager : MonoBehaviour
{
    [field: SerializeField] public FloatStat MoveSpeed { get; private set; } = new(10);
    [field: SerializeField] public FloatStat SwimmingSpeed { get; private set; } = new(8);
}

[Serializable]
public class FloatStat
{
    [SerializeField] float baseValue;
    [SerializeField] List<FloatMod> mods = new();
    [SerializeField] List<TempMod> tempMods = new();
    bool dirty = true;

    float lastValue;

    public FloatStat(float startValue) => baseValue = startValue;

    public float Value
    {
        get
        {
            if (!dirty)
                return lastValue;
            lastValue = baseValue;
            foreach (var floatMod in mods)
                lastValue += floatMod.Value;
            foreach (var tempMod in tempMods)
                lastValue += tempMod.Mod.Value;
            return lastValue;
        }
    }

    public void AddMod(FloatMod mod)
    {
        mods.Add(mod);
        dirty = true;
    }

    public void AddTempMod(TempMod mod)
    {
        tempMods.Add(mod);
    }

    public void Tick(int ticks)
    {
        for (var index = tempMods.Count - 1; index >= 0; index--)
        {
            var tempMod = tempMods[index];
            if (tempMod.TickDown(ticks))
            {
                tempMods.RemoveAt(index);
                dirty = true;
            }
        }
    }

    public void RemoveMod(FloatMod mod) => dirty = mods.Remove(mod);
}

[Serializable]
public struct FloatMod
{
    public FloatMod(float value) => Value = value;
    [field: SerializeField] public float Value { get; private set; }
}

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