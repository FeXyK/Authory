using System;
using System.Collections.Generic;
using System.Linq;

namespace AuthoryMasterServer
{
    /// <summary>
    /// Defines a map by its index and name and contains the running map server channels for it.
    /// </summary>
    public class AuthoryMap
    {
        public List<AuthoryMapServer> OnlineChannels { get; set; }

        public int MapIndex { get; set; }
        public string MapName { get; set; }

        public AuthoryMap(int mapIndex, string mapName)
        {
            MapIndex = mapIndex;
            MapName = mapName;
            OnlineChannels = new List<AuthoryMapServer>();
        }

        public AuthoryMapServer GetLeastLoadedChannel()
        {
            AuthoryMapServer leastLoaded = null;

            foreach (var map in OnlineChannels)
            {
                if (leastLoaded == null)
                {
                    leastLoaded = map;
                }
                if (leastLoaded.Load < map.Load)
                {
                    leastLoaded = map;
                }

            }
            return leastLoaded;
        }
    }
}

