using Assets.Authory.Scripts.Enum;
using System;
using UnityEngine;

[Serializable]
public class EffectColors
{
    [SerializeField] public EffectType Effect;
    [SerializeField] public Color MainColor;
    [SerializeField] public Color OutlineColor;
}