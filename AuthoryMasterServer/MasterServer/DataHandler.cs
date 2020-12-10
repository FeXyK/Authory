using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AuthoryMasterServer
{
    /// <summary>
    /// Stores the data on the server.
    /// </summary>
    public class DataHandler
    {
        private static DataHandler _instance;
        public static DataHandler Instance => _instance ??= new DataHandler();

        public int MasterId { get; private set; }

        public Dictionary<int, AuthoryMap> Maps { get; set; }

        public List<Account> OnlineAccounts { get; set; }

        public List<AuthoryNode> Nodes { get; set; }

        public DataHandler()
        {
            OnlineAccounts = new List<Account>();
            Nodes = new List<AuthoryNode>();

            Maps = new Dictionary<int, AuthoryMap>();

            MasterId = 0;
        }

        public void ListNodes()
        {
            foreach (var node in Nodes)
            {
                Console.WriteLine("-----------------------");
                Console.WriteLine(node);
                foreach (var map in node.MapServers)
                {
                    Console.WriteLine(map);
                }
            }
        }

        public int FindLatestPort()
        {
            int port = 0;

            foreach (var node in Nodes)
            {
                foreach (var map in node.MapServers)
                {
                    if (map.Port >= port)
                        port = map.Port;
                }
            }

            return port;
        }

        public Account GetAccountByCharacterName(string characterName)
        {
            return OnlineAccounts.Find(x => x.ConnectedCharacter.Name.ToLower() == characterName.ToLower());
        }

        public Account GetAccount(NetConnection connection)
        {
            return OnlineAccounts.Find(x => x.Connection == connection);
        }

        public Account GetAccount(int accountId)
        {
            return OnlineAccounts.Find(x => x.AccountId == accountId);
        }

        public Account GetAccount(string username)
        {
            return OnlineAccounts.Find(x => x.AccountName == username);
        }

        public void AddNode(AuthoryNode newNode)
        {
            Nodes.Add(newNode);
        }

        public AuthoryMapServer GetMapServer(int reqeustedMapIndex)
        {
            return Maps[reqeustedMapIndex].OnlineChannels.Find(x => x.Load < x.Load);
        }

        public AuthoryNode GetNode(NetConnection senderConnection)
        {
            return Nodes.Find(x => x.NodeMasterConnection == senderConnection);
        }

        public AuthoryMap GetMap(int reqeustedMapIndex)
        {
            return Maps.ContainsKey(reqeustedMapIndex) ? Maps[reqeustedMapIndex] : null;
        }

        public void RemoveNode(AuthoryNode node)
        {
            foreach (var server in node.MapServers)
            {
                server.AuthoryMap.OnlineChannels.Remove(server);
            }
            Nodes.Remove(node);
        }

        public AuthoryNode GetOverloadedNode()
        {
            AuthoryNode returnNode = null;

            foreach (var node in Nodes)
            {
                if (returnNode == null)
                {
                    returnNode = node;
                }
                else
                {
                    if (returnNode.GetOverallLoad() < node.GetOverallLoad())
                    {
                        returnNode = node;
                    }
                }
            }

            return returnNode;
        }
    }
}