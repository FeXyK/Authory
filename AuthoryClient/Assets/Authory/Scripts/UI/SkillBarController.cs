using Assets.Authory.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controls skills. 
/// 
/// </summary>
public class SkillBarController : MonoBehaviour
{
    [SerializeField] SkillLayoutController skillLayoutPrefab = null;

    List<SkillLayoutController> Skills;

    UIController uiController;
    ChannelingController channelingController;
    TargetController targetController;

    PlayerEntity player;
    bool AutoAttack;

    void Start()
    {
        player = AuthoryData.Instance.Player;
        channelingController = FindObjectOfType<ChannelingController>();
        targetController = FindObjectOfType<TargetController>();
        uiController = FindObjectOfType<UIController>();

        Skills = new List<SkillLayoutController>();

        foreach (var skill in SkillCollection.Instance.SkillUIObjects.Values)
        {
            SkillLayoutController skillLayout = Instantiate(skillLayoutPrefab);
            skillLayout.transform.SetParent(this.transform);
            skillLayout.SetSkill(Instantiate(skill));
            Skills.Add(skillLayout);
        }
    }

    private void Update()
    {
        if (channelingController.Casting) return;

        if (!uiController.IsActive)
        {
            if (player.Target != null && Input.GetKeyDown(KeyCode.E))
            {
                AuthorySender.SendInteract(player.Target);
            }
            foreach (var skillBarSkill in Skills)
            {
                if (AutoAttack)
                {
                    if (player.Target != null && player.Target.Dead)
                    {
                        AutoAttack = false;
                    }
                    if (skillBarSkill.Hotkey == KeyCode.Alpha0 && skillBarSkill.IsReady())
                    {
                        UseSkill(KeyCode.Alpha0);
                    }
                }
                if (Input.GetKeyDown(skillBarSkill.Hotkey))
                {
                    if (Input.GetKeyDown(KeyCode.Alpha0))
                    {
                        AutoAttack = !AutoAttack;
                    }
                    if (skillBarSkill.IsReady())
                        UseSkill(skillBarSkill.Hotkey);
                }
            }
        }
    }

    private void UseSkill(KeyCode keyCode)
    {
        GetTargetId();
        if (SkillCollection.Instance.SkillUIObjects.TryGetValue(keyCode, out Skill skill))
        {
            if (player.Target != null || skill.IsTargeted)
            {
                if (skill.IsTargeted)
                {
                    AuthorySender.SendSkillRequest((byte)skill.SkillId, GetCameraPointing());
                    channelingController.Set(skill);
                }
                else
                {
                    if (Vector3.Distance(player.transform.position, player.Target.transform.position) > skill.Range)
                    {
                        if (skill.SkillId != 0)
                            uiController.SystemMessage("Out of range");
                        player.GetComponent<PlayerMove>().MoveTowards(player.Target, skill.Range);
                    }
                    else
                    {
                        AuthorySender.SendSkillRequest((byte)skill.SkillId, player.Target.Id);
                        channelingController.Set(skill);
                    }
                }
            }
        }
    }

    private Vector3 GetCameraPointing()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 30, LayerMask.GetMask("Terrain")))
        {
            return hit.point;
        }

        return player.transform.position + player.transform.forward * 100;
    }

    private ushort GetTargetId()
    {
        ushort targetId = ushort.MaxValue;

        if (player.Target != null && player.Target.Alive)
        {
            targetId = player.Target.Id;
        }
        else
        {
            targetController.FindNearestTarget();
            if (player.Target != null)
            {
                targetId = player.Target.Id;
            }
        }

        //Self cast
        if (Input.GetKey(KeyCode.LeftAlt)) targetId = player.Id;

        return targetId;
    }

    public void SetCooldown(Skill castingSkill)
    {
        if (castingSkill == null) return;

        SkillLayoutController current = Skills.SingleOrDefault(x => x.Hotkey == castingSkill.Hotkey);

        if (current != null)
        {
            current.SetCooldown();
        }
    }

    public void ResetSkillCooldown(int skillID)
    {
        SkillLayoutController interrupted = Skills.Find(x => x.SkillId == skillID);
        if (interrupted != null)
        {
            interrupted.ResetCooldown();
        }
    }
}
