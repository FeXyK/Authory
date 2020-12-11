using Lidgren.Network;
using Lidgren.Network.Shared;
using System;
using UnityEngine;

public class AuthorySender
{
    public static NetClient Client { get; private set; }
    public static NetClient MasterClient { get; private set; }
    public static AuthoryData Data { get; private set; }

    private static AuthorySender _instance;
    public static AuthorySender Instance
    {
        get
        {
            if (_instance == null)
                _instance = new AuthorySender();

            return _instance;
        }
    }

    public AuthorySender() { }
    public void Set(NetClient client, NetClient masterClient, AuthoryData data)
    {
        MasterClient = masterClient;
        Client = client;
        Data = data;
    }

    public static void SendChannelSwitch(Channel selectedChannel)
    {
        NetOutgoingMessage msgOut = MasterClient.CreateMessage();

        msgOut.Write((byte)MasterMessageType.ChannelSwitchRequest);
        msgOut.Write(selectedChannel.Index);

        MasterClient.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);
    }

    public static void Movement()
    {
        if (Client == null) return;

        NetOutgoingMessage msgOut = Client.CreateMessage();
        msgOut.Write((byte)MessageType.PlayerMovement);

        msgOut.Write(Data.Player.transform.position.x);
        //msgOut.Write(Data.Player.transform.position.y);
        msgOut.Write(Data.Player.transform.position.z);

        Client.SendMessage(msgOut, NetDeliveryMethod.Unreliable);
    }

    public static void SendInteract(Entity target)
    {
        NetOutgoingMessage msgOut = Client.CreateMessage();

        msgOut.Write((byte)MessageType.Interact);
        msgOut.Write(target.Id);

        Client.SendMessage(msgOut, NetDeliveryMethod.Unreliable);
    }

    public static void Action()
    {
        if (Client == null) return;

        NetOutgoingMessage msgOut = Client.CreateMessage();
        msgOut.Write((byte)ActionType.Jump);

        Client.SendMessage(msgOut, NetDeliveryMethod.Unreliable);
    }

    public static void SendChatMessage(string message, MasterMessageType chatMsgType = MasterMessageType.GlobalChat, string receiverName = null)
    {
        if (MasterClient.ServerConnection != null)
        {
            NetOutgoingMessage msgOut = MasterClient.CreateMessage();

            //msgOut.Write(Data.Player.Name);
            msgOut.Write((byte)chatMsgType);
            msgOut.Write(message);

            if (chatMsgType == MasterMessageType.PrivateChat)
            {
                msgOut.Write(receiverName);
            }

            MasterClient.SendMessage(msgOut, NetDeliveryMethod.ReliableUnordered);
        }
    }

    public static void SendSkillRequest(byte skillID, Vector3 targetPosition)
    {
        if (Client == null) return;

        NetOutgoingMessage msgOut = Client.CreateMessage();
        msgOut.Write((byte)MessageType.SkillRequestRawPosition);
        msgOut.Write(skillID, 8);
        msgOut.Write(targetPosition.x);
        msgOut.Write(targetPosition.z);

        Client.SendMessage(msgOut, NetDeliveryMethod.Unreliable);
    }

    public static void SendSkillRequest(byte skillID, ushort targetID)
    {
        if (Client == null) return;

        NetOutgoingMessage msgOut = Client.CreateMessage();
        msgOut.Write((byte)MessageType.SkillRequest);
        msgOut.Write(targetID, 16);
        msgOut.Write(skillID, 8);

        Client.SendMessage(msgOut, NetDeliveryMethod.Unreliable);
    }

    public static void SendRespawn(bool home)
    {
        NetOutgoingMessage msgOut = Client.CreateMessage();

        msgOut.Write((byte)MessageType.Respawn);
        msgOut.Write(home);

        Client.SendMessage(msgOut, NetDeliveryMethod.ReliableUnordered);
    }
}