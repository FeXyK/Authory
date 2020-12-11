using Lidgren.Network;
using Lidgren.Network.Shared;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the UI elements in the Login and Character screen.
/// </summary>
public class MasterClientManager : MonoBehaviour
{

    [SerializeField] MasterUIController MasterUIController = null;
    public UIController UIController { get; set; }

    public AuthoryClient AuthoryClient { get; set; }

    public NetClient MasterClient { get; set; }
    public NetPeerConfiguration MasterConfig { get; set; }


    public int MapServerPort { get; set; }
    public long MapServerUID { get; set; }
    public string MapServerIP { get; set; }

    public List<Channel> Channels { get; set; }

    float nextTryTime;
    float tryDelay = 2f;

    static MasterClientManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        DontDestroyOnLoad(this);
        Channels = new List<Channel>();
    }

    void Update()
    {
        ReadMaster();
    }

    public void SendLogin()
    {
        if (nextTryTime < Time.time)
        {
            InitMaster(MasterUIController.GetMasterAuthString());

            ConnectMaster(MasterUIController.GetMasterIP(), MasterUIController.GetMasterPort(), MasterUIController.GetUsername(), MasterUIController.GetPassword());
            nextTryTime = Time.time + tryDelay;
        }
    }

    public void SendLogout()
    {
        MasterClient.Disconnect("Logout");
        MasterUIController.SetActiveLoginScreen(true);
    }

    public void SendCreateCharacter()
    {
        NetOutgoingMessage msgOut = MasterClient.CreateMessage();

        msgOut.Write((byte)MasterMessageType.CreateCharacter);
        Character character = MasterUIController.GetNewCharacter();
        msgOut.Write(character.Name);
        msgOut.Write(character.ModelType);

        MasterClient.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);

        MasterUIController.HideCharacterCreator();
    }

    public void SendDeleteCharacter()
    {
        NetOutgoingMessage msgOut = MasterClient.CreateMessage();

        msgOut.Write((byte)MasterMessageType.DeleteCharacter);
        msgOut.Write(MasterUIController.GetSelectedCharacter().Id);
        msgOut.Write(MasterUIController.GetSelectedCharacter().Name);

        MasterClient.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);
    }

    private void ReadMaster()
    {
        if (MasterClient == null) return;

        MasterMessageType msgType;
        NetIncomingMessage msgIn;

        while ((msgIn = MasterClient.ReadMessage()) != null)
        {
            if (msgIn.MessageType == NetIncomingMessageType.Data)
            {
                msgType = (MasterMessageType)msgIn.ReadByte();
                switch (msgType)
                {
                    case MasterMessageType.GlobalChat:
                        AuthoryClient.Handler.ChatMessage(msgIn, msgType);
                        break;
                    case MasterMessageType.WorldChat:
                        AuthoryClient.Handler.ChatMessage(msgIn, msgType);
                        break;
                    case MasterMessageType.PrivateChat:
                        AuthoryClient.Handler.ChatMessage(msgIn, msgType);
                        break;
                    case MasterMessageType.ChannelInfo:
                        if (AuthoryClient != null)
                            AuthoryClient.Handler.ChannelInfo(msgIn);
                        else
                        {
                            int count = msgIn.ReadInt32();

                            for (int i = 0; i < count; i++)
                            {
                                Channel channel = new Channel()
                                {
                                    Name = msgIn.ReadString(),
                                    Index = msgIn.ReadInt32(),
                                    IP = msgIn.ReadString(),
                                    Port = msgIn.ReadInt32()
                                };

                                Channels.Add(channel);
                                Debug.Log("ADDING CHANNEL: " + channel.Index);
                            }
                        }
                        break;
                    case MasterMessageType.RefreshCharacterList:
                        {
                            RefreshCharacterList(msgIn);
                        }
                        break;
                    case MasterMessageType.NewAccountConnection:
                        {
                            MasterUIController.SetActiveCharacterScreen(true);
                            nextTryTime = Time.time;
                            RefreshCharacterList(msgIn);

                            Debug.Log("CONNECTION REQUEST SENT");
                        }
                        break;
                    case MasterMessageType.ConnectionApproved:
                        {
                            MapServerUID = msgIn.ReadInt64();
                            MapServerIP = msgIn.ReadString();
                            MapServerPort = msgIn.ReadInt32();
                            int MapIndex = msgIn.ReadInt32();
                            Debug.Log($"MapServerUID: {MapServerUID}");
                            Debug.Log($"MapServerIP: {MapServerIP}");
                            Debug.Log($"MapServerPort: {MapServerPort}");

                            LoadMap(MapIndex);
                        }
                        break;
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (MasterClient != null && MasterClient.ServerConnection != null && MasterClient.ServerConnection.Status != NetConnectionStatus.Disconnected)
            MasterClient.Disconnect("Quit");
    }

    public void RefreshCharacterList(NetIncomingMessage msgIn)
    {
        MasterUIController.ClearCharacterList();

        int countCharacter = msgIn.ReadInt32();
        for (int i = 0; i < countCharacter; i++)
        {
            Debug.Log("Character: " + i);
            string Name = msgIn.ReadString() + ": ";
            byte Level = msgIn.ReadByte();
            byte ModelType = msgIn.ReadByte();
            int Id = msgIn.ReadInt32();

            MasterUIController.AddCharacter(Name, Level, ModelType, Id);
        }
    }

    public void LoadMap(int mapIndex)
    {
        Debug.Log("LOADING MAP :" + mapIndex + 1);
        if (AuthoryClient.Client != null)
        {
            AuthoryClient.Client.Disconnect("Bye");
            AuthoryData.Instance.Clear();
        }
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadScene(mapIndex + 1);
    }

    public void InitMaster(string masterConnName)
    {
        this.MasterConfig = new NetPeerConfiguration(masterConnName);
        this.MasterConfig.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

        this.MasterClient = new NetClient(MasterConfig);

        this.MasterClient.Start();
    }

    public void ConnectMaster(string masterIP, int masterPort, string username, string password)
    {
        if (MasterClient.ServerConnection != null)
            MasterClient.Disconnect("Reconnect");

        NetOutgoingMessage msgOut = MasterClient.CreateMessage();

        msgOut.Write((byte)MasterMessageType.NewAccountConnection);
        msgOut.Write(username);
        msgOut.Write(password);

        MasterClient.Connect(masterIP, masterPort, msgOut);
    }

    public void SendConnectionRequest()
    {
        if (nextTryTime > Time.time) return;

        if (MasterUIController.GetSelectedCharacter() == null) return;

        nextTryTime = Time.time + tryDelay;

        MasterUIController.gameObject.SetActive(false);
        MasterUIController.SetInteract(false);

        Character selectedCharacter = MasterUIController.GetSelectedCharacter();

        NetOutgoingMessage msgOut = MasterClient.CreateMessage();

        msgOut.Write((byte)MasterMessageType.ServerConnectionRequest);

        if (MasterUIController == null)

            Debug.LogError("Master ui is null");

        msgOut.Write(selectedCharacter.Id);


        MasterClient.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);
    }

    public void SendChannelSwitchRequest()
    {
        if (nextTryTime > Time.time) return;

        nextTryTime = Time.time + tryDelay;

        Character selectedCharacter = MasterUIController.GetSelectedCharacter();

        NetOutgoingMessage msgOut = MasterClient.CreateMessage();

        msgOut.Write((byte)MasterMessageType.ChannelSwitchRequest);

        if (MasterUIController == null)
            Debug.LogError("Master ui is null");

        msgOut.Write(selectedCharacter.Id);
        msgOut.Write(UIController.GetChannelSelector().SelectedChannel.Index);


        MasterClient.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);
    }

    private void OnLevelWasLoaded(int level)
    {
        Debug.Log($"Level loaded: {level}");
        if (level != 0)
        {
            AuthoryClient = GameObject.FindObjectOfType<ClientManager>().AuthoryClient;
            UIController = GameObject.FindObjectOfType<UIController>();
        }
        else
        {
            this.gameObject.SetActive(true);
            MasterUIController.gameObject.SetActive(true);
            MasterUIController.SetActiveLoginScreen();
        }
    }

    public string GetMapServerAuthString()
    {
        return MasterUIController.GetMapServerAuthString();
    }
}
