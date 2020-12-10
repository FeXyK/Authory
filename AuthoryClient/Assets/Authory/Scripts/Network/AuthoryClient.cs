using Lidgren.Network;
using UnityEngine;
using MessageType = Lidgren.Network.Shared.MessageType;

public class AuthoryClient
{
    public static NetClient Client { get; private set; }
    public static NetClient MasterClient { get; private set; }
    public AuthoryHandler Handler { get; private set; }
    public AuthoryData Data { get; private set; }
    public AuthorySender Sender { get; private set; }

    public UIController UIController { get; private set; }

    public AuthoryClient(string connName)
    {
        UIController = GameObject.FindObjectOfType<UIController>();
        NetPeerConfiguration config = new NetPeerConfiguration(connName);
        config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

        MasterClient = GameObject.FindObjectOfType<MasterClientManager>().MasterClient;
        Client = new NetClient(config);

        Client.Start();

        Data = AuthoryData.Instance;
        Handler = new AuthoryHandler(Data);
        Sender = AuthorySender.Instance;
        Sender.Set(Client, MasterClient, Data);
    }

    public void Connect(string ip, int port, long uid)
    {
        NetOutgoingMessage msgOut = Client.CreateMessage();
        msgOut.Write(uid);

        Client.Connect(ip, port, msgOut);
    }

    public void Read()
    {
        MessageType msgType;
        NetIncomingMessage msgIn;
        while ((msgIn = Client.ReadMessage()) != null)
        {
            if (msgIn.MessageType == NetIncomingMessageType.Data)
            {
                msgType = (MessageType)msgIn.ReadByte();
                //Debug.Log(msgType);
                switch (msgType)
                {
                    case MessageType.Disconnect:
                        Handler.Disconnect(msgIn);
                        break;
                    case MessageType.MobRespawn:
                        Handler.MobRespawn(msgIn);
                        break;
                    case MessageType.Respawn:
                        Handler.RespawnPlayer(msgIn);
                        break;
                    case MessageType.PlayerMovement:
                        Handler.Movement(msgIn);
                        break;
                    case MessageType.MovementSpeed:
                        Handler.MovementSpeedChange(msgIn);
                        break;
                    case MessageType.MobUpdate:
                        Handler.MobUpdate(msgIn);
                        break;
                    case MessageType.PlayerUpdate:
                        Handler.PlayerUpdate(msgIn);
                        break;
                    case MessageType.AttributeUpdate:
                        Handler.AttributeUpdate(msgIn);
                        break;
                    case MessageType.FullEntityUpdate:
                        Handler.FullEntityUpdate(msgIn);
                        break;
                    case MessageType.GridFullEntityUpdate:
                        Handler.GridFullEntityUpdate(msgIn);
                        break;
                    case MessageType.GridResourceEntityFullUpdate:
                        Handler.GridFullResourceEntityUpdate(msgIn);
                        break;
                    case MessageType.SkillInfo:
                        Handler.Skill(msgIn);
                        break;
                    case MessageType.SkillInfoAlternativePosition:
                        Handler.SkillAlternativePosition(msgIn);
                        break;
                    case MessageType.SkillInterrupt:
                        Handler.SkillInterrupted(msgIn);
                        break;
                    case MessageType.DamageInfo:
                        Handler.DamageInfo(msgIn);
                        break;
                    case MessageType.UpdateExperience:
                        Handler.UpdateExperience(msgIn);
                        break;
                    case MessageType.UpdateMaxExperience:
                        Handler.UpdateMaxExperience(msgIn);
                        break;
                    case MessageType.MobMovementToPosition:
                        Handler.MobPathTo(msgIn);
                        break;
                    case MessageType.Teleport:
                        Handler.Teleport(msgIn);
                        break;
                    case MessageType.PositionCorrection:
                        Handler.PositionCorrection(msgIn);
                        break;
                    case MessageType.SystemInfo:
                        Handler.SystemInfo(msgIn);
                        break;
                    case MessageType.BuffApply:
                        Handler.BuffApply(msgIn);
                        break;
                    case MessageType.BuffRefresh:
                        Handler.BuffRefresh(msgIn);
                        break;
                    case MessageType.BuffRemove:
                        Handler.BuffRemove(msgIn);
                        break;
                    case MessageType.Death:
                        Handler.Death(msgIn);
                        break;
                    case MessageType.LevelUp:
                        Handler.LevelUp(msgIn);
                        break;
                }
            }
            Client.Recycle(msgIn);
        }
    }
}
