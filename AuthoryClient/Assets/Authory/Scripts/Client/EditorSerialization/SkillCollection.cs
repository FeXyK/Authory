using Assets.Authory.Scripts.Enum;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains all the skills. If a skill needs to be spawned it should be called from here.
/// </summary>
public class SkillCollection : MonoBehaviour
{

    [SerializeField] List<SkillObjectContainer> skillObjects;
    [SerializeField] List<EffectColors> skillEffectColors = null;
    [SerializeField] List<Buff> buffScriptableObjects = null;
    [SerializeField] List<Skill> skillScriptableObjects = null;

    public Dictionary<ushort, GameObject> SkillObjects { get; private set; }
    public Dictionary<EffectType, EffectColors> SkillEffectColors { get; private set; }

    public Dictionary<ushort, Buff> BuffObjects { get; private set; }
    public Dictionary<KeyCode, Skill> SkillUIObjects { get; private set; }

    public static SkillCollection Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        SkillObjects = new Dictionary<ushort, GameObject>();
        SkillEffectColors = new Dictionary<EffectType, EffectColors>();
        BuffObjects = new Dictionary<ushort, Buff>();
        SkillUIObjects = new Dictionary<KeyCode, Skill>();

        foreach (var skillObject in skillObjects)
        {
            SkillObjects.Add(skillObject.ID, skillObject.SkillObject);
        }

        foreach (var skillColor in skillEffectColors)
        {
            SkillEffectColors.Add(skillColor.Effect, skillColor);
        }

        foreach (var buff in buffScriptableObjects)
        {
            if (buff != null)
            {
                BuffObjects.Add(buff.BuffId, Instantiate(buff));
            }
        }

        foreach (var skill in skillScriptableObjects)
        {
            if (skill != null)
            {
                if (skill.Hotkey != KeyCode.None)
                    SkillUIObjects.Add(skill.Hotkey, Instantiate(skill));
            }
        }
    }
}
