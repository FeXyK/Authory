using Assets.Authory.Scripts.Enum;
using UnityEngine;

/// <summary>
/// Defines a skill.
/// </summary>
[CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/Create Skill", order = 1)]
public class Skill : ScriptableObject
{
    [SerializeField] public KeyCode Hotkey;
    [SerializeField] public ushort SkillId;
    [SerializeField] public string SkillName;
    [SerializeField] public string Description;
    [SerializeField] public Sprite ContentSprite;
    [SerializeField] public EffectType EffectType;
    [SerializeField] public float Cooldown;


    [SerializeField] public float CastTime;
    [SerializeField] public float Range;
    [SerializeField] public int CostType;
    [SerializeField] public int ManaCost;

    [SerializeField] public bool IsTargeted = false;
}