using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillLayoutController : MonoBehaviour
{
    [SerializeField] public KeyCode Hotkey;
    [SerializeField] public int SkillId;
    [SerializeField] Skill skill = null;

    [SerializeField] Image content = null;
    [SerializeField] Image cooldownContent = null;
    [SerializeField] TMP_Text keyText = null;
    [SerializeField] TMP_Text cooldownText = null;
    [SerializeField] TMP_Text manaText = null;

    [SerializeField] float cooldown;



    void Update()
    {
        cooldown -= Time.deltaTime;
        DisplayCooldown();
    }

    public void ResetCooldown()
    {
        cooldown = 0;
        DisplayCooldown();
    }

    public void SetCooldown()
    {
        cooldown = skill.Cooldown;
    }

    public void SetSkill(Skill skill)
    {
        this.skill = skill;

        SkillId = skill.SkillId;
        content.sprite = skill.ContentSprite;
        cooldownContent.sprite = skill.ContentSprite;

        keyText.text = skill.Hotkey.ToString().Replace("Alpha", "");
        manaText.text = skill.ManaCost.ToString();

        Hotkey = skill.Hotkey;
        cooldown = 0;
    }

    public void DisplayCooldown()
    {
        if (skill != null)
        {
            cooldownText.gameObject.SetActive(cooldown > 0);
            if (cooldown > 0)
                cooldownText.text = (int)cooldown + "<color=#FFFFFF>s</color>";
            cooldownContent.fillAmount = (float)cooldown / (float)skill.Cooldown;
        }
    }

    public bool IsReady()
    {
        return cooldown < 0;
    }
    }
