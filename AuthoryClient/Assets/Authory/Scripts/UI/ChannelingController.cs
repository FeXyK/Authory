using Assets.Authory.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChannelingController : MonoBehaviour
{
    [SerializeField] private Slider ChannelingBar = null;
    [SerializeField] private TMP_Text ChannelingSkill = null;
    [SerializeField] private SkillBarController SkillBarController = null;
    private Skill CastingSkill;
    private PlayerMove PlayerMove;

    public bool Casting { get; set; }

    private void Start()
    {
        PlayerMove = GameObject.FindObjectOfType<PlayerMove>();
        SkillBarController = FindObjectOfType<SkillBarController>();
    }
    void Update()
    {
        ChannelingBar.value += Time.deltaTime;
        if (Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Vertical"))
        {
            this.gameObject.SetActive(false);
            Casting = false;
        }
        if (ChannelingBar.value >= ChannelingBar.maxValue)
        {
            this.gameObject.SetActive(false);
            PlayerMove.EnableMovement(false);
            Casting = false;
            SkillBarController.SetCooldown(CastingSkill);
        }
    }

    public void Set(Skill skill)
    {
        CastingSkill = skill;

        this.gameObject.SetActive(true);
        ChannelingBar.maxValue = skill.CastTime;
        ChannelingBar.value = 0;

        ChannelingSkill.text = skill.SkillName;
        ChannelingSkill.color = SkillCollection.Instance.SkillEffectColors[skill.EffectType].MainColor;
        ChannelingSkill.outlineColor = SkillCollection.Instance.SkillEffectColors[skill.EffectType].OutlineColor;
        Casting = true;
        if (skill.Cooldown > 0)
        {
            PlayerMove.EnableMovement();
        }
    }
}
