using TMPro;
using UnityEngine;

public class MasterUIController : MonoBehaviour
{
    [SerializeField] GameObject LoginScreen = null;
    [SerializeField] GameObject CharacterSelectorScreen = null;

    [SerializeField] CharacterSelector CharacterSelector = null;

    [SerializeField] TMP_InputField UsernameInputField = null;
    [SerializeField] TMP_InputField PasswordInputField = null;

    [SerializeField] TMP_InputField MasterServerAuthStringInputField = null;
    [SerializeField] TMP_InputField MasterServerIPInputField = null;
    [SerializeField] TMP_InputField MasterServerPortInputField = null;

    [SerializeField] TMP_InputField MapServerAuthStringInputField = null;
    [SerializeField] CharacterCreatorController CharacterCreator = null;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public string GetMapServerAuthString()
    {
        return MapServerAuthStringInputField.text.Trim();
    }

    public string GetMasterAuthString()
    {
        return MasterServerAuthStringInputField.text.Trim();
    }

    public string GetMasterIP()
    {
        return MasterServerIPInputField.text.Trim();
    }

    public int GetMasterPort()
    {
        return int.Parse(MasterServerPortInputField.text.Trim());
    }

    public void AddCharacter(string name, byte level, byte modelType, int id)
    {
        CharacterSelector.AddCharacter(new Character(name, level, modelType, id));
    }

    public Character GetSelectedCharacter()
    {
        return CharacterSelector.SelectedCharacter;
    }

    public void SetInteract(bool value)
    {
        CharacterSelector.SetInteract(value);
    }

    public void SetActiveLoginScreen(bool value = true)
    {
        LoginScreen.SetActive(value);
    }

    public void SetActiveCharacterScreen(bool value = true)
    {
        CharacterSelectorScreen.SetActive(value);
    }

    public string GetUsername()
    {
        return UsernameInputField.text.Trim();
    }

    public string GetPassword()
    {
        return PasswordInputField.text;
    }

    public Character GetNewCharacter()
    {
        return CharacterCreator.GetNewCharacter();
    }

    public void ClearCharacterList()
    {
        CharacterSelector.Clear();
    }

    public void HideCharacterCreator()
    {
        CharacterCreator.ShowCreatorMenu(false);
    }
}
