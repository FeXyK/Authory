using Assets.Authory.Scripts;
using Lidgren.Network.Shared;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles the UI elements in the game view.
/// </summary>
public class UIController : MonoBehaviour
{
    [SerializeField] TMP_Text PingText = null;

    //Experience
    [SerializeField] TMP_Text RawExperience = null;
    [SerializeField] TMP_Text PercentageExperience = null;
    [SerializeField] Slider ExperienceBar = null;

    //Channels
    [SerializeField] ChannelSelector ChannelSelector = null;

    //Chat
    [SerializeField] TMP_Text[] ChatWindows = null;
    [SerializeField] TMP_InputField ChatType = null;
    [SerializeField] TMP_InputField PrivateMessageRecepient = null;
    [SerializeField] ScrollRect ChatScrollRect = null;
    [SerializeField] TMP_Dropdown ChatMessageTypeDropdown = null;
    private MasterMessageType ChatMessageType;

    //DeathScreen
    [SerializeField] GameObject GameOverPanel = null;
    [SerializeField] Button RespawnNearest = null;
    [SerializeField] Button RespawnHome = null;

    //GameMenu
    [SerializeField] GameObject GameMenuPanel = null;
    [SerializeField] GameObject OptionsPanel = null;


    //Combat
    [SerializeField] public SkillBarController SkillBarController = null;
    [SerializeField] public ChannelingController ChannelingController = null;
    [SerializeField] public SelectedTargetInfoController SelectedTargetInfoController = null;

    public PlayerEntity Player { get; set; }
    public bool IsActive { get; private set; }

    long maxExperience = 1;
    string privateChatReceiver = "";

    float chatLastActiveDelay = 0.1f;
    float maxChatLastActiveDelay = 0.1f;


    void Update()
    {
        IsActive = ChatType.isFocused || PrivateMessageRecepient.isFocused;

        chatLastActiveDelay -= Time.deltaTime;

        if (Player != null)
        {
            if (AuthoryClient.Client.ServerConnection != null)
                if (AuthoryClient.Client.ServerConnection.Status == Lidgren.Network.NetConnectionStatus.Connected)
                {
                    int ping = (int)(AuthoryClient.Client.ServerConnection.AverageRoundtripTime * 1000f);
                    PingText.text = ping + " ms";
                    PingText.color = new Color((255f - (ping + 50)) / 255f, 1f, 0f, 1f);
                }
                else
                {
                    PingText.text = AuthoryClient.Client.ServerConnection.Status.ToString();
                }
            if (chatLastActiveDelay < 0)
                if (Input.GetKeyDown(KeyCode.Return))
                    if (!ChatType.isFocused)
                    {
                        ChatType.Select();
                    }
        }
        Vector2 localMousePosition = ChatWindows[0].rectTransform.InverseTransformPoint(Input.mousePosition);
        if (!ChatWindows[0].rectTransform.rect.Contains(localMousePosition))
        {
            ScrollDown();
        }

        GameOverPanel.SetActive(Player.Dead);
    }

    public void UpdateExperience(long newExptTick, long experience)
    {

        SystemMessage($"Gained {experience - Player.Experience} Exp");
        Player.Experience = experience;
        RawExperience.text = string.Format($"{Player.Experience} / {this.maxExperience}");
        PercentageExperience.text = string.Format($"{((double)Player.Experience / (double)this.maxExperience) * 100.0f}%");
        ExperienceBar.value = Player.Experience;
    }
    public void UpdateMaxExperience(long maxExperience, long experience, int level = 0)
    {
        if (maxExperience != 0)
        {
            Player.Experience = experience;
            this.maxExperience = maxExperience;
            ExperienceBar.maxValue = maxExperience;
            ExperienceBar.value = experience;
            RawExperience.text = string.Format($"{Player.Experience} / {this.maxExperience}");
            PercentageExperience.text = string.Format($"{((double)Player.Experience / (double)this.maxExperience) * 100.0f}%");
        }
        if (level != 0)
            SystemMessage($"<color=#FFF000>Congratulations! Level up to Lv.{level}</color>");
    }

