using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChannelSelector : MonoBehaviour
{
    [SerializeField]  TMP_Dropdown ChannelSelectorDropdown = null;
    public List<Channel> Channels { get; set; }
    public Channel SelectedChannel { get; set; }

    private void Awake()
    {
        Channels = new List<Channel>();
    }

    public void AddChannel(Channel channel)
    {
        Channels.Add(channel);
        ReloadDropdownOptions();
    }

    public void OnSelectionChange()
    {
        SelectedChannel = Channels[ChannelSelectorDropdown.value];
        AuthorySender.SendChannelSwitch(SelectedChannel);

        Debug.Log("CHANNEL SELECTED");
    }

    public void ReloadDropdownOptions()
    {
        ChannelSelectorDropdown.options.Clear();

        foreach (var channel in Channels)
        {
            ChannelSelectorDropdown.options.Add(new TMP_Dropdown.OptionData(channel.MapIndex + ": Ch " + channel.Index));
        }


        ChannelSelectorDropdown.SetValueWithoutNotify(0);
        ChannelSelectorDropdown.RefreshShownValue();
    }

    public void SetInteract(bool value)
    {
        ChannelSelectorDropdown.interactable = value;
    }
}