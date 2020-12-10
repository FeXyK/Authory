using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreatorController : MonoBehaviour
{
    [SerializeField] TMP_InputField NewCharacterName = null;
    [SerializeField] TMP_Dropdown NewCharacterModelType = null;
    [SerializeField] Button CreateButton = null;
    [SerializeField] Button CancelButton = null;

    public void ShowCreatorMenu(bool value)
    {
        NewCharacterName.text = "";
        NewCharacterModelType.value = 0;

        NewCharacterName.gameObject.SetActive(value);
        NewCharacterModelType.gameObject.SetActive(value);
        CreateButton.gameObject.SetActive(value);
        CancelButton.gameObject.SetActive(value);
    }

    public void Show()
    {
        ShowCreatorMenu(true);
    }

    public void Cancel()
    {
        ShowCreatorMenu(false);
    }

    public Character GetNewCharacter()
    {
        return new Character()
        {
            Name = NewCharacterName.text.Trim(),
            ModelType = (byte)NewCharacterModelType.value,
        };
    }
}
