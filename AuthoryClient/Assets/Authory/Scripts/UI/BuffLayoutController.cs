using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuffLayoutController : MonoBehaviour
{
    [SerializeField] Image content = null;
    [SerializeField] Image border = null;
    [SerializeField] TMP_Text timer = null;

    Buff buff;

    private void Update()
    {
        timer.text = string.Format(buff.BuffLifetime.ToString("#.00"));

        buff.BuffLifetime -= Time.deltaTime;
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
    }

    public void SetContent(Sprite sprite)
    {
        content.sprite = sprite;
    }

    public void SetBorder(Sprite sprite)
    {
        border.sprite = sprite;
    }
}
