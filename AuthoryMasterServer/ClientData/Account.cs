using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AuthoryMasterServer
{
    public class Account
    {
        public NetConnection Connection { get; set; }
        public AuthoryMapServer ConnectedServerMap { get; set; }
        public Character ConnectedCharacter { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }

        public int MapIndex { get; set; }
        public int RequestedChannelId { get; set; }

        public bool ConnectionApproved { get; set; }

        public List<Character> Characters { get; set; }

        public Account(int accountId, string accountName, NetConnection connection = null)
        {
            Characters = new List<Character>();
            this.AccountId = accountId;
            this.Connection = connection;
            this.AccountName = accountName;
        }

        public Character GetCharacter(int requestedCharacterId)
        {
            return Characters.Single(x => x.CharacterId == requestedCharacterId);
        }

        public void SetConnectedCharacter(int characterId)
        {
            ConnectedCharacter = Characters.Single(x => x.CharacterId == characterId);
        }

        public override string ToString()
        {
            return string.Format($"MasterId: {AccountId}\nName: {AccountName}\nConnectedServer{(ConnectedServerMap == null ? "Not connected" : ConnectedServerMap.ToString())}");
        }
    }
}

