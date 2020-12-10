using Lidgren.Network;
using Lidgren.Network.Shared;
using System.Collections.Generic;

namespace AuthoryMasterServer
{
    /// <summary>
    /// Containst a specific MapServer that is running under a node.
    /// </summary>
    public class AuthoryMapServer
    {
        public AuthoryMap AuthoryMap { get; set; }
        public AuthoryNode MasterNode { get; set; }
        public List<Character> OnlineCharacters { get; set; }

        public string IP { get; set; }
        public int Port { get; set; }

        /// <summary>
        /// Players on the server, gets refreshed by ReportLoad messages from the node .
        /// </summary>
        public int Load { get; set; }

        public AuthoryMapServer(AuthoryNode masterNode, int mapPort, AuthoryMap map)
        {
            OnlineCharacters = new List<Character>();

            this.MasterNode = masterNode;

            this.IP = masterNode.NodeMasterIP;
            this.Port = mapPort;


            this.AuthoryMap = map;
        }

        /// <summary>
        /// Sends new connection request from account to the map server's node. 
        /// </summary>
        public void SendNewCharacterInfo(Character character)
        {
            OutgoingMessageHandler.Instance.SendCharacterInfo(character, this);
        }

        public void SendMessageToConnectedPlayers(Account messageFrom, MasterMessageType messageType, string messageContent)
        {
            OutgoingMessageHandler.Instance.SendChatMessage(messageFrom, messageType, messageContent, OnlineCharacters);
        }

        public Character GetCharacter(string receiverName)
        {
            foreach (var character in OnlineCharacters)
            {
                if (character.Name == receiverName)
                {
                    return character;
                }
            }

            return null;
        }

        public Character GetCharacter(int characterId)
        {
            foreach (var character in OnlineCharacters)
            {
                if (character.CharacterId == characterId)
                {
                    return character;
                }
            }

            return null;
        }

        public override string ToString()
        {
            return string.Format($"Map Name: {AuthoryMap.MapName}\n{IP}:{Port}\nMapIndex: {AuthoryMap.MapIndex}\n{OnlineCharacters.Count}\nCurrent Load: {Load}");
        }
    }
}

