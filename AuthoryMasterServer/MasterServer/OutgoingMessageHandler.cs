using Lidgren.Network;
using Lidgren.Network.Shared;
using System;
using System.Collections.Generic;

namespace AuthoryMasterServer
{
    public class OutgoingMessageHandler
    {
        private NetServer Server;

        private static OutgoingMessageHandler _instance;
        public static OutgoingMessageHandler Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new OutgoingMessageHandler();
                return _instance;
            }
        }

        public OutgoingMessageHandler Init(NetServer netServer)
        {
            this.Server = netServer;
            return Instance;
        }

        /// <summary>
        /// Sends map request for the given node.
        /// </summary>
        /// <param name="nodeMasterConnection">The conection of the node</param>
        /// <param name="map">The map that has to be started</param>
        /// <param name="latestPort">MapServer port will start on this port (need for localhost testing)</param>
        public void SendMapsRequest(NetConnection nodeMasterConnection, AuthoryMap map, int latestPort)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MasterMessageType.RequestMap);

            msgOut.Write(latestPort);

            msgOut.Write(map.MapIndex);
            msgOut.Write(map.MapName);


            Server.SendMessage(msgOut, nodeMasterConnection, NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Sends map requests for the given node.
        /// </summary>
        /// <param name="nodeMasterConnection">The conection of the node</param>
        /// <param name="map">The map that has to be started</param>
        /// <param name="latestPort">MapServer port will start on this port (need for localhost testing)</param>
        public void SendMapsRequest(NetConnection nodeMasterConnection, AuthoryMap[] maps, int latestPort)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MasterMessageType.RequestMaps);

            msgOut.Write(latestPort);

            msgOut.Write(maps.Length);
            foreach (var map in maps)
            {
                msgOut.Write(map.MapIndex);
                msgOut.Write(map.MapName);
            }
            Server.SendMessage(msgOut, nodeMasterConnection, NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Sends shutduwn request for the given node for the specific map.
        /// </summary>
        /// <param name="nodeMasterConnection">Connection of the node</param>
        /// <param name="map">Map server to be shutduwn</param>
        public void SendMapsRemoveRequest(NetConnection nodeMasterConnection, AuthoryMap map)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MasterMessageType.RemoveMap);

            msgOut.Write(map.MapIndex);

            Server.SendMessage(msgOut, nodeMasterConnection, NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Sends shutduwn requests for the given node for the specific map.
        /// </summary>
        /// <param name="nodeMasterConnection">Connection of the node</param>
        /// <param name="map">Map server to be shutduwn</param>
        public void SendMapsRemoveRequest(NetConnection nodeMasterConnection, AuthoryMap[] maps)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MasterMessageType.RemoveMaps);

            foreach (var map in maps)
            {
                msgOut.Write(map.MapIndex);
                msgOut.Write(map.MapName);
            }
            Server.SendMessage(msgOut, nodeMasterConnection, NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Sends chat message to the receivers
        /// </summary>
        public void SendChatMessage(Account messageFrom, MasterMessageType messageType, string messageContent, List<Character> receivers)
        {
            NetOutgoingMessage msgOut;

            foreach (var receiver in receivers)
            {
                msgOut = Server.CreateMessage();

                msgOut.Write((byte)messageType);

                msgOut.Write(messageFrom.ConnectedCharacter.Name);
                msgOut.Write(messageContent);

                Server.SendMessage(msgOut, receiver.Account.Connection, NetDeliveryMethod.ReliableOrdered);
            }
        }

        /// <summary>
        /// Sends private chat message
        /// </summary>
        public void SendPrivateChatMessage(Account messageFrom, string messageContent, Account receiver)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MasterMessageType.PrivateChat);

            msgOut.Write(messageFrom.ConnectedCharacter.Name);
            msgOut.Write(messageContent);

            Server.SendMessage(msgOut, receiver.Connection, NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Sends character info for the given map server.
        /// </summary>
        public void SendCharacterInfo(Character character, AuthoryMapServer mapServer)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            //NodeMaster will send the message for the map server with this port

            msgOut.Write((byte)MasterMessageType.CharacterInfo);

            msgOut.Write(mapServer.Port);
            msgOut.Write(character.Account.AccountName);//string
            msgOut.Write(character.Name);//string

            msgOut.Write(character.Account.AccountId);//int
            msgOut.Write(character.CharacterId);//int

            msgOut.Write(character.Health);//int
            msgOut.Write(character.Mana);//int

            msgOut.Write(character.Experience);//int
            msgOut.Write(character.Level);//byte
            msgOut.Write(character.ModelType);//byte

            msgOut.Write(character.PositionX);//float
            msgOut.Write(character.PositionZ);//float

            msgOut.Write(character.Account.Connection.RemoteUniqueIdentifier);//long

            Server.SendMessage(msgOut, mapServer.MasterNode.NodeMasterConnection, NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Sends connection approved by the map server message to the client. 
        /// Contains the map servers information for connection.
        /// </summary>
        public void SendConnectionApproved(Account account, AuthoryMapServer mapServer)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MasterMessageType.ConnectionApproved);

            msgOut.Write(account.Connection.RemoteUniqueIdentifier);
            msgOut.Write(mapServer.IP);
            msgOut.Write(mapServer.Port);
            msgOut.Write(mapServer.AuthoryMap.MapIndex);
            System.Console.WriteLine("Conn details sent to client");
            Server.SendMessage(msgOut, account.Connection, NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Sends MasterServer System information that the client can convert into a predefined text.
        /// </summary>
        public void SendInfo(Account account, SystemMessageType systemMessage)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MasterMessageType.Information);
            msgOut.Write((byte)systemMessage);

            account.Connection.SendMessage(msgOut, NetDeliveryMethod.Unreliable, 0);
        }

        /// <summary>
        /// Requests character information from the given map servers's node.
        /// </summary>
        /// <param name="connectedCharacter"></param>
        /// <param name="connectedServerMap"></param>
        public void RequestCharacterInfo(Character connectedCharacter, AuthoryMapServer connectedServerMap)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MasterMessageType.RequestCharacterInfo);
            msgOut.Write(connectedServerMap.Port);

            msgOut.Write(connectedCharacter.CharacterId);

            connectedServerMap.MasterNode.NodeMasterConnection.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered, 0);
        }

        /// <summary>
        /// Sends character list to the account's connection.
        /// </summary>
        /// <param name="account"></param>
        public void SendCharacterListRefresh(Account account)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();
            msgOut.Write((byte)MasterMessageType.RefreshCharacterList);
            msgOut.Write(CreateCharacterListData(account));

            account.Connection.SendMessage(msgOut, NetDeliveryMethod.ReliableUnordered, 0);
        }

        /// <summary>
        /// Send after successfull login, triggers the Character selection screen on the client, and contains the characters of the account.
        /// </summary>
        /// <param name="account">Connected account</param>
        public void SendNewConnectionMessage(Account account)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();
            msgOut.Write((byte)MasterMessageType.NewAccountConnection);

            msgOut.Write(CreateCharacterListData(account));

            account.Connection.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered, 0);
        }

        /// <summary>
        /// Writes the informations of the account's characters
        /// </summary>
        /// <param name="account"></param>
        /// <returns>NetBuffer that has the characters data serialized</returns>
        public NetBuffer CreateCharacterListData(Account account)
        {
            NetBuffer buffer = new NetBuffer();
            buffer.Write(account.Characters.Count);
            foreach (var character in account.Characters)
            {
                buffer.Write(character.Name);
                buffer.Write(character.Level);
                buffer.Write(character.ModelType);
                buffer.Write(character.CharacterId);
            }
            return buffer;
        }
    }
}