    public void IncomingChatMessage(MasterMessageType msgType, string username, string message, string date = "")
    {
        if (date.Length == 0)
            date = DateTime.Now.ToString("HH:mm");


        switch (msgType)
        {
            case MasterMessageType.GlobalChat:
                ChatWindows[0].text += string.Format($"\n{date} <color=#E0B426>[Global]\t {username}: {message}</color>");
                break;
            case MasterMessageType.WorldChat:
                ChatWindows[0].text += string.Format($"\n{date} <color=#E06A10>[World]\t {username}: {message}</color>");
                break;
            case MasterMessageType.PrivateChat:
                if (username.ToLower() == Player.Name.ToLower())
                    ChatWindows[0].text += string.Format($"\n{date} <color=#E01FDA>[Private]\t To: {privateChatReceiver}: {message}</color>");
                else
                    ChatWindows[0].text += string.Format($"\n{date} <color=#E01FDA>[Private]\t From: {username}: {message}</color>");
                break;
        }
    }

    public void WriteSentPrivateMessageToChat(string username, string message)
    {
        ChatWindows[0].text += string.Format($"\n{DateTime.Now.ToString("HH:mm")} <color=#E01FDA>[Private]\t To: {privateChatReceiver}: {message}</color>");
    }

    public void ChatSendMessage()
    {
        if (ChatType.text.Length > 0)
        {
            string message = ChatType.text;
            ChatType.text = "";
            if (ChatMessageType == MasterMessageType.PrivateChat)
            {
                if (privateChatReceiver.Length > 1)
                {
                    AuthorySender.SendChatMessage(message, ChatMessageType, privateChatReceiver);
                }
            }
            else
                AuthorySender.SendChatMessage(message, ChatMessageType);
        }
        if (!EventSystem.current.alreadySelecting)
        {
            EventSystem.current.SetSelectedGameObject(null);
            ChatType.DeactivateInputField();
        }
        chatLastActiveDelay = maxChatLastActiveDelay;
    }

    public void ChatOnValueChanged()
    {
        string text;
        if (ChatType.text.Length > 2)
        {
            text = ChatType.text;

            if (text[0] == '/' && text[2] == ' ')
            {
                switch (text.ToLower()[1])
                {
                    case 'p':
                        ChatMessageType = MasterMessageType.PrivateChat;
                        ChatType.text = "";
                        ActivatePrivateChatLayout();
                        break;
                    case 'g':
                        DisablePrivateChatLayout();
                        ChatMessageType = MasterMessageType.GlobalChat;
                        ChatType.text = "";
                        break;
                    case 'w':
                        DisablePrivateChatLayout();
                        ChatMessageType = MasterMessageType.WorldChat;
                        ChatType.text = "";
                        break;
                }
            }
            ChatMessageTypeDropdown.value = (int)ChatMessageType;
        }
    }

    public ChannelSelector GetChannelSelector()
    {
        return ChannelSelector;
    }

    public void ChatOnPrivateReceiverSelected()
    {
        PrivateMessageRecepient.text = privateChatReceiver;
    }

    public void ChatOnPrivateReceiverNameChanged()
    {
        privateChatReceiver = PrivateMessageRecepient.text.Replace("To: ", "");
        PrivateMessageRecepient.text = "To: " + privateChatReceiver;

        ChatType.Select();
    }

    private void ActivatePrivateChatLayout()
    {
        PrivateMessageRecepient.gameObject.SetActive(true);
        PrivateMessageRecepient.Select();
    }

    private void DisablePrivateChatLayout()
    {
        PrivateMessageRecepient.gameObject.SetActive(false);
    }

    public void ChatSelectMessageType()
    {
        ChatMessageType = (MasterMessageType)ChatMessageTypeDropdown.value;

        if (ChatMessageType == MasterMessageType.PrivateChat)
        {
            ActivatePrivateChatLayout();
        }
        else
        {
            DisablePrivateChatLayout();
            ChatType.Select();
        }
    }

    public void SystemMessage(string message)
    {
        ChatWindows[0].text += string.Format($"\n<color=#FF0000>{GetCurrentTime()} [System]: {message}</color>");
    }

    public void ScrollDown()
    {
        ChatScrollRect.verticalNormalizedPosition = 0;
    }

    private string GetCurrentTime()
    {
        return DateTime.Now.ToString("HH:mm");
    }

    public void SendRespawn(bool home = false)
    {
        AuthorySender.SendRespawn(home);
    }

    public void Resume()
    {
        OptionsPanel.SetActive(false);
        GameMenuPanel.SetActive(false);
    }

    public void Options()
    {
        OptionsPanel.SetActive(true);
        GameMenuPanel.SetActive(false);
    }

    public void Quit()
    {
        AuthorySender.Disconnect();

        Application.Quit();
    }
}