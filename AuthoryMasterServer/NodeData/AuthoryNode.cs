using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AuthoryMasterServer
{
    public class AuthoryNode
    {
        public int MaxLoad { get; set; } = 1000;

        public List<AuthoryMapServer> MapServers { get; set; }

        public NetConnection NodeMasterConnection { get; set; }


        public long NodeMasterUID { get; set; }
        public string NodeMasterIP { get; set; }
        public int NodeMasterPort { get; set; }


        public AuthoryNode(NetConnection masterConnection, string ip, int port)
        {
            this.MapServers = new List<AuthoryMapServer>();
            this.NodeMasterConnection = masterConnection;
            this.NodeMasterUID = masterConnection.RemoteUniqueIdentifier;

            this.NodeMasterIP = ip;
            this.NodeMasterPort = port;

        }

        public void AddMapServer(AuthoryMapServer mapServer)
        {
            MapServers.Add(mapServer);
            Console.WriteLine($"Added MapServer:\n{mapServer} to Node:\n{this}");
        }

        public void RemoveMapServer(int mapPort, int mapIndex, string mapName)
        {
            for (int i = MapServers.Count - 1; i >= 0; i--)
            {
                AuthoryMapServer mapServer = MapServers[i];
                if (mapServer.Port == mapPort && mapServer.AuthoryMap.MapIndex == mapIndex)
                {
                    MapServers.RemoveAt(i);
                    Console.WriteLine($"Removed MapServer:\n{mapServer} from Node:\n{this}");
                    return;
                }
            }
        }

        public void ListMapServers()
        {
            foreach (var mapServer in MapServers)
            {
                Console.WriteLine(mapServer);
            }
        }

        public AuthoryMapServer GetMapServerByPort(int port)
        {
            return MapServers.Find(x => x.Port == port);
        }

        public int GetOverallLoad()
        {
            int load = 0;
            foreach (var map in MapServers)
            {
                load += map.Load;
            }

            return load;
        }

        public AuthoryMapServer GetOverloadedMap()
        {
            AuthoryMapServer mostLoaded = null;

            foreach (var map in MapServers)
            {
                if (mostLoaded == null)
                    mostLoaded = map;
                else if (mostLoaded.Load < map.Load)
                {
                    mostLoaded = map;
                }
            }

            Console.WriteLine(mostLoaded.Load);
            Console.WriteLine(mostLoaded.Load);
            Console.WriteLine(mostLoaded.Load);
            Console.WriteLine(mostLoaded.Load);
            Console.WriteLine(mostLoaded.AuthoryMap.MapIndex);
            Console.WriteLine(mostLoaded.AuthoryMap.MapIndex);
            Console.WriteLine(mostLoaded.AuthoryMap.MapIndex);
            Console.WriteLine(mostLoaded.AuthoryMap.MapIndex);
            Console.WriteLine(mostLoaded.AuthoryMap.MapIndex);
            Console.WriteLine(mostLoaded.AuthoryMap.MapIndex);
            Console.WriteLine(mostLoaded.AuthoryMap.MapIndex);
            Console.WriteLine(mostLoaded.AuthoryMap.MapIndex);

            return mostLoaded;
        }

        public List<AuthoryMapServer> GetMapsByMapIndex(int mapIndex)
        {
            List<AuthoryMapServer> mapsByIndex = new List<AuthoryMapServer>();
            foreach (var map in MapServers)
            {
                if (map.AuthoryMap.MapIndex == mapIndex)
                {
                    mapsByIndex.Add(map);
                }
            }

            return mapsByIndex;
        }

        public override string ToString()
        {
            return string.Format($"Uid: {NodeMasterUID}\nAt: {NodeMasterIP}:{NodeMasterPort}");
        }
    }
}