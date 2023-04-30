using System;
using UnityEngine;

[Serializable]
public struct FloatMod
{
    public FloatMod(float value) => Value = value;
    [field: SerializeField] public float Value { get; private set; }
}