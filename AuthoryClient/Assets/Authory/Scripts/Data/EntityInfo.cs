using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

/// <summary>
/// Handles the canvas over an entity that contains name and health. 
/// </summary>
public class EntityInfo : MonoBehaviour
{
    [SerializeField] Camera mainCamera = null;
    [SerializeField] TMP_Text Name = null;
    [SerializeField] Slider HealthBar = null;
    [SerializeField] Slider ManaBar = null;
    [SerializeField] Image HealthBarFillerImage = null;

    [SerializeField] Color NormalColor = Color.white;
    [SerializeField] Color SelectionColor = Color.white;
    [SerializeField] Color HighlightColor = Color.white;

    [SerializeField] float NormalHeight = 1;
    [SerializeField] float SelectionHeight = 1;
    [SerializeField] float HighlightHeight = 1;

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }


    void Update()
    {
        this.transform.rotation = mainCamera.transform.rotation;
        Name.color = NormalColor;

        this.gameObject.SetActive(true);
    }

    public void SetInfo(string name, bool isPlayer = false)
    {
        Name.text = name;
        if (isPlayer)
            HealthBarFillerImage.color = Color.green;
    }

    public void UpdateHealthBar(Entity entity)
    {
        HealthBar.maxValue = entity.Health.MaxValue;
        HealthBar.value = entity.Health.Value;

        if (ManaBar != null)
        {
            ManaBar.maxValue = entity.Mana.MaxValue;
            ManaBar.value = entity.Mana.Value;
        }
    }
    public void Normal()
    {
        Name.color = NormalColor;
        HealthBar.transform.localScale = new Vector3(1, NormalHeight, 1);
    }

    public void Highlight()
    {
        Name.color = HighlightColor;
        HealthBar.transform.localScale = new Vector3(1, HighlightHeight, 1);
    }

    public void Selected()
    {
        Name.color = SelectionColor;

        HealthBar.transform.localScale = new Vector3(1, SelectionHeight, 1);
    }

}
