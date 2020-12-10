using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    [SerializeField] TMP_Dropdown CharacterSelectorDropdown = null; 
    public List<Character> Characters { get; set; }
    public Character SelectedCharacter { get; set; }

    private void Awake()
    {
        Characters = new List<Character>();
    }

    public void AddCharacter(Character character)
    {
        Characters.Add(character);
        ReloadDropdownOptions();
    }

    public void OnSelectionChange()
    {
        SelectedCharacter = Characters[CharacterSelectorDropdown.value];
    }

    public void ReloadDropdownOptions()
    {

        CharacterSelectorDropdown.ClearOptions();
        foreach (var character in Characters)
        {
            CharacterSelectorDropdown.options.Add(new TMP_Dropdown.OptionData(character.Name));
        }

        CharacterSelectorDropdown.SetValueWithoutNotify(0);
        CharacterSelectorDropdown.RefreshShownValue();
        OnSelectionChange();
    }

    public void SetInteract(bool value)
    {
        CharacterSelectorDropdown.interactable = value;
    }

    public void Clear()
    {
        Characters.Clear();
        CharacterSelectorDropdown.ClearOptions();
        CharacterSelectorDropdown.RefreshShownValue();
        SelectedCharacter = null;
    }
}