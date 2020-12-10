using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuffLayoutController : MonoBehaviour
{
    [SerializeField] Image content = null;
    [SerializeField] Image border = null;
    [SerializeField] TMP_Text tooltip = null;
    [SerializeField] TMP_Text timer = null;

    string buffName;
    string description;
    Buff buff;

    private void Update()
    {
        timer.text = string.Format(buff.BuffLifetime.ToString("#.00"));
        //Debug.LogError($"BLT: {buff.BuffLifetime}");

        if (buff.BuffLifetime < 0)
            Destroy(this.gameObject);
    }

    public void SetContent(Buff buff)
    {
        this.buff = buff;
        SetContent(this.buff.ContentSprite, this.buff.BuffName, this.buff.Description);
    }

    public void SetContent(Sprite sprite, string buffName, string description)
    {
        content.sprite = sprite;
        SetTooltipContent(buffName, description);
    }

    public void SetContent(Sprite sprite)
    {
        content.sprite = sprite;
        SetTooltipContent("Unknown", "");
    }

    public void SetBorder(Sprite sprite)
    {
        border.sprite = sprite;
    }

    public void SetTooltipContent(string buffName, string description)
    {
        this.buffName = buffName;
        this.description = description;

        this.tooltip.text = string.Format($"{this.buffName}\n{this.description}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //tooltip.gameObject.SetActive(true);
        //tooltip.transform.position = Input.mousePosition;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //tooltip.gameObject.SetActive(false);
    }
}
