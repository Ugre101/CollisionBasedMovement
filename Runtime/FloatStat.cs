using System;
using System.Collections.Generic;
using UnityEngine;

